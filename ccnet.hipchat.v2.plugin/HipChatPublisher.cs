// -----------------------------------------------------------------------
// <copyright file="HipChatPublisher.cs" company="Ace Olszowka">
// Copyright (c) 2015 Ace Olszowka.
// </copyright>
// -----------------------------------------------------------------------

namespace ccnet.hipchat.v2.plugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Exortech.NetReflector;
    using HipChat.Net;
    using HipChat.Net.Http;
    using HipChat.Net.Models.Request;
    using ThoughtWorks.CruiseControl.Core;

    /// <summary>
    /// A CruiseControl.NET Publisher that Publishes to HipChat.
    /// </summary>
    [ReflectorType("hipchat")]
    public class HipChatPublisher : ITask
    {
        /// <summary>
        /// Gets or sets the OAuth 2 bearer token for use with the HipChat v2 API.
        /// </summary>
        /// <remarks>See https://hipchat.com/account/api for more information or to generate your key.</remarks>
        [ReflectorProperty("auth-token")]
        public string AuthToken { get; set; }

        /// <summary>
        /// Gets or sets the Room Name to receive notification.
        /// </summary>
        [ReflectorProperty("room-name")]
        public string RoomName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not a notification should only be sent on error.
        /// </summary>
        /// <remarks>
        ///     It is recommended that you set this property, otherwise you
        /// will be notified of every successful build which can become very
        /// chatty.
        ///     You will still be notified when a build is "Fixed" if the build
        /// was previously broken, which in most scenarios is what you want. In
        /// this iteration there is no way to turn off these fixed alerts.
        /// </remarks>
        [ReflectorProperty("notify-only-on-error", Required = false)]
        public bool NotifyOnlyOnError { get; set; }

        /// <summary>
        /// Executed when this Publisher is Invoked.
        /// </summary>
        /// <param name="result">The result of the Integration being ran.</param>
        public void Run(IIntegrationResult result)
        {
            Tuple<bool, SendNotification> evaluatedIntergration = EvaluateIntegration(result, this.NotifyOnlyOnError);

            if (evaluatedIntergration.Item1)
            {
                var hipchatClient =
                    new HipChatClient(new ApiConnection(new Credentials(this.AuthToken)));

                // Wait for the message to be posted
                hipchatClient.Rooms.SendNotificationAsync(this.RoomName, evaluatedIntergration.Item2).Wait();
            }
        }

        /// <summary>
        /// Evaluate the given <see cref="IIntegrationResult"/> for the appropriate HipChatPublisher action.
        /// </summary>
        /// <param name="result">The <see cref="IIntegrationResult"/>.</param>
        /// <param name="notifyOnlyOnError">Indicates whether or not we should only notify on error.</param>
        /// <returns>
        /// A Tuple where the first value indicates whether or not anything
        /// should be posted to HipChat and the second value contains the
        /// message to post.
        /// </returns>
        internal static Tuple<bool, SendNotification> EvaluateIntegration(IIntegrationResult result, bool notifyOnlyOnError)
        {
            bool shouldSendRoomNotification = true;
            SendNotification message = null;

            if (result.Failed)
            {
                message = IntegrationFailedMessage(result);
            }
            else if (result.Succeeded || result.Fixed)
            {
                // The check for Fixed here is intentional, the CruiseControl.NET
                // Documentation is not clear on if you are always guaranteed to
                //  Have a Succeeded build that is also Fixed.
                if (result.Fixed)
                {
                    message = IntegrationFixedMessage(result);
                }
                else
                {
                    // This success was "yet another successful build" check
                    // Check to see if we should log these.
                    if (notifyOnlyOnError)
                    {
                        shouldSendRoomNotification = false;
                    }

                    message = IntegrationSuccessfulMessage(result);
                }
            }
            else
            {
                // In theory we should never hit this, but is probably useful
                // when attempting to port this to different CCNET Versions.
                System.Diagnostics.Trace.WriteLine("Unknown Integration Status!");
            }

            return new Tuple<bool, SendNotification>(shouldSendRoomNotification, message);
        }

        /// <summary>
        /// Generates the message displayed in HipChat when the Integration fails.
        /// </summary>
        /// <param name="result">The <see cref="IIntegrationResult"/>.</param>
        /// <returns>The message to be sent to HipChat when the Integration fails.</returns>
        internal static SendNotification IntegrationFailedMessage(IIntegrationResult result)
        {
            // Append @ for Mentions, Currently this is broken, but a message is out to
            // HipChat Support to see if we can get support for this when using an HTML
            IEnumerable<string> mentions =
                result.FailureUsers.ToArray().Select(failureUser => "@" + failureUser);
            string failureUsers = string.Join(",", mentions);

            SendNotification failedMessage =
                new SendNotification()
                {
                    Color = MessageColor.Red,
                    Message = string.Format("<b>[FAILED]</b> {0} <a href=\"{1}\">Build Log</a>. Breakers: {2}", result.ProjectName, result.ProjectUrl, failureUsers),
                    Notify = true
                };

            return failedMessage;
        }

        /// <summary>
        /// Generates the message displayed in HipChat when the Integration is Fixed.
        /// </summary>
        /// <param name="result">The <see cref="IIntegrationResult"/>.</param>
        /// <returns>The message to be sent to HipChat when the Integration is Fixed.</returns>
        internal static SendNotification IntegrationFixedMessage(IIntegrationResult result)
        {
            SendNotification fixedMessage =
                new SendNotification()
                {
                    Color = MessageColor.Green,
                    Message = string.Format("<b>[FIXED]</b> {0}", result.ProjectName),
                    Notify = true
                };

            return fixedMessage;
        }

        /// <summary>
        /// Generates the message displayed in HipChat when the Integration is Successful.
        /// </summary>
        /// <param name="result">The <see cref="IIntegrationResult"/>.</param>
        /// <returns>The message to be sent to HipChat when the Integration is Successful.</returns>
        internal static SendNotification IntegrationSuccessfulMessage(IIntegrationResult result)
        {
            SendNotification successMessage =
                new SendNotification()
                {
                    Color = MessageColor.Green,
                    Message = string.Format("<b>[SUCCESSFUL]</b> {0}", result.ProjectName),
                    Notify = true
                };

            return successMessage;
        }
    }
}
