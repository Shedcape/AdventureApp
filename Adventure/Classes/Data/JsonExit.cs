﻿using Adventure.Classes.Models;
using Adventure.Enums;

namespace Adventure.Classes.Data
{
    public class JsonExit
    {
        public int ID { get; set; }
        public bool IsLocked { get; set; } = false;
        public int UnlockedBy { get; set; } = -1;
        public string Description { get; set; } = string.Empty;
        public string[]? Directions { get; set; } = null;
        public int[]? LocationIDs { get; set; } = null;
        public Dictionary<Directions, Location> Locations { get; set; } = new Dictionary<Directions, Location>();
        public int? ObstructionID { get; set; }

    }
}
