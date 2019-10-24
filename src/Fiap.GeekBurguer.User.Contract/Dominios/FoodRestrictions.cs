using System;

namespace Fiap.GeekBurguer.Users.Contract
{
    public class FoodRestrictions
    {
        public string Restrictions { get; set; }
        public string Others { get; set; }
        public Guid UserId { get; set; }
        public Guid RequesterId { get; set; }
    }
}
