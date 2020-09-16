using System;
using System.Net.Http;
using BrivTest2.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Newtonsoft.Json;

namespace BrivTest2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IHostingEnvironment _env;
        //These is used with the Session Manager
        private Character _character = new Character();
        private bool isSaved; //would be used for error handling
        public ValuesController(IHostingEnvironment env)
        {
            _env = env;
            //Processing process = new Processing(_env);
        }
        #region Get Character
        [HttpGet("getcharacter")]
        public IActionResult GetCharacter()
        {
            Processing process = new Processing(_env);

            bool savedCharacter;
            string filepath;

            //Checks to see if there is a save file for this character. Normally this would be from a database,
            //but i stuck with JSON since that's what I received and had the structure for. 
            savedCharacter = process.CheckForSave();

            //If so, load the information from the file
            if (savedCharacter == true)
            {
                filepath = _env.WebRootPath + "/js/brivSave.json";
                _character = process.PopulateCharacter(filepath, savedCharacter);
            }
            //If not, create a new character from the base file
            else
            {
                filepath = _env.WebRootPath + "/js/briv.json";
                _character = process.PopulateCharacter(filepath, savedCharacter);
                int rolledHP = process.CalculateHP(_character.Classes, _character.Stats.Constitution);

                _character.MaxHP = rolledHP;
                _character.CurrentHP = rolledHP;
                _character.TempHP = 0;

                isSaved = SaveCharacterInformation(_character);
            }

            SessionManager.Set(HttpContext.Session, "character", _character);
            string json = JsonConvert.SerializeObject(_character);

            return Ok(json);
        }
        #endregion
        #region Get Ability Modifiers
        [HttpGet("getmodifiers")]
        public IActionResult GetModifiers()
        {
            Processing process = new Processing(_env);
            Modifier _modifiers = new Modifier();

            _character = SessionManager.Get<Character>(HttpContext.Session, "character");
            _modifiers = process.GetStatModifiers(_character.Stats);

            SessionManager.Set(HttpContext.Session, "modifiers", _modifiers);
            string json = JsonConvert.SerializeObject(_modifiers);

            return Ok(json);
        }
        #endregion
        #region Do Damage
        [HttpPost("dodamage")]
        public IActionResult DoDamage(AttackDamage damage)
        {
            _character = SessionManager.Get<Character>(HttpContext.Session, "character");

            Processing process = new Processing(_env);
            HitPoints _hitPoints = new HitPoints();

            _hitPoints = process.RemainingHP(damage.DamageType, damage.Damage, _character.Defenses, _character.CurrentHP, _character.TempHP, _character.MaxHP);
            _character.CurrentHP = _hitPoints.HP;
            _character.TempHP = _hitPoints.TempHP;

            isSaved = SaveCharacterInformation(_character);

            SessionManager.Set(HttpContext.Session, "character", _character);
            string json = JsonConvert.SerializeObject(_hitPoints);

            return Ok(json);
        }
        #endregion
        #region Add Temporary HP
        [HttpPost("addtemphp")]
        public IActionResult AddTempHP(HitPoints tempHP)
        {
            _character = SessionManager.Get<Character>(HttpContext.Session, "character");

            if (_character.TempHP < tempHP.TempHP)
            {
                _character.TempHP = tempHP.TempHP;
                SessionManager.Set(HttpContext.Session, "character", _character);
            }

            isSaved = SaveCharacterInformation(_character);
            string json = JsonConvert.SerializeObject(_character.TempHP);

            return Ok(json);
        }
        #endregion
        #region Healing HP
        [HttpPost("healhp")]
        public IActionResult HealHP(HitPoints healing)
        {
            _character = SessionManager.Get<Character>(HttpContext.Session, "character");

            if ((_character.CurrentHP += healing.HealingHP) > _character.MaxHP)
            {
                _character.CurrentHP = _character.MaxHP;
            }

            else
            {
                _character.CurrentHP += healing.HealingHP;
            }

            isSaved = SaveCharacterInformation(_character);
            SessionManager.Set(HttpContext.Session, "character", _character);

            string json = JsonConvert.SerializeObject(_character.CurrentHP);

            return Ok(json);
        }
        #endregion
        #region Save Character Information
        private bool SaveCharacterInformation(Character character)
        {
            Processing process = new Processing(_env);
            bool saveCharacter = process.SaveCharacter(character);
            return saveCharacter;
        }
        #endregion

        private IActionResult HttpResponseMessage(HttpResponseMessage result)
        {
            throw new NotImplementedException();
        }
    }
}
