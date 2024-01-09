using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPServerExample.Data;
using TCPServerExample.DB;
using static System.Net.Mime.MediaTypeNames;

namespace TCPServerExample.Game
{
    public class Item
    {
        public ItemInfo info { get; } = new();

        public int ItemDbId
        {
            get { return info.ItemDbId; }
            set { info.ItemDbId = value;}
        }

        public int TemplateId
        {
            get { return info.TemplateId; }
            set { info.TemplateId = value; }
        }

        public int Count
        {
            get { return info.Count; }
            set { info.Count = value; }
        }

        public ItemType ItemType { get; private set; }
        public bool Stackable { get; protected set; }


        public Item(ItemType itemType)
        {
            this.ItemType = itemType;
        }

        public static Item MakeItem(ItemDb itemdb)
        {
            Item item = null;

            ItemData itemData = null;
            DataManager.ItemDict.TryGetValue(itemdb.TemplateId, out itemData);
            if (itemData != null)
                return null;
            switch (itemData.itemType)
            {
                case ItemType.Weapon:
                    item = new Weapon(itemdb.TemplateId);
                    break;
                case ItemType.Armor:
                    item = new Armor(itemdb.TemplateId);
                    break;
                case ItemType.Consumable:
                    item = new Consumable(itemdb.TemplateId);
                    break;
            }
            if (item != null)
            {
                item.ItemDbId = itemdb.ItemDbId;
                item.Count = itemdb.Count;
            }

            return item;
        }
    }

    public class Weapon : Item
    {
        public WeaponType WeaponType { get; private set; }
        public int Damage { get; private set; }
        public Weapon(int templateId)
            : base(ItemType.Weapon)
        {
            Init(templateId);
        }

        void Init(int templateId)
        {
            ItemData itemData = null;
            DataManager.ItemDict.TryGetValue(templateId, out itemData);
            if (itemData.itemType != ItemType.Weapon)
                return;

            WeaponData data = (WeaponData)itemData;
            {
                TemplateId = templateId;
                Count = 1;
                WeaponType = data.weaponType;
                Damage = data.damage;
                Stackable = false;
            }
        }
    }

    public class Armor : Item
    {
        public ArmorType ArmorType { get; private set; }
        public int Defence { get; private set; }
        public Armor(int templateId)
                    : base(ItemType.Armor)
        {
            Init(templateId);
        }

        void Init(int templateId)
        {
            ItemData itemData = null;
            DataManager.ItemDict.TryGetValue(templateId, out itemData);
            if (itemData.itemType != ItemType.Armor)
                return;

            ArmorData data = (ArmorData)itemData;
            {
                TemplateId = templateId;
                Count = 1;
                ArmorType = data.armorType;
                Defence = data.defence;
                Stackable = false;
            }
        }
    }

    public class Consumable : Item
    {
        public ConsumableType ConsumableType { get; private set; }
        public int MaxCount { get; set; }
        public Consumable(int templateId)
                    : base(ItemType.Consumable)
        {
            Init(templateId);
        }

        void Init(int templateId)
        {
            ItemData itemData = null;
            DataManager.ItemDict.TryGetValue(templateId, out itemData);
            if (itemData.itemType != ItemType.Consumable)
                return;

            ConsumableData data = (ConsumableData)itemData;
            {
                TemplateId = templateId;
                Count = 1;
                MaxCount = data.maxCount;
                ConsumableType = data.consumableType;
                Stackable = data.maxCount > 1;
            }
        }
    }
}
