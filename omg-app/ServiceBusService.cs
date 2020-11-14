namespace omg_app
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using omg_app.Models;

    public static class ServiceBusService
    {
        /// <summary>
        /// Access lock object private field.
        /// </summary>
        private static readonly object AccessLock = new object();

        /// <summary>
        /// Service bus queue client private field.
        /// </summary>
        private static IQueueClient queueClient = null;

        /// <summary>
        /// Emit new message in the service bus queue.
        /// </summary>
        /// <param name="message">String message to be emitted.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task AddMessageAsync(string message)
        {
            var messageObj = new Message(Encoding.UTF8.GetBytes(message));

            await queueClient.SendAsync(messageObj);
        }

        /// <summary>
        /// Setup service bus.
        /// </summary>
        /// <param name="configurationModel">Configuration event bus parameter.</param>
        /// <param name="createListener">Determines whether a listener should be configured.</param>
        public static void SetupServiceBus(ServiceBusModel configurationModel, bool createListener = true)
        {
            lock (AccessLock)
            {
                if (queueClient == null)
                {
                    queueClient = new QueueClient(configurationModel.ConnectionString, configurationModel.QueueName);

                    if (createListener)
                    {
                        StartListening();
                    }
                }
            }
        }

        /// <summary>
        /// Configure message listener.
        /// </summary>
        private static void StartListening()
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of Concurrent calls to the callback `ProcessMessagesAsync`, set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                AutoComplete = false
            };

            queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        /// <summary>
        /// Message processor. When new message is emitted in service bus queue this method is being executed.
        /// </summary>
        /// <param name="message">Message parameter.</param>
        /// <param name="token">Cancellation token parameter.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            // Process the message. Currently only messages for success sync and clear cache jobs make changes to the API. All other are skipped.
            var messageText = Encoding.UTF8.GetString(message.Body);

            Store.Message = messageText;

            // Complete the message so that it is not received again.
            // This can be done only if the queueClient is created in ReceiveMode.PeekLock mode (which is default).
            await queueClient.CompleteAsync(message.SystemProperties.LockToken);

            // Note: Use the cancellationToken passed as necessary to determine if the queueClient has already been closed.
            // If queueClient has already been Closed, you may chose to not call CompleteAsync() or AbandonAsync() etc. calls
            // to avoid unnecessary exceptions.
        }

        /// <summary>
        /// Service bus exception handler.
        /// </summary>
        /// <param name="exceptionReceivedEventArgs">Exception received event args.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;

            // This section is used to log the errors during web job execution.
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");

            return Task.CompletedTask;
        }
    }
}