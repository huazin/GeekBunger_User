using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fiap.GeekBurguer.Users.Contract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fiap.GeekBurguer.Users.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodRestrictionController : ControllerBase
    {
        #region MOCK
        private Guid UserIdUm;
        private Guid UserIdDois;
        private List<FoodRestrictions> listaRestricoes;
        #endregion

        public FoodRestrictionController()
        {
            UserIdUm = new Guid("31914279-0fec-44a3-864e-70a31c8bf832");
            UserIdDois = new Guid("5673d5ce-2284-4786-984d-a8d69b2fd8a4");

            listaRestricoes = new List<FoodRestrictions>();

            listaRestricoes.Add(
                new FoodRestrictions
                {
                    RequesterId = Guid.NewGuid(),
                    UserId = UserIdUm,
                    Restrictions = new List<Restriction>() ,
                    Others = new RestrictionOther("Brocolis")
                }
            );
            listaRestricoes.Add(
                new FoodRestrictions
                {
                    RequesterId = Guid.NewGuid(),
                    UserId = UserIdDois,
                    Restrictions = new List<Restriction>(),
                    Others = new RestrictionOther("Ovos")
                }
            );
        }


        [HttpGet("{userId}/GetFoodRestrictionsByUserId")]
        public IActionResult GetFoodRestrictionsByUserId(Guid userId)
        {
            FoodRestrictions restricao = listaRestricoes.Find(l => l.UserId == userId);

            return Ok(restricao);
        }

        [HttpPost]
        public IActionResult PostFoodRestrictionsByUserId([FromBody]FoodRestrictions foodRestrictions)
        {
            if (foodRestrictions.UserId != Guid.Empty)
            {
                return Ok();
            }

            return new NotFoundResult();
        }
    }
}