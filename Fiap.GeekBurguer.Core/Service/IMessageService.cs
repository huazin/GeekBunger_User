using Fiap.GeekBurguer.Domain.Model;
using Microsoft.Azure.ServiceBus;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fiap.GeekBurguer.Core.Service
{
    public interface IMessageService<T> where T : Base
    {
        void AddToMessageList(IEnumerable<EntityEntry<T>> changes);
        void SendMessageAsync();
        void SendMessagesAsync();
    }
}
