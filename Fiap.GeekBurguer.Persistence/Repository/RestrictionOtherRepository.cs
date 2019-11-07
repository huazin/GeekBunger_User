using Fiap.GeekBurguer.Core.Service;
using Fiap.GeekBurguer.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fiap.GeekBurguer.Persistence.Repository
{
    public class RestrictionOtherRepository : BaseRepository<RestrictionOther>
    {
        public RestrictionOtherRepository(MyDbContext context, IMessageService<RestrictionOther> messageService) : base(context, messageService)
        {
        }
        public IEnumerable<RestrictionOther> ObterPorUsuario(Guid id)
        {
            return _Db.RestrictionOther.Where(p => p.UserID == id);
        }
    }
}
