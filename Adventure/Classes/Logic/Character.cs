﻿using Adventure.Enums;
using System.Collections.Specialized;
using Adventure.Enums;

namespace Adventure.Classes.Models
{

    public class Character
    {
        public string Name { get; set; }
        public List<Item> Items { get; set; } = new List<Item>();
        public void AddItem(Item item)
        {
            Items.Add(item);
        }
        public void RemoveItem(Item item)
        {
            Items.Remove(item);
        }
        public bool HasItem(Items itemType)
        {
            return Items.Any(item => item.Type == itemType);
        }
        public Item? GetItem(Items itemType)
        {
            return Items.Find(item => item.Type == itemType);
        }
        public string DisplayInventory()
        {
            if (Items.Count == 0) return "You have no items.";
            string output = "You have the following items: ";
            int count = 0;
            foreach (var item in Items)
            {
                if (count == 0)
                {
                    output += item.Name;
                }
                else if (count == Items.Count - 1)
                {
                    output += $" & {item}.";
                }

                else
                {
                    output += ", " + item.Name;
                }
                count++;
            }
            return output;
        }
    }
}
