using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrivTest2.Models
{
    public class Character
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public List<Class> Classes { get; set; }
        public Stat Stats{ get; set; }
        public List<Item> Items { get; set; }
        public List<Def> Defenses { get; set; }

        public class Class
        {
            public string Name { get; set; }
            public int HitDiceValue { get; set; }
            public int ClassLevel { get; set; }
        }

        public class Stat
        {
            public int Strength { get; set; }
            public int Dexterity { get; set; }
            public int Constitution { get; set; }
            public int Intelligence { get; set; }
            public int Wisdom { get; set; }
            public int Charisma { get; set; }
        }

        public class Modifier
        {
            public string AffectedObject { get; set; }
            public string AffectedValue { get; set; }
            public int Value { get; set; }
        }

        public class Item
        {
            public string Name { get; set; }
            //public List<Modifiers> Modifier { get; set; }
            public Modifier Modifier { get; set; }
        }

        public class Def
        {
            public string Type { get; set; }
            public string Defense { get; set; }
        }
    }
}
