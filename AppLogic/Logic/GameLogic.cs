﻿using AppLogic.Models;
using AppLogic.Enums;
using AppLogic.DataAccess;
using System.Reflection.Metadata.Ecma335;
using static System.Net.Mime.MediaTypeNames;

namespace AppLogic.Logic
{
    public class GameLogic
    {
        private GameState GameState { get; set; }
        public GameLogic(string saveFile = "")
        {
            if (saveFile == "")
            {
                Data.LoadAllData();
                GameState = new GameState(new Character());

            }
            else
            {
                Character PC = Data.LoadSave(saveFile);
                GameState = new GameState(PC);
            }
        }
        public Outcome DecisionTree(string text)
        {
            Outcome outcome = new();
            Dictionary<Command, Func<ParsedText, Outcome, string>> methods = new()
            {
                {Command.Drop, DropX },
                {Command.Inventory, ShowPlayerInventory},
                {Command.Move, MoveCharacter },
                {Command.Check, CheckDirection },
                {Command.Examine, InspectX },
                {Command.Inspect, InspectX },
                {Command.Take, TakeX},
                {Command.Use, UseItemOnX },
                {Command.Give, GiveToNPC },
                {Command.Stop, StopTalkingToNPC },
                {Command.Help, HelpText },
                {Command.Talk, StartTalkingToNPC },
                {Command.Store, PutItemInContainer }
            };
            ParsedText parsedText = Parser.ParseText(text.ToLower());
            if (parsedText.Command == Command.Stop && GameState.ConversationMode)
            {
                outcome.Message = StopTalkingToNPC(parsedText, outcome);
            } 
            else if (parsedText.Command == Command.Give && GameState.ConversationMode)
            {
                outcome.Message = GiveToNPC(parsedText, outcome);
            }
            else if (GameState.ConversationMode)
            {
                outcome.Message = TalkToNPC(text, outcome);
            }
            else if (methods.ContainsKey(parsedText.Command))
            {
                outcome.Message = methods[parsedText.Command](parsedText, outcome);
            }
            else
            {
                outcome.Message = "Command was not recognized";
            }
            outcome.CurrentLocation = GameState.GetCurrentLocationInfo();
            outcome.InventoryNames = GameState.GetPlayerItems();
            outcome.HasWon = GameState.IsWon;
            return outcome;
        }
        private string StartTalkingToNPC(ParsedText parsed, Outcome outcome)
        {
            if (GameState.ConversationMode) return $"You're already talking to someone.";
            if (parsed.NPC != string.Empty && parsed.Command == Command.Talk
                && parsed.RemainingContains("to"))
            {
                return GameState.StartTalkingToNPC(parsed, outcome);
            }
            return "Command was not recognized.";

        }
        private string TalkToNPC(string text, Outcome outcome)
        {
            return GameState.TalkToNPC(text, outcome);
        }
        private string StopTalkingToNPC(ParsedText parsed, Outcome outcome)
        {
            return GameState.StopTalkingToNPC(outcome);
        }
        private string GiveToNPC(ParsedText parsed, Outcome outcome)
        {
            if (parsed.RemainingContains("to") && parsed.ItemOne != string.Empty && parsed.NPC != string.Empty)
            {
                return GameState.GiftNPC(parsed);
            }
            if (parsed.RemainingContains("to") == false)
            {
                return "The format is 'give item to NPC'.";
            }
            if (parsed.NPC == string.Empty)
            {
                return "You need to specify the name of the NPC.";
            }
            if (parsed.ItemOne == string.Empty)
            {
                return "You need to specify the item.";
            }
            return "Command was not recognized";
        }
        private string InspectX(ParsedText parsed, Outcome outcome)
        {
            bool isRemainingZero = parsed.Remaining.Length == 0;
            if (parsed.ItemOne != string.Empty && isRemainingZero
                && parsed.HasOnly("itemone command"))
            {
                return GameState.ExamineItem(parsed);
            }
            if (parsed.Container != string.Empty && isRemainingZero
                && parsed.HasOnly("container command"))
            {
                return GameState.ExamineContainer(parsed);

            }
            if (parsed.Container != string.Empty && parsed.RemainingContains("on in")
                && parsed.ItemOne != string.Empty && parsed.HasOnly("container command itemone"))
            {
                return GameState.InspectItemInContainer(parsed);
            }
            if (parsed.Direction != Direction.Unknown && isRemainingZero 
                && parsed.HasOnly("direction command")) 
            {
                (bool check, string output) = GameState.CheckDirection(parsed);
                return output;
            }
            if (isRemainingZero && parsed.HasOnly("command")) return GameState.InspectLocation();
            if (parsed.Direction == Direction.Unknown && isRemainingZero) return "Not a valid direction";
            return "Command was not recognized.";
            
        }
        private string PutItemInContainer(ParsedText parsed, Outcome outcome)
        {
            if (parsed.Container == string.Empty) return "You need to specify the container.";
            if (!parsed.RemainingContains("in")) return "The format is 'put item in container'.";
            if (parsed.ItemOne == string.Empty) return "You need to specify the item.";
            return GameState.PutItemInContainer(parsed);
        }
        private string DropX(ParsedText parsed, Outcome outcome)
        {
            if (parsed.ItemOne != string.Empty) return GameState.DropItem(parsed);
            if (parsed.Container != string.Empty && parsed.HasOnly("container command")) return GameState.DropContainer(parsed);
            return GameState.DropItem(parsed);
        }
        private string TakeX(ParsedText parsed, Outcome outcome)
        {
            if (parsed.Container != string.Empty && parsed.Remaining.Contains("from")
                && parsed.ItemOne != string.Empty && parsed.Remaining.Length == 4)
            {
                return GameState.PickUpItemFromContainer(parsed);
            }
            if (parsed.ItemOne != string.Empty && parsed.Remaining.Length == 0) return GameState.PickUpItem(parsed);
            if (parsed.Container != string.Empty && parsed.HasOnly("container command")) return GameState.TakeContainer(parsed);
            return "Command was not recognized.";
        }
        private string UseItemOnX(ParsedText parsed, Outcome outcome)
        {
            if (parsed.ItemOne == string.Empty &&
                (parsed.Obstruction == string.Empty || parsed.ItemTwo == string.Empty ||
                parsed.Direction != Direction.Unknown) && !parsed.Remaining.Contains("on in") && parsed.Remaining.Length != 2)
            {
                return "Command was not recognized.";
            }

            if (parsed.ItemTwo != string.Empty)
            {
                return GameState.UseItemOnItem(parsed);
            }
            if (parsed.Obstruction != string.Empty)
            {
                return GameState.ClearObstruction(parsed);
            }
            if (parsed.ItemOne != string.Empty && parsed.Direction != Direction.Unknown)
            {
                return GameState.UnlockDoor(parsed);
            }
            return "Command was not recognized.";
        }
        private string CheckDirection(ParsedText parsed, Outcome outcome)
        {
            if (parsed.Direction == Direction.Unknown)
            {
                return "The direction needs to be North, South, East or West.";
            }
            (bool check, string output) = GameState.CheckDirection(parsed);
            return output;

        }
        private string MoveCharacter(ParsedText parsed, Outcome outcome)
        {
            if (parsed.Direction == Direction.Unknown)
            {
                return "The direction needs to be North, South, East or West.";
            }
            return GameState.MoveToLocation(parsed);
        }
        private string ShowPlayerInventory(ParsedText parsed, Outcome outcome)
        {
            return GameState.DisplayInventory();
        }
        public GameState UpdateState()
        {
            return GameState;
        }
        public Outcome StateAtGameStart()
        {
            return new Outcome() { CurrentLocation = GameState.GetCurrentLocationInfo(), InventoryNames = GameState.GetPlayerItems(), HasWon = false, Message = "" };
        }
        public string[] GameStart()
        {
            string[] output = new string[] { $"You awake with no memory of where you are, nor how you got here.",
                            $"All you know is that you need to get out of here, and get back home. ",
                            "You can write 'help' to get a list of useful commands.",
                            $"{GameState.InspectLocation()}" };
            return output;
        }
        private string HelpText(ParsedText parsed, Outcome outcome)
        {
            return $"Here are some helpful commands" +
                   $"\n move 'direction' - moves in direction." +
                   $"\n examine - examine the location." +
                   $"\n examine 'item' - examine the item." +
                   $"\n examine 'direction' - examines the direction." +
                   $"\n examine 'container' - examines container." +
                   $"\n take 'item' - take the item." +
                   $"\n take 'item' from 'container' - takes item from container." +
                   $"\n drop 'item' - drops the item." +
                   $"\n use 'item' on 'item' - use an item on another item." +
                   $"\n use 'item' on 'direction' - uses a key on a specific door." +
                   $"\n use 'item' on 'obstacle' - uses an item on an obstacle." +
                   $"\n talk to 'npc' - talks to an NPC." +
                   $"\n give 'item' to 'npc' - gives item to NPC.";
        }
        public void SaveGame(string saveFileName)
        {
            Character PC = GameState.GetCharacter();
            Data.SaveGame(saveFileName, PC);
        }
    }
}
