using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Fiap.GeekBurguer.Domain.Model
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; }
        public string Face { get; set; }
        public Guid RequesterId { get; set; }
    }
}
