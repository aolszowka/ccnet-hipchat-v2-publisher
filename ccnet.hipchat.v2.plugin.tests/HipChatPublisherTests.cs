// -----------------------------------------------------------------------
// <copyright file="HipChatPublisherTests.cs" company="Ace Olszowka">
// Copyright (c) 2015 Ace Olszowka.
// </copyright>
// -----------------------------------------------------------------------

namespace ccnet.hipchat.v2.plugin.tests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using HipChat.Net.Models.Request;
    using Moq;
    using NUnit.Framework;
    using ThoughtWorks.CruiseControl.Core;

    /// <summary>
    /// Unit tests for the <see cref="HipChatPublisher"/> class.
    /// </summary>
    [TestFixture]
    public class HipChatPublisherTests
    {
        /// <summary>
        ///   Deep Integration Test, this will actually invoke the publisher
        /// and requires that you have an HipChat OAuth2 Token and rooms setup.
        /// </summary>
        /// <param name="integrationResult">An Integration Result.</param>
        /// <param name="notifyOnlyOnError">Should we notify only on error?</param>
        /// <param name="expected">[NOT USED] This is a hack so we can reuse our test cases from another test suite.</param>
        [TestCaseSource(typeof(EvaluateIntegration_ValidInput_Tests)), Explicit("Deep Integration Test"), Ignore("Disabled as it is noisy")]
        public void IntegrationTest(IIntegrationResult integrationResult, bool notifyOnlyOnError, Tuple<bool, SendNotification> expected)
        {
            // DON'T COMMIT THE BELOW
            string authToken = "dontevercheckinyourauthkey";
            string rooms = "dontevercheckinaroomname";
            // DON'T COMMIT THE ABOVE

            HipChatPublisher hcPublisher = new HipChatPublisher();
            hcPublisher.AuthToken = authToken;
            hcPublisher.RoomNames = rooms;
            hcPublisher.NotifyOnlyOnError = notifyOnlyOnError;

            hcPublisher.Run(integrationResult);

            Assert.True(true);
        }

        [TestCaseSource(typeof(EvaluateIntegration_ValidInput_Tests)), Ignore("Broken because third party does not implement Equals")]
        public void EvaluateIntegration_ValidInput(IIntegrationResult integrationResult, bool notifyOnlyOnError, Tuple<bool, SendNotification> expected)
        {
            Tuple<bool, SendNotification> actual = HipChatPublisher.EvaluateIntegration(integrationResult, notifyOnlyOnError);

            Assert.That(actual, Is.EqualTo(expected));
        }
    }

    /// <summary>
    /// The Test Factory for the <see cref="EvaluateIntegration_ValidInput"/> tests.
    /// </summary>
    internal class EvaluateIntegration_ValidInput_Tests : IEnumerable
    {
        /// <summary>
        /// The name used in several of the below tests.
        /// </summary>
        const string BuildName = "SomeBuildName With Spaces";

        /// <summary>
        /// The Build Successful SendNotification for several of the tests.
        /// </summary>
        private SendNotification BuildSuccessfulSendNotification =
            new SendNotification()
            {
                Color = MessageColor.Green,
                Message = "<b>[SUCCESSFUL]</b> SomeBuildName With Spaces",
                Notify = true
            };

        private SendNotification BuildFixedSendNotification =
            new SendNotification()
            {
                Color = MessageColor.Green,
                Message = "<b>[FIXED]</b> SomeBuildName With Spaces",
                Notify = true
            };

        private string[] BuildFailureSingleUser = new string[] { "bob" };

        private string[] BuildFailureMultiUser = new string[] { "bob", "alice" };

        const string BuildFailureUrl = "http://example.com";

        private SendNotification BuildFailedSingleSendNotification =
                        new SendNotification()
                        {
                            Color = MessageColor.Red,
                            Message = "<b>[FAILED]</b> SomeBuildName With Spaces <a href=\"http://example.com\">Build Log</a>. Breakers: @bob",
                            Notify = true
                        };

        private SendNotification BuildFailedMultiSendNotification =
                new SendNotification()
                {
                    Color = MessageColor.Red,
                    Message = "<b>[FAILED]</b> SomeBuildName With Spaces <a href=\"http://example.com\">Build Log</a>. Breakers: @bob,@alice",
                    Notify = true
                };

        public IEnumerator GetEnumerator()
        {
            // Success Cases
            yield return new TestCaseData(MockIIntegrationResultSuccess(BuildName), false, new Tuple<bool, SendNotification>(true, BuildSuccessfulSendNotification)).SetName("SuccessfulBuild_NotifyOnlyOnError_True");
            yield return new TestCaseData(MockIIntegrationResultSuccess(BuildName), true, new Tuple<bool, SendNotification>(false, BuildSuccessfulSendNotification)).SetName("SuccessfulBuild_NotifyOnlyOnError_False");

            // Fixed Cases
            yield return new TestCaseData(MockIIntegrationResultFixed(BuildName), true, new Tuple<bool, SendNotification>(true, BuildFixedSendNotification)).SetName("FixedBuild_NotifyOnlyOnError_True");
            yield return new TestCaseData(MockIIntegrationResultFixed(BuildName), false, new Tuple<bool, SendNotification>(true, BuildFixedSendNotification)).SetName("FixedBuild_NotifyOnlyOnError_False");

            // Failure Cases
            yield return new TestCaseData(MockIIntegrationResultFailure(BuildName, BuildFailureUrl, BuildFailureSingleUser), true, new Tuple<bool, SendNotification>(true, BuildFailedSingleSendNotification)).SetName("FailedBuild_NotifyOnlyOnError_True_SingleUser");
            yield return new TestCaseData(MockIIntegrationResultFailure(BuildName, BuildFailureUrl, BuildFailureMultiUser), false, new Tuple<bool, SendNotification>(true, BuildFailedMultiSendNotification)).SetName("FailedBuild_NotifyOnlyOnError_False_MultiUser");
        }

        private static IIntegrationResult MockIIntegrationResultSuccess(string projectName)
        {
            Mock<IIntegrationResult> result = new Mock<IIntegrationResult>(MockBehavior.Strict);

            result.SetupGet(x => x.ProjectName).Returns(projectName);
            result.SetupGet(x => x.Succeeded).Returns(true);
            result.SetupGet(x => x.Fixed).Returns(false);
            result.SetupGet(x => x.Failed).Returns(false);

            return result.Object;
        }

        private static IIntegrationResult MockIIntegrationResultFixed(string projectName)
        {
            Mock<IIntegrationResult> result = new Mock<IIntegrationResult>(MockBehavior.Strict);

            result.SetupGet(x => x.ProjectName).Returns(projectName);
            result.SetupGet(x => x.Succeeded).Returns(true);
            result.SetupGet(x => x.Fixed).Returns(true);
            result.SetupGet(x => x.Failed).Returns(false);

            return result.Object;
        }

        private static IIntegrationResult MockIIntegrationResultFailure(string projectName, string projectURL, IEnumerable<string> failingUsers)
        {
            Mock<IIntegrationResult> result = new Mock<IIntegrationResult>(MockBehavior.Strict);

            result.SetupGet(x => x.ProjectName).Returns(projectName);
            result.SetupGet(x => x.Succeeded).Returns(false);
            result.SetupGet(x => x.Fixed).Returns(false);
            result.SetupGet(x => x.Failed).Returns(true);
            result.SetupGet(x => x.ProjectUrl).Returns(projectURL);
            result.SetupGet(x => x.FailureUsers).Returns(new ArrayList(failingUsers.ToArray()));

            return result.Object;
        }
    }
}
