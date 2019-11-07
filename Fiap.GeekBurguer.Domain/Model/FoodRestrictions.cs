using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Fiap.GeekBurguer.Domain.Model
{
    public class FoodRestrictions : Base
    {
        [Key]
        public Guid UserId { get; set; }
        public List<Restriction> Restrictions { get; set; }
        public RestrictionOther Others { get; set; }


        [NotMapped]
        public Guid RequesterId { get; set; }
    }
}
