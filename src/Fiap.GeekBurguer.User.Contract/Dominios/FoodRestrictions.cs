using Fiap.GeekBurguer.Users.Contract.Dominios;
using System;
using System.Collections.Generic;

namespace Fiap.GeekBurguer.Users.Contract
{
    public class FoodRestrictions
    {
        public List<Restriction> Restrictions { get; set; }
        public RestrictionOther Others { get; set; }
        public Guid UserId { get; set; }
        public Guid RequesterId { get; set; }
    }
}
