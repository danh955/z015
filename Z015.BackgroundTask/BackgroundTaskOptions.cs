// <copyright file="BackgroundTaskOptions.cs" company="None">
// Free and open source code.
// </copyright>

namespace Z015.BackgroundTask
{
    /// <summary>
    /// Background task options class.
    /// </summary>
    public class BackgroundTaskOptions
    {
        /// <summary>
        /// Gets or sets the keep alive URL.
        /// This URL will get used every so often to keep the website running in a shared hosted environment.
        /// Leave blank to disable.
        /// </summary>
        public string KeepAliveUrl { get; set; }

        /// <summary>
        /// Gets or sets the tick delay in minutes.
        /// This tells how often keep alive will happen.
        /// </summary>
        public int? TickDelayMinutes { get; set; }
    }
}