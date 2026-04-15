// File: Item.cs
// Student: Chinonso Praise Ignatius
// Course: SECU2000 - Application Security
// Description: this is the item model, it matches the Items table in the database

namespace InventoryAPI.Models
{
    // this class represents an inventory item in the system
    public class Item
    {
        // unique id for each item
        public int ItemId { get; set; }

        // name of the inventory item
        public string ItemName { get; set; } = string.Empty;

        // category the item belongs to
        public string Category { get; set; } = string.Empty;

        // how many of this item we have
        public int Quantity { get; set; }

        // extra details about the item
        public string Description { get; set; } = string.Empty;

        // which user added this item
        public int AddedBy { get; set; }

        // when the item was added
        public DateTime CreatedAt { get; set; }
    }

    // this class is used when someone adds or updates an item
    public class ItemRequest
    {
        // name of the item being added
        public string ItemName { get; set; } = string.Empty;

        // category of the item being added
        public string Category { get; set; } = string.Empty;

        // quantity of the item being added
        public int Quantity { get; set; }

        // description of the item being added
        public string Description { get; set; } = string.Empty;
    }
}