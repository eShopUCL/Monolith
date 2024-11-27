using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System;
using Microsoft.eShopWeb.PublicApi.Messaging.Messages;
using System.Text.Json;

namespace Microsoft.eShopWeb.PublicApi.Messaging
{
    public class CatalogRequestProducer
    {
        private readonly IModel _channel;

        public CatalogRequestProducer(IModel channel)
        {
            _channel = channel;
        }

        public async Task<List<CatalogItemResponse>> GetCatalogItemsAsync(CatalogRequest CatalogRequest)
        {
            const string deadLetterExchange = "dead-letter-exchange";
            const string deadLetterQueue = "dead-letter-queue";

            //Opret DLQ
            _channel.ExchangeDeclare(exchange: deadLetterExchange, type: ExchangeType.Fanout, durable: true);
            _channel.QueueDeclare(queue: deadLetterQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(queue: deadLetterQueue, exchange: deadLetterExchange, routingKey: "");

            var mainQueueArguments = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", deadLetterExchange }
            };

            var tss = new TaskCompletionSource();

            // Opret Queue og bind til exchange
            _channel.ExchangeDeclare(exchange: "order-exchange", type: ExchangeType.Direct, durable: true);
            _channel.QueueDeclare(queue: "catalog-queue", durable: true, exclusive: false, autoDelete: false, arguments: mainQueueArguments);
            _channel.QueueBind(queue: "catalog-queue", exchange: "order-exchange", routingKey: "request-catalog-items");

            var tcs = new TaskCompletionSource<List<CatalogItemResponse>>();

            // Opret en midlertidig kø til svaret
            var replyQueueName = _channel.QueueDeclare().QueueName;

            // Angiv køens navn i properties for at få et svar
            var props = _channel.CreateBasicProperties();
            props.ReplyTo = replyQueueName;
            props.CorrelationId = Guid.NewGuid().ToString();

            // Lyt på reply-køen
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                if (ea.BasicProperties.CorrelationId == props.CorrelationId)
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var response = JsonSerializer.Deserialize<List<CatalogItemResponse>>(Encoding.UTF8.GetString(body));

                        // Fuldfør TaskCompletionSource med svaret
                        tcs.TrySetResult(response);
                    }
                    catch (Exception ex)
                    {
                        // Håndter inkonsistens eller fejl i deserialisering
                        tcs.TrySetException(new Exception("Failed to deserialize response.", ex));
                    }
                }
                else
                {
                    // Håndter inkonsistens ved ikke-matching CorrelationId
                    tcs.TrySetException(new Exception("CorrelationId mismatch. Received unexpected response."));
                }
            };
            _channel.BasicConsume(queue: replyQueueName, autoAck: true, consumer: consumer);

            // Timeout håndtering
            var timeout = Task.Delay(10000); // 10 sekunder timeout
            _ = Task.Run(async () =>
            {
                await timeout;
                if (!tcs.Task.IsCompleted)
                {
                    tcs.TrySetException(new TimeoutException("No response received within the timeout period."));
                }
            });

            // Send anmodningen
            var message = CatalogRequest;
            var messageBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            _channel.BasicPublish(
                exchange: "catalog-exchange",
                routingKey: "request-catalog-items",
                basicProperties: props,
                body: messageBody
            );

            // Vent på svaret
            return await tcs.Task;
        }
    }
}
