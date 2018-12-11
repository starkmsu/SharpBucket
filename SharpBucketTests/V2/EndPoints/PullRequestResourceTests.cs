﻿using NUnit.Framework;
using SharpBucket.V2.EndPoints;
using Shouldly;

namespace SharpBucketTests.V2.EndPoints
{
    [TestFixture]
    internal class PullRequestResourceTests
    {
        private PullRequestResource ExistingPullRequest { get; set; }

        private PullRequestResource NotExistingPullRequest { get; set; }

        [OneTimeSetUp]
        protected void Init()
        {
            // pull request number 2 on MercurialRepository is public and declined
            // so we could expect that it will be always accessible and won't change
            // which is what we need to have reproducible tests
            ExistingPullRequest = SampleRepositories.MercurialRepository.PullRequestsResource().PullRequestResource(2);

            // there is no change that a pull request with the max value of int32 exist one day
            NotExistingPullRequest = SampleRepositories.MercurialRepository.PullRequestsResource().PullRequestResource(int.MaxValue);
        }

        [Test]
        public void GetPullRequest_ExistingPublicPullRequest_ReturnValidInfo()
        {
            var pullRequest = ExistingPullRequest.GetPullRequest();
            pullRequest.ShouldNotBeNull();
            pullRequest.id.ShouldBe(2);
            pullRequest.author?.username.ShouldBe("goodtune");
            pullRequest.title.ShouldBe("Selective read/write or read-only repos with hg-ssh");
            pullRequest.state.ShouldBe("DECLINED");
        }

        [Test]
        public void GetPullRequest_NotExistingPublicPullRequest_ReturnNull()
        {
            var pullRequest = NotExistingPullRequest.GetPullRequest();
            pullRequest.ShouldBeNull();
        }

        [Test]
        public void GetPullRequestActivity_ExistingPublicPullRequest_ReturnValidInfo()
        {
            var activities = ExistingPullRequest.GetPullRequestActivity();
            activities.ShouldNotBeNull();
            activities.Count.ShouldBe(4);
            activities[activities.Count - 4].update.state.ShouldBe("DECLINED");
        }

        [Test]
        public void GetPullRequestActivity_NotExistingPublicPullRequest_ReturnEmpty()
        {
            var activities = NotExistingPullRequest.GetPullRequestActivity();
            activities.ShouldBeEmpty();
        }

        [Test]
        public void ListPullRequestComments_ExistingPublicPullRequest_ReturnValidInfo()
        {
            var comments = ExistingPullRequest.ListPullRequestComments();
            comments.ShouldNotBeNull();
            comments.Count.ShouldBe(2);
            comments[0].content.raw.ShouldBe("This repo is not used for development, it's just a mirror (and I am just an infrequent contributor). Please consult http://mercurial.selenic.com/wiki/ContributingChanges and send your patch to ``mercurial-devel`` ML.");
        }

        [Test]
        public void ListPullRequestComments_NotExistingPublicPullRequest_ReturnEmpty()
        {
            var comments = NotExistingPullRequest.ListPullRequestComments();
            comments.ShouldBeEmpty();
        }

        [Test]
        public void GetPullRequestComment_ExistingCommentOnAPublicPullRequest_ReturnValidInfo()
        {
            var comment = ExistingPullRequest.GetPullRequestComment(53789);
            comment.ShouldNotBeNull();
            comment.content.raw.ShouldBe("This repo is not used for development, it's just a mirror (and I am just an infrequent contributor). Please consult http://mercurial.selenic.com/wiki/ContributingChanges and send your patch to ``mercurial-devel`` ML.");
        }

        [Test]
        public void GetPullRequestComment_NotExistingCommentOnPublicPullRequest_ReturnEmpty()
        {
            var comment = ExistingPullRequest.GetPullRequestComment(int.MaxValue);
            comment.ShouldNotBeNull();
            comment.id.ShouldBeNull();
        }

        [Test]
        public void ListPullRequestCommits_ExistingPublicPullRequest_ReturnValidInfo()
        {
            var commits = ExistingPullRequest.ListPullRequestCommits();
            commits.ShouldNotBeNull();
            commits.Count.ShouldBe(2);
            commits[0].message.ShouldBe("Update the docstring");
        }

        [Test]
        public void ListPullRequestCommits_NotExistingPublicPullRequest_ReturnEmpty()
        {
            var commits = NotExistingPullRequest.ListPullRequestCommits();
            commits.ShouldBeEmpty();
        }

        [Test]
        public void GetDiffForPullRequest_ExistingPublicPullRequest_ReturnValidInfo()
        {
            var diff = ExistingPullRequest.GetDiffForPullRequest();
            diff.ShouldNotBeNull();
            // TODO: to complete once the right POCO will be returned
        }

        [Test]
        public void GetDiffForPullRequest_NotExistingPublicPullRequest_ReturnEmpty()
        {
            var diff = NotExistingPullRequest.GetDiffForPullRequest();
            diff.ShouldNotBeNull();
            // TODO: to complete once the right POCO will be returned
        }
    }
}
