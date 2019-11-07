using Fiap.GeekBurguer.Domain.Model;
using Microsoft.Azure.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fiap.GeekBurguer.Core.Service
{
    public class MessageService<T> : IMessageService<T> where T : Base
    {
        public IConfiguration Configuration { get; set; }
        public Task _lastTask { get; set; }
        public List<Message> _messages { get; set; }
        public MessageService(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void AddToMessageList(IEnumerable<EntityEntry<T>> changes)
        {
            _messages.AddRange(changes
            .Where(entity => entity.State != EntityState.Detached && entity.State != EntityState.Unchanged).Select(entity => GetMessage(entity)));
        }

        private Message GetMessage(EntityEntry<T> entity)
        {
            //var productChanged = Mapper.Map<ProductChangedMessage>(entity);
            var productChangedSerialized = JsonConvert.SerializeObject(entity);
            var productChangedByteArray = Encoding.UTF8.GetBytes(productChangedSerialized);
            return new Message
            {
                Body = productChangedByteArray,
                MessageId = Guid.NewGuid().ToString(),
                Label = entity.Entity.ID.ToString()
            };
        }

        public async void SendMessageAsync()
        {
            var connectionString = Configuration["connectionStrings:serviceBusConnectionString"];
            var queueClient = new QueueClient(connectionString, "ProductChanged");
            int tries = 0;
            Message message;
            while (true)
            {
                if ((_messages.Count <= 0) || (tries > 10))
                    break;
                lock (_messages)
                {
                    message = _messages.FirstOrDefault();
                }
                await queueClient.SendAsync(message);
                _messages.Remove(message);
            }
            await queueClient.CloseAsync();
        }

        public async void SendMessagesAsync()
        {
            if (_lastTask != null && !_lastTask.IsCompleted)
                return;
            var connectionString = Configuration["connectionStrings:serviceBusConnectionString"];
            var queueClient = new QueueClient(connectionString, "ProductChanged");
            _lastTask = SendAsync(queueClient);
            await _lastTask;
            var closeTask = queueClient.CloseAsync();
            await closeTask;
            HandleException(closeTask);
        }
        private async Task SendAsync(QueueClient queueClient)
        {
            int tries = 0;
            Message message;
            while (true)
            {
                if (_messages.Count <= 0)
                    break;
                lock (_messages)
                {
                    message = _messages.FirstOrDefault();
                }
                var sendTask = queueClient.SendAsync(message);
                await sendTask;
                var success = HandleException(sendTask);
                if (!success)
                    Thread.Sleep(10000 * (tries < 60 ? tries++ : tries));
                else
                    _messages.Remove(message);
            }
        }

        public bool HandleException(Task task)
        {
            if (task.Exception == null || task.Exception.InnerExceptions.Count == 0) return true;

            task.Exception.InnerExceptions.ToList().ForEach(innerException =>
            {
                Console.WriteLine($"Error in SendAsync task: {innerException.Message}. Details:{innerException.StackTrace} ");

                if (innerException is ServiceBusCommunicationException)
                    Console.WriteLine("Connection Problem with Host. Internet Connection can be down");
            });

            return false;
        }
    }
}
        
