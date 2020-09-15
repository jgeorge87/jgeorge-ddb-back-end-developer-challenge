using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using static BrivTest2.Models.Character;

namespace BrivTest2.Models
{
    public class Processing
    {
        private readonly Random _random = new Random();
        private IHostingEnvironment _env; 
        public Processing(IHostingEnvironment env)
        {
            _env = env;
        }
        public void JsonSerialize(object data, string filePath)
        {
            JsonSerializer jsonSerializer = new JsonSerializer();
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            StreamWriter sw = new StreamWriter(filePath);
            JsonWriter jsonWriter = new JsonTextWriter(sw);

            jsonSerializer.Serialize(jsonWriter, data);
            jsonWriter.Close();
            sw.Close();
        }

        public object JsonDeserialize(Type dataType, string filePath)
        {
            JObject obj = null;
            JsonSerializer jsonSerializer = new JsonSerializer();
            if (File.Exists(filePath))
            {
                StreamReader sr = new StreamReader(filePath);
                JsonReader jsonReader = new JsonTextReader(sr);
                obj = jsonSerializer.Deserialize(jsonReader) as JObject;
                jsonReader.Close();
                sr.Close();
            }
            return obj.ToObject(dataType);
        }

        public Character PopulateCharacter()
        {
            Character briv = JsonConvert.DeserializeObject<Character>(File.ReadAllText(_env.WebRootPath + "/js/briv.json"));
            Character character = new Character();

            List<Class> list = briv.Classes;
            List<Item> items = briv.Items;
            List<Def> defense = briv.Defenses;
            Stat stat = new Stat();
            //This function is adding the +2 to the constitution stat from the equipped item
            stat = StatIncreaseByItem(briv.Stats, briv.Items);

            character.Name = briv.Name;
            character.Level = briv.Level;
            character.Classes = list;
            character.Stats = stat;
            character.Items = items;
            character.Defenses = defense;

            return character;
        }

        public int CalculateHP (List<Class> classes, int constitution)
        {
            int totalHP = 0;
            int totalLevel = 0;

            //This loops through the character's classes
            foreach(Class c in classes)
            {
                int hitDie = c.HitDiceValue;
                int level = c.ClassLevel;
                int levelHP = 0;

                //This will roll the hit dice for each class
                for(int i = 0; i < level; i++)
                {
                    int hp = RandomNumber(1, hitDie);
                    levelHP += hp;
                }
                //The HP and level totals are aggregated here
                totalHP += levelHP;
                totalLevel += level;
            }

            double conMod = ((constitution - 10) / 2); //This formula gets the constitution modifier before rounding
            totalHP += (int)Math.Floor(conMod); //This will round that value down
            totalHP += (int)conMod * totalLevel; //This adds the rest of the hit points based on the character's constitution
            return totalHP;
        }

        public Stat StatIncreaseByItem(Stat stats, List<Item> items)
        {
            //This will add any stat increases from equipped items. (There's likely a better way)
            //Else if was not used in the case that an item provides multiple stat increases.
            foreach (Item item in items)
            {
                if (item.Modifier.AffectedValue == "strength")
                {
                    stats.Strength += item.Modifier.Value;
                }

                if (item.Modifier.AffectedValue == "dexterity")
                {
                    stats.Dexterity += item.Modifier.Value;
                }

                if (item.Modifier.AffectedValue == "constitution")
                {
                    stats.Constitution += item.Modifier.Value;
                }

                if (item.Modifier.AffectedValue == "intelligence")
                {
                    stats.Intelligence += item.Modifier.Value;
                }

                if (item.Modifier.AffectedValue == "wisdom")
                {
                    stats.Wisdom += item.Modifier.Value;
                }

                if (item.Modifier.AffectedValue == "charisma")
                {
                    stats.Charisma += item.Modifier.Value;
                }
            }

            return stats;
        }

        // Generates a random number within a range.      
        public int RandomNumber(int min, int max)
        {
            return _random.Next(min, max);
        }

        public HitPoints RemainingHP(string type, int damage, List<Def> defenses, int HP, int tempHp)
        {
            //This will handle damage resistances and immunities
            foreach (Def defense in defenses)
            {
                if(defense.Type == type && defense.Defense == "immunity")
                {
                    damage = 0;
                }

                else if(defense.Type == type && defense.Defense == "resistance")
                {
                    damage = (int)Math.Floor(Convert.ToDouble(damage) / 2);
                }
            }
            //This will apply the damage to temporary hit points if they are available
            if(tempHp > 0)
            {
                if (damage > tempHp)
                {
                    damage -= tempHp;
                    tempHp = 0;
                }
                //If the damage amount is less than the temporary hit points the damage is set to zero after affecting the hit points
                else
                {
                    tempHp -= damage;
                    damage = 0;
                }
            }

            int remainingHP = HP - damage;
            HitPoints hp = new HitPoints();
            hp.HP = remainingHP;
            hp.TempHP = tempHp;
            return hp;
        }
    }
}
