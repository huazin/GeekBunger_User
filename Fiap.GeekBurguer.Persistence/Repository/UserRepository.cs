using Fiap.GeekBurguer.Core.Service;
using Fiap.GeekBurguer.Domain.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fiap.GeekBurguer.Persistence.Repository
{
    public class UserRepository : BaseRepository<User>
    {
        public UserRepository(MyDbContext context, IMessageService<User> messageService) : base(context, messageService)
        {
        }
    }
}
