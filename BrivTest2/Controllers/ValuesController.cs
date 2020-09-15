using System;
using System.Net.Http;
using BrivTest2.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BrivTest2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private IHostingEnvironment _env;
        //These are used witht the Session Manager
        private Character _character = new Character();
        private HitPoints _hitPoints = new HitPoints();
        public ValuesController(IHostingEnvironment env)
        {
            _env = env;
            Processing process = new Processing(_env);
        }
        [HttpGet("getcharacter")]
        public IActionResult GetCharacter()
        {
            Processing process = new Processing(_env);

            _character = process.PopulateCharacter();
            _hitPoints.HP = process.CalculateHP(_character.Classes, _character.Stats.Constitution);

            SessionManager.Set(HttpContext.Session, "HP", _hitPoints);
            SessionManager.Set(HttpContext.Session, "character", _character);

            string json = JsonConvert.SerializeObject(_character);

            return Ok(json);
        }

        [HttpPost("dodamage")]
        public IActionResult DoDamage(AttackDamage damage)
        {
            _character = SessionManager.Get<Character>(HttpContext.Session, "character");
            _hitPoints = SessionManager.Get<HitPoints>(HttpContext.Session, "HP");

            Processing process = new Processing(_env);
            HitPoints hp = new HitPoints();

            _hitPoints = process.RemainingHP(damage.Type, damage.Damage, _character.Defenses, _hitPoints.HP, _hitPoints.TempHP);

            SessionManager.Set(HttpContext.Session, "HP", _hitPoints);

            string json = JsonConvert.SerializeObject(SessionManager.Get<HitPoints>(HttpContext.Session, "HP"));

            return Ok(json);
        }

        [HttpPost("addtemphp")]
        public IActionResult AddTempHP(HitPoints tempHP)
        {
            _hitPoints = SessionManager.Get<HitPoints>(HttpContext.Session, "HP");
            _hitPoints.TempHP = tempHP.TempHP;

            SessionManager.Set(HttpContext.Session, "HP", _hitPoints);

            string json = JsonConvert.SerializeObject(_hitPoints);

            return Ok(json);
        }

        private IActionResult HttpResponseMessage(HttpResponseMessage result)
        {
            throw new NotImplementedException();
        }
    }
}
