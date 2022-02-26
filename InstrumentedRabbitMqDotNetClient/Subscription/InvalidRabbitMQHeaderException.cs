using System;

namespace InstrumentedRabbitMqDotNetClient.Subscription
{
    /// <summary>
    /// Represents an error encountered while retrieving the headers from a received message.
    /// </summary>
    public class InvalidRabbitMQHeaderException : Exception
    {
        /// <summary>
        /// Creates a new instance of the <see cref="InvalidRabbitMQHeaderException"/> class with the specified error message;
        /// </summary>
        /// <param name="errorMessage">The error message explaining why this exception happened.</param>
        public InvalidRabbitMQHeaderException(string errorMessage) : base(errorMessage)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="InvalidRabbitMQHeaderException"/> class with the specified error message;
        /// </summary>
        /// <param name="errorMessage">The error message explaining why this exception happened.</param>
        /// <param name="innerException">The exception that was thrown while trying to get a header value.</param>
        public InvalidRabbitMQHeaderException(string errorMessage, Exception innerException) : base(errorMessage, innerException)
        {
        }
    }
}