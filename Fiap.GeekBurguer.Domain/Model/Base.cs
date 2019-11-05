using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Fiap.GeekBurguer.Domain.Model
{
    public class Base
    {
        [Key]
        public Guid ID { get; set; }
    }
}
