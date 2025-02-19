﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NUnit.Framework;
using SharpBucket.V2;
using SharpBucket.V2.EndPoints;
using SharpBucket.V2.Pocos;
using SharpBucketTests.V2.Pocos;
using Shouldly;

namespace SharpBucketTests.V2.EndPoints
{
    [TestFixture]
    internal class RepositoryResourceTests
    {
        [Test]
        public void GetRepository_FromMercurialRepo_CorrectlyFetchesTheRepoInfo()
        {
            var repositoryResource = SampleRepositories.MercurialRepository;
            repositoryResource.ShouldNotBe(null);
            var testRepository = repositoryResource.GetRepository();

            testRepository.ShouldBeFilled();
            testRepository.name.ShouldBe(SampleRepositories.MERCURIAL_REPOSITORY_NAME);
            testRepository.website.ShouldNotBeNullOrEmpty(); // this repository is an example of one where website is filled
        }

        [Test]
        public void ListWatchers_FromMercurialRepo_ShouldReturnMoreThan10UniqueWatchers()
        {
            var repositoryResource = SampleRepositories.MercurialRepository;
            repositoryResource.ShouldNotBe(null);
            var watchers = repositoryResource.ListWatchers();
            watchers.ShouldNotBe(null);
            watchers.Count.ShouldBeGreaterThan(10);

            var uniqueNames = new HashSet<string>();
            foreach (var watcher in watchers)
            {
                watcher.ShouldBeFilled();
                uniqueNames.ShouldNotContain(watcher.uuid);
                uniqueNames.Add(watcher.uuid);
            }
        }

        [Test]
        public void ListForks_FromMercurialRepo_ShouldReturnMoreThan10UniqueForks()
        {
            var repositoryResource = SampleRepositories.MercurialRepository;
            repositoryResource.ShouldNotBe(null);
            var forks = repositoryResource.ListForks();
            forks.ShouldNotBe(null);
            forks.Count.ShouldBeGreaterThan(10);

            var uniqueNames = new HashSet<string>();
            foreach (var fork in forks)
            {
                fork.ShouldBeFilled();

                // since they are forks of mercurial, their parent should be mercurial
                fork.parent.ShouldBeFilled();
                fork.parent.name.ShouldBe(SampleRepositories.MERCURIAL_REPOSITORY_NAME);

                uniqueNames.ShouldNotContain(fork.full_name);
                uniqueNames.Add(fork.full_name);
            }
        }

        [TestCase(3)]
        [TestCase(103)]
        [Test]
        public void ListCommits_FromMercurialRepoWithSpecifiedMax_ShouldReturnSpecifiedNumberOfCommits(int max)
        {
            var repositoryResource = SampleRepositories.MercurialRepository;
            repositoryResource.ShouldNotBe(null);
            var commits = repositoryResource.ListCommits(max: max);
            commits.Count.ShouldBe(max);
        }

        [Test]
        public void ListCommits_OnASpecifiedBranch_ShouldReturnTheRightNumberOfCommits()
        {
            var repositoryResource = SampleRepositories.TestRepository.RepositoryResource;

            var allCommits = repositoryResource.ListCommits();
            var commitsOnMaster = repositoryResource.ListCommits("master");
            var commitsOnToAccept = repositoryResource.ListCommits("branchToAccept");
            var commitsOnToDecline = repositoryResource.ListCommits("branchToDecline");

            allCommits.Count.ShouldBe(5);
            commitsOnMaster.Count.ShouldBe(2);
            commitsOnToAccept.Count.ShouldBe(4);
            commitsOnToDecline.Count.ShouldBe(3);
        }

        [Test]
        public void ListCommits_ExcludingABranch_ShouldReturnTheRightNumberOfCommits()
        {
            var repositoryResource = SampleRepositories.TestRepository.RepositoryResource;

            var commits = repositoryResource.ListCommits(new CommitsParameters { Excludes = { "master" } });

            commits.Count.ShouldBe(3);
        }

        [Test]
        public void ListCommits_CombiningAnIncludeAndAnExclude_ShouldReturnTheRightNumberOfCommits()
        {
            var repositoryResource = SampleRepositories.TestRepository.RepositoryResource;

            var commits = repositoryResource.ListCommits(new CommitsParameters { Includes = { "branchToDecline" }, Excludes = { "master" } });

            commits.Count.ShouldBe(1);
        }

        [Test]
        public void ListCommits_OfABranchExcludingMaster_ShouldReturnOnlyBranchCommits()
        {
            var repositoryResource = SampleRepositories.TestRepository.RepositoryResource;

            var commits = repositoryResource.ListCommits("branchToAccept", new CommitsParameters { Excludes = { "master" } });

            commits.Count.ShouldBe(2);
        }

        [Test]
        public void ListCommits_WithAPath_ShouldReturnTheRightNumberOfCommits()
        {
            var repositoryResource = SampleRepositories.TestRepository.RepositoryResource;

            var commits = repositoryResource.ListCommits(new CommitsParameters { Path = "src/" });

            commits.Count.ShouldBe(4); // only the bad commit in branchToDecline do not change anything in src path
        }

        [Test]
        public void GetCommit_AKnownHashOnMercurialRepository_ShouldReturnCorrectData()
        {
            var repositoryResource = SampleRepositories.MercurialRepository;

            var commit = repositoryResource.GetCommit("abae1eb695c077fa21b6ef0b7056f36d63cf0302");

            commit.ShouldNotBeNull();
            commit.hash.ShouldBe("abae1eb695c077fa21b6ef0b7056f36d63cf0302");
            commit.date.ShouldNotBeNullOrWhiteSpace();
            commit.message.ShouldNotBeNullOrWhiteSpace();
            commit.author.raw.ShouldNotBeNullOrWhiteSpace();
            commit.author.user.ShouldBeFilled();
            commit.links.ShouldNotBeNull();
            commit.parents[0].ShouldBeFilled();
            commit.repository.uuid.ShouldNotBeNullOrWhiteSpace();
            commit.repository.full_name.ShouldNotBeNullOrWhiteSpace();
            commit.repository.name.ShouldNotBeNullOrWhiteSpace();
            commit.repository.links.ShouldNotBeNull();
            commit.summary.ShouldBeFilled();
        }

        [Test]
        public void CreateRepository_NewPublicRepository_CorrectlyCreatesTheRepository()
        {
            var accountName = TestHelpers.AccountName;
            var repositoryName = Guid.NewGuid().ToString("N");
            var repositoryResource = SampleRepositories.RepositoriesEndPoint.RepositoryResource(accountName, repositoryName);
            var repository = new Repository
            {
                name = repositoryName,
                language = "c#",
                scm = "git"
            };

            var repositoryFromPost = repositoryResource.PostRepository(repository);
            repositoryFromPost.name.ShouldBe(repositoryName);
            repositoryFromPost.scm.ShouldBe("git");
            repositoryFromPost.language.ShouldBe("c#");

            var repositoryFromGet = repositoryResource.GetRepository();
            repositoryFromGet.name.ShouldBe(repositoryName);
            repositoryFromGet.scm.ShouldBe("git");
            repositoryFromGet.language.ShouldBe("c#");

            repositoryFromPost.full_name.ShouldBe(repositoryFromGet.full_name);
            repositoryFromPost.uuid.ShouldBe(repositoryFromGet.uuid);

            repositoryResource.DeleteRepository();
        }

        [Test]
        public void CreateRepository_InATeamWhereIHaveNoRights_ThrowAnException()
        {
            var repositoryName = Guid.NewGuid().ToString("N");
            var repositoryResource = SampleRepositories.RepositoriesEndPoint.RepositoryResource(SampleRepositories.MERCURIAL_ACCOUNT_NAME, repositoryName);
            var repository = new Repository
            {
                name = repositoryName
            };

            var exception = Assert.Throws<BitbucketV2Exception>(() => repositoryResource.PostRepository(repository));
            exception.HttpStatusCode.ShouldBe(HttpStatusCode.Forbidden);
            exception.ErrorResponse.error.message.ShouldBe("You cannot administer other userspersonal accounts.");
        }

        [Test]
        public void ApproveCommitAndDeleteCommitApproval_TestRepository_CommitStateChangedCorrectly()
        {
            var currentUser = TestHelpers.AccountName;
            var testRepository = SampleRepositories.TestRepository;
            var repositoryResource = testRepository.RepositoryResource;
            var firstCommit = testRepository.RepositoryInfo.FirstCommit;
            var initialCommit = repositoryResource.GetCommit(firstCommit);
            initialCommit?.participants.Any(p => p.User.nickname == currentUser && p.Approved).ShouldBe(false, "Initial state should be: 'not approved'");

            var userRole = repositoryResource.ApproveCommit(firstCommit);
            var approvedCommit = repositoryResource.GetCommit(firstCommit);
            repositoryResource.DeleteCommitApproval(firstCommit);
            var notApprovedCommit = repositoryResource.GetCommit(firstCommit);

            userRole.Approved.ShouldBe(true);
            userRole.User.nickname.ShouldBe(currentUser);
            userRole.Role.ShouldBe("PARTICIPANT");
            approvedCommit?.participants.Any(p => p.User.nickname == currentUser && p.Approved).ShouldBe(true, "Commit should be approved after call to ApproveCommit");
            notApprovedCommit?.participants.Any(p => p.User.nickname == currentUser && p.Approved).ShouldBe(false, "Commit should not be approved after call to DeleteCommitApproval");
        }

        [Test]
        public void BuildStatusInfo_AddGetChangeOnFirstCommit_ShouldWork()
        {
            var testRepository = SampleRepositories.TestRepository;
            var repositoryResource = testRepository.RepositoryResource;
            var firstCommit = testRepository.RepositoryInfo.FirstCommit;

            var firstBuildStatus = new BuildInfo
            {
                key = "FooBuild42",
                state = BuildInfoState.INPROGRESS,
                url = "https://foo.com/builds/{repository.full_name}",
                name = "Foo Build #42",
                description = "fake build status from a fake build server"
            };
            var buildInfo = repositoryResource.AddNewBuildStatus(firstCommit, firstBuildStatus);
            buildInfo.ShouldNotBeNull();
            buildInfo.state.ShouldBe(firstBuildStatus.state);
            buildInfo.name.ShouldBe(firstBuildStatus.name);
            buildInfo.description.ShouldBe(firstBuildStatus.description);

            var getBuildInfo = repositoryResource.GetBuildStatusInfo(firstCommit, "FooBuild42");
            getBuildInfo.ShouldNotBeNull();
            getBuildInfo.state.ShouldBe(BuildInfoState.INPROGRESS);

            getBuildInfo.state = BuildInfoState.SUCCESSFUL;
            var changedBuildInfo = repositoryResource.ChangeBuildStatusInfo(firstCommit, "FooBuild42", getBuildInfo);
            changedBuildInfo.ShouldNotBeNull();
            changedBuildInfo.state.ShouldBe(BuildInfoState.SUCCESSFUL);
        }
    }
}