using System;
using System.Collections.Generic;
using System.Text;

namespace Fiap.GeekBurguer.Domain.Model
{
    public class FoodRestrictions
    {
        public string Restrictions { get; set; }
        public string Others { get; set; }
        public Guid UserId { get; set; }
        public Guid RequesterId { get; set; }
    }
}
