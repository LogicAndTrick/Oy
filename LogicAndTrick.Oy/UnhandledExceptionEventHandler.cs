using System;

namespace LogicAndTrick.Oy
{
    /// <summary>
    /// Called when an exception is thrown while publishing a message.
    /// </summary>
    public delegate void UnhandledExceptionEventHandler(object sender, UnhandledExceptionEventArgs e);

    /// <summary>
    /// The event args of an unhandled exception.
    /// </summary>
    public class UnhandledExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// The exception that was thrown
        /// </summary>
        public Exception Exception { get; }
        
        /// <summary>
        /// Set to true to stop publishing this message.
        /// </summary>
        public bool StopPublishing { get; set; }

        public UnhandledExceptionEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}