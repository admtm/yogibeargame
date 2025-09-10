using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace YogiBear.Persistence
{
    public enum ItemType { PICNICBASKET, TREE };
    public class Item : Pieces
    {
        public ItemType Type { get; }

        public Item(ItemType type, int x, int y) : base(x, y)
        {
            Type = type;
        }

        public bool AmICollectible()
        {
            switch (Type)
            {
                case ItemType.TREE:
                    return false;
                case ItemType.PICNICBASKET:
                    return true;
                default:
                    throw new ArgumentException(nameof(Type), $"Invalid item type: {Type}");
            }
        }

        public string WhatAmI()
        {
            switch (Type)
            {
                case ItemType.PICNICBASKET:
                    return "P";
                case ItemType.TREE:
                    return "T";
                default:
                    throw new ArgumentException(nameof(Type), $"Invalid item type: {Type}");
            }
        }

        public string WhichColor()
        {
            switch (Type)
            {
                case ItemType.PICNICBASKET:
                    return "MediumVioletRed";
                case ItemType.TREE:
                    return "ForestGreen";
                default:
                    throw new ArgumentException(nameof(Type), $"Invalid item type: {Type}");
            }
        }
    }
}
