using Fiap.GeekBurguer.Domain.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fiap.GeekBurguer.Persistence.Repository
{
    public class BaseRepository<T> where T : Base
    {
        private MyDbContext _Db;

        public BaseRepository(MyDbContext context)
        {
            _Db = context;
        }
        public void Inserir(T obj)
        {
            _Db.Add(obj);
            _Db.SaveChanges();
        }


        public void InserirLote(List<T> lista)
        {
            _Db.AddRange(lista);
            _Db.SaveChanges();
        }

        public T Obter(Guid id)
        {
            return (T)_Db.Find(typeof(T), id);
        }

        public T Alterar(T objeto)
        {
            var objBase = Obter(objeto.ID);
            objBase = objeto;
            _Db.SaveChanges();
            return objBase;
        }

        public void Remover(T objeto)
        {
            _Db.Remove(objeto);
        }
    }
}
