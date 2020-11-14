namespace omg_app.Models
{
    public class ServiceBusModel
    {
        /// <summary>
        /// Gets or sets connection string property.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets queue name property.
        /// </summary>
        public string QueueName { get; set; }
    }
}