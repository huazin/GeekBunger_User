using Fiap.GeekBurguer.Core.Service;
using Fiap.GeekBurguer.Domain.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fiap.GeekBurguer.Persistence.Repository
{
    public class RestrictionRepository : BaseRepository<Restriction>
    {
        public RestrictionRepository(MyDbContext context, IMessageService<Restriction> messageService) : base(context, messageService)
        {
        }
    }
}
