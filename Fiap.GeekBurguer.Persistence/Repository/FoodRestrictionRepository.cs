using System;
using System.Collections.Generic;
using System.Linq;
using Fiap.GeekBurguer.Core.Service;
using Fiap.GeekBurguer.Domain.Model;

namespace Fiap.GeekBurguer.Persistence.Repository
{
    public class FoodRestrictionRepository
    {
        protected RestrictionRepository _DbRestriction;
        protected RestrictionOtherRepository _DbOther;
        protected IMessageService<FoodRestrictions> _messageService;
        public FoodRestrictionRepository(RestrictionRepository restriction, RestrictionOtherRepository other, IMessageService<FoodRestrictions> messageService)
        {
            _messageService = messageService;
            _DbRestriction = restriction;
            _DbOther = other;
        }

        public void Inserir(FoodRestrictions obj)
        {
            _DbOther.Inserir(obj.Others);
            _DbRestriction.InserirLote(obj.Restrictions);
        }


        public void InserirLote(List<FoodRestrictions> lista)
        {
            foreach (var item in lista)
            {
                Inserir(item);
            }
        }

        public FoodRestrictions Obter(Guid id)
        {
            return new FoodRestrictions()
            {
                UserId = id,
                Others = _DbOther.ObterPorUsuario(id).FirstOrDefault(),
                Restrictions = _DbRestriction.ObterPorUsuario(id).ToList()
            };
        }

        public FoodRestrictions Alterar(FoodRestrictions objeto)
        {
            foreach (var item in objeto.Restrictions)
            {
                _DbRestriction.Alterar(item);
            }
            _DbOther.Alterar(objeto.Others);

            return objeto;
        }

        public void Remover(FoodRestrictions objeto)
        {
            foreach (var item in objeto.Restrictions)
            {
                _DbRestriction.Remover(item);
            }
            _DbOther.Remover(objeto.Others);
        }

    }
}
