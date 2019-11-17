using System;
using System.Collections.Generic;
using System.Text;

namespace Fiap.GeekBurguer.Users.Contract
{
    public class User
    {
        public string Face { get; set; }
        public Guid? UserId { get; set; }
        public Guid RequesterId { get; set; }
    }
}
