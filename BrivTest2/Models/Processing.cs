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
        private readonly IHostingEnvironment _env; 
        public Processing(IHostingEnvironment env)
        {
            _env = env;
        }
        #region Save Character
        public bool SaveCharacter(Character character)
        {
            using (StreamWriter file = File.CreateText(_env.WebRootPath + "/js/brivSave.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, character);

                if (File.Exists(_env.WebRootPath + "/js/brivSave.json"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion
        #region Check For Save
        public bool CheckForSave()
        {
            if (File.Exists(_env.WebRootPath + "/js/brivSave.json"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
        #region JSON Serialze and Deserialize
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
        #endregion
        #region Populate Character
        public Character PopulateCharacter(string filepath, bool loadCharacter)
        {
            Character briv = JsonConvert.DeserializeObject<Character>(File.ReadAllText(filepath));
            Character character = new Character();

            List<Class> list = briv.Classes;
            List<Item> items = briv.Items;
            List<Def> defense = briv.Defenses;
            character.Name = briv.Name;
            character.Level = briv.Level;
            character.Classes = list;
            character.Items = items;
            character.Defenses = defense;

            if (loadCharacter == true)
            {
                character.MaxHP = briv.MaxHP;
                character.CurrentHP = briv.CurrentHP;
                character.TempHP = briv.TempHP;
                character.Stats = briv.Stats;
            }

            else
            {
                //This function is adding the +2 to the constitution stat from the equipped item
                Stat stat = StatIncreaseByItem(briv.Stats, briv.Items);
                character.Stats = stat;
            }
            
            return character;
        }
        #endregion
        #region Calculate HP Upon Creation
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

            int conMod = (int)Math.Floor(Convert.ToDouble(constitution - 10) / 2); //This formula gets the constitution modifier
            totalHP += ((int)conMod * totalLevel) + conMod; //This adds the rest of the hit points based on the character's constitution
            return totalHP;
        }
        #endregion
        #region Get Ability Stat Modifiers
        public Modifier GetStatModifiers(Stat stats)
        {
            Modifier mods = new Modifier();
            mods.Strength = (int)Math.Floor(Convert.ToDouble(stats.Strength - 10) / 2);
            mods.Dexterity = (int)Math.Floor(Convert.ToDouble(stats.Dexterity - 10) / 2);
            mods.Constitution = (int)Math.Floor(Convert.ToDouble(stats.Constitution - 10) / 2);
            mods.Intelligence = (int)Math.Floor(Convert.ToDouble(stats.Intelligence - 10) / 2);
            mods.Wisdom = (int)Math.Floor(Convert.ToDouble(stats.Wisdom - 10) / 2);
            mods.Charisma = (int)Math.Floor(Convert.ToDouble(stats.Charisma - 10) / 2);

            return mods;
        }
        #endregion
        #region Get Stat Score Increase From Items
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
        #endregion
        #region Random Number Generator For Rolling Hit Dice
        // Generates a random number within a range.      
        public int RandomNumber(int min, int max)
        {
            return _random.Next(min, max);
        }
        #endregion
        #region Calculate Remaining HP After Taking Damage
        public HitPoints RemainingHP(string type, int damage, List<Def> defenses, int HP, int tempHp, int maxHP)
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
            //Makes sure HP does not display in negative number
            if (remainingHP < 0)
            {
                remainingHP = 0;
            }

            HitPoints hp = new HitPoints();
            hp.HP = remainingHP;
            hp.TempHP = tempHp;
            hp.MaxHP = maxHP;
            return hp;
        }
        #endregion
    }
}
