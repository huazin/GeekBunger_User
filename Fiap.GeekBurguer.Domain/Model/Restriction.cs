using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Fiap.GeekBurguer.Domain.Model
{
    public class Restriction : Base
    {
        public string Nome { get; set; }
        public bool IsOther { get; set; }
    }
}
