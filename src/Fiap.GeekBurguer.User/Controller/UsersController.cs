using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Fiap.GeekBurguer.Users.Contract;

namespace Fiap.GeekBurguer.Users.Controller
{
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        #region MOCK

        private Guid UserIdUm;
        private Guid UserIdDois;
                
        private string faceUm;
        private string faceDois;

        private List<FoodRestrictions> listaRestricoes;

        #endregion

        public UsersController()
        {
            UserIdUm = new Guid("31914279-0fec-44a3-864e-70a31c8bf832");
            UserIdDois = new Guid("5673d5ce-2284-4786-984d-a8d69b2fd8a4");

            faceUm = "ImagemUm";
            faceDois = "ImagemDois";

            listaRestricoes = new List<FoodRestrictions>();

            listaRestricoes.Add(
                new FoodRestrictions {
                    RequesterId = Guid.NewGuid(),
                    UserId = UserIdUm,
                    Restrictions = "['soja','gluten']",
                    Others = "brocolis"
                }
            );
            listaRestricoes.Add(
                new FoodRestrictions
                {
                    RequesterId = Guid.NewGuid(),
                    UserId = UserIdDois,
                    Restrictions = "['lactose']",
                    Others = "ovos"
                }
            );
        }    


        [HttpGet("{user}")]
        public IActionResult GetUserByFace(User user)
        {
            if (user.Face == "ImagemUm")
            {
                user.UserId = UserIdUm;
            }
            else if (user.Face == "ImagemDois")
            {
                user.UserId = UserIdDois;
            }

            return Ok(user);
        }

        [HttpGet("{userId}/GetFoodRestrictionsByUserId")]
        public IActionResult GetFoodRestrictionsByUserId(Guid userId)
        {
            FoodRestrictions restricao = listaRestricoes.Find(l => l.UserId == userId);

            return Ok(restricao);
        }

        [HttpPost("{foodRestrictions}")]
        public IActionResult PostFoodRestrictionsByUserId(FoodRestrictions foodRestrictions)
        {
            if (foodRestrictions != null)
            {
                return Ok();
            }

            return new NotFoundResult();
        }
    }
}