﻿namespace Adventure.Classes.Models
{
    public class GameState
    {
        private Character PC { get; set; } = new Character();
        private Location CurrentLocation { get; set; } = new Location();
        public void SetStartingLocation(Location location)
        {
            CurrentLocation = location;
        }
        public (bool, string) CheckDirection(Parsed parsed)
        {
            bool pathExists = CurrentLocation.Exits.ContainsKey(parsed.Direction);
            if (pathExists == false)
            {
                return (false, $"There is no door to the {parsed.DirectionText.ToLower()}.");
            }
            bool hasObstruction = CurrentLocation.Exits[parsed.Direction].Obstruction != null;
            if (hasObstruction)
            {
                Obstruction? obstruction = CurrentLocation.Exits[parsed.Direction].Obstruction;
                string output = $"{obstruction.Article} {obstruction.Name.ToLower()} blocks the exit.";
                return (false, output);
            }
            if (CurrentLocation.Exits[parsed.Direction].IsLocked)
            {
                return (false, $"The door is locked.");
            }
            return (true, CurrentLocation.Exits[parsed.Direction].Inspect());
        }
        public string MoveToLocation(Parsed parsed)
        {
            (bool check, string output) = CheckDirection(parsed);
            if (check == false)
            {
                return output;
            }
            CurrentLocation = CurrentLocation.Exits[parsed.Direction].Locations[parsed.Direction];
            return $"You move to the {parsed.DirectionText.ToLower()}. You are now in {CurrentLocation.Name.ToLower()}.";
        }
        public string DropItem(Parsed parsed)
        {
            Item? itemToDrop = PC.GetItem(parsed.ItemOne);
            if (itemToDrop != null)
            {
                CurrentLocation.Items.Add(itemToDrop);
                PC.Items.Remove(itemToDrop);
                return $"You drop {itemToDrop} on the ground.";
            }
            return $"You do not have {parsed.ItemOneText}.";
        }
        public string InspectDirection(Parsed parsed)
        {
            (bool check, string message) = CheckDirection(parsed);
            if (message == $"There is no path to the {parsed.DirectionText}") return "Not a valid direction.";
            return CurrentLocation.Exits[parsed.Direction].Inspect();
        }
        public string UseItemOnItem(Parsed parsed)
        {
            Item? itemOne = PC.GetItem(parsed.ItemOne);
            if (itemOne == null) return $"You do not have {parsed.ItemOneText}.";
            Item? itemTwo = PC.GetItem(parsed.ItemTwo);
            if (itemTwo == null) return $"You do not have {parsed.ItemTwoText}.";

            if (itemOne.UsableOn != parsed.ItemTwo || itemOne.SpecialItem == null)
            {
                return $"Nothing happens when you use {parsed.ItemOneText} on {parsed.ItemTwoText}.";
            }
            Item newItem = itemOne.SpecialItem;
            PC.AddItem(newItem);
            PC.RemoveItem(itemOne);
            PC.RemoveItem(itemTwo);
            return $"You use {parsed.ItemOneText} on {parsed.ItemTwoText}, and gain {newItem}.";
        }
        public string PickUpItem(Parsed parsed)
        {
            if (parsed.ItemOne == Enums.Items.Unknown) return "You need to specify an item.";
            Item? itemToPickup = CurrentLocation.GetItem(parsed.ItemOne);
            if (itemToPickup != null)
            {
                PC.AddItem(itemToPickup);
                CurrentLocation.RemoveItem(itemToPickup);
                return $"You pick up {itemToPickup}.";
            }
            else
            {
                return $"There is no {parsed.ItemOneText} in the room.";
            }
        }
        public string PickUpItemFromContainer(Parsed parsed)
        {
            Container? container = CurrentLocation.GetContainer(parsed.Container);
            if (container == null) return $"There is no {parsed.ContainerText} in {CurrentLocation.Name.ToLower()}";
            Item? itemToPickUp = container.GetItem(parsed.ItemOne);
            if (itemToPickUp == null) return $"There is no {parsed.ItemOneText} in {parsed.ContainerText}";
            PC.AddItem(itemToPickUp);
            container.RemoveItem(itemToPickUp);
            return $"You pick up {itemToPickUp} from {parsed.ContainerText}";
        }
        public string ExamineItem(Parsed parsed)
        {
            Item? InspectedItem = PC.GetItem(parsed.ItemOne);
            if (parsed.ItemOne == Enums.Items.Unknown) return "You need to specify an item to examine.";
            if (InspectedItem == null)
            {
                InspectedItem = CurrentLocation.GetItem(parsed.ItemOne);
                if (InspectedItem == null)
                {
                    return $"There's no {parsed.ItemOneText} in neither your inventory nor the {CurrentLocation.Name.ToLower()}";
                }
                return InspectedItem.Inspect();

            }
            return InspectedItem.Inspect();
        }
        public string ExamineContainer(Parsed parsed)
        {
            Container? examinedContainer = CurrentLocation.GetContainer(parsed.Container);
            if (examinedContainer == null) return $"There is no {parsed.ContainerText} in {CurrentLocation.Name.ToLower()}.";
            return examinedContainer.Inspect();
        }
        public string ClearObstruction(Parsed parsed)
        {
            Item? item = PC.GetItem(parsed.ItemOne);
            if (item == null)
            {
                return $"You do not have {parsed.ItemOneText}";
            }
            Exit? exit = null;
            Obstruction? obstruction = null;
            foreach (Exit value in CurrentLocation.Exits.Values)
            {
                if (value.Obstruction != null && value.Obstruction.Type == parsed.Obstruction)
                {
                    exit = value;
                    obstruction = value.Obstruction;
                    break;
                }
            }
            if (obstruction == null)
            {
                return $"There is no {parsed.ObstructionText} in the {CurrentLocation.Name.ToLower()}";
            }
            if (obstruction != null && exit != null && obstruction.ClearedBy.ToLower() == item.Name.ToLower())
            {
                exit.Obstruction = null;
                PC.Items.Remove(item);
                return $"You use your {item.Name.ToLower()} to clear the {obstruction.Name.ToLower()}";
            }
            if (item.Name.ToLower() != obstruction.ClearedBy.ToLower())
            {
                return $"You cannot clear a {parsed.ObstructionText} with {item.Name}";
            }
            return "Something went wrong";

        }
        public string InspectLocation()
        {
            return CurrentLocation.Inspect();
        }
        public string DisplayInventory()
        {
            return PC.DisplayInventory();
        }
        public List<string> GetPlayerItems()
        {
            return PC.Items.Select(item => item.Name).ToList();
        }
        public string GetCurrentLocationInfo()
        {
            return CurrentLocation.Name;
        }

    }
}
