using Fiap.GeekBurguer.Core.Service;
using Fiap.GeekBurguer.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fiap.GeekBurguer.Persistence.Repository
{
    public class RestrictionRepository : BaseRepository<Restriction>
    {
        public RestrictionRepository(MyDbContext context, IMessageService<Restriction> messageService) : base(context, messageService)
        {
        }

        public IEnumerable<Restriction> ObterPorUsuario(Guid id)
        {
            return _Db.Restriction.Where(p => p.UserID == id);
        }
    }
}
