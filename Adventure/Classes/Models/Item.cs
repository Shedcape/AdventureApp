﻿using Adventure.Enums;
using Adventure.Classes.Data;

namespace Adventure.Classes.Models
{
    public class Item
    {
        public int ID { get; set; } = -1;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int UsableOn { get; set; } = -1;
        public Items Type { get; set; } = Items.Unknown;
        public int SpecialItem { get; set; } = -1;
        public string Article { get; set; } = string.Empty;
        public override string ToString()
        {
            return $"{Article.ToLower()} {Name.ToLower()}";
        }
        public string Inspect()
        {
            Item? specialItem = Data.Data.GetItem(SpecialItem);
            Item? usableOn = Data.Data.GetItem(UsableOn);
            string usableOnText = UsableOn == -1 || specialItem == null ? "" : $"This item can be used on {usableOn} to gain {specialItem}.";
            return $"{Description} {usableOnText}";
        }
    }
}
