using System;
using System.Collections.Generic;

namespace ghd.Webhooks
{
    public class GitHub
    {
        public class PingWebhook
        {
            public string Zen { get; set; }
            public int HookId { get; set; }
            public Hook Hook { get; set; }
            public Repository Repository { get; set; }
            public Sender Sender { get; set; }
        }
        
        public class PushWebhook
        {
            public string Ref { get; set; }
            public string Before { get; set; }
            public string After { get; set; }
            public bool Created { get; set; }
            public bool Deleted { get; set; }
            public bool Forced { get; set; }
            public string BaseRef { get; set; }
            public string Compare { get; set; }
            public List<Commit> Commits { get; set; }
            public Commit HeadCommit { get; set; }
            public Repository Repository { get; set; }
            public GitUser Pusher { get; set; }
            public Sender Sender { get; set; }
        }

        public class Config
        {
            public string ContentType { get; set; }
            public string InsecureSsl { get; set; }
            public string Url { get; set; }
        }

        public class LastResponse
        {
            public object Code { get; set; }
            public string Status { get; set; }
            public object Message { get; set; }
        }

        public class Hook
        {
            public string Type { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
            public bool Active { get; set; }
            public List<string> Events { get; set; }
            public Config Config { get; set; }
            public DateTime UpdatedAt { get; set; }
            public DateTime CreatedAt { get; set; }
            public string Url { get; set; }
            public string TestUrl { get; set; }
            public string PingUrl { get; set; }
            public LastResponse LastResponse { get; set; }
        }
        
        public class GitUser
        {
            public string Name { get; set; }
            public string Email { get; set; }
        }

        public class User
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Username { get; set; }
        }

        public class Commit
        {
            public string Id { get; set; }
            public string TreeId { get; set; }
            public bool Distinct { get; set; }
            public string Message { get; set; }
            public string Timestamp { get; set; }
            public string Url { get; set; }
            public User Author { get; set; }
            public User Committer { get; set; }
            public List<string> Added { get; set; }
            public List<string> Removed { get; set; }
            public List<string> Modified { get; set; }
        }

        public class Repository
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string FullName { get; set; }
            public User Owner { get; set; }
            public bool Private { get; set; }
            public string HtmlUrl { get; set; }
            public string Description { get; set; }
            public bool Fork { get; set; }
            public string Url { get; set; }
            public string ForksUrl { get; set; }
            public string KeysUrl { get; set; }
            public string CollaboratorsUrl { get; set; }
            public string TeamsUrl { get; set; }
            public string HooksUrl { get; set; }
            public string IssueEventsUrl { get; set; }
            public string EventsUrl { get; set; }
            public string AssigneesUrl { get; set; }
            public string BranchesUrl { get; set; }
            public string TagsUrl { get; set; }
            public string BlobsUrl { get; set; }
            public string GitTagsUrl { get; set; }
            public string GitRefsUrl { get; set; }
            public string TreesUrl { get; set; }
            public string StatusesUrl { get; set; }
            public string LanguagesUrl { get; set; }
            public string StargazersUrl { get; set; }
            public string ContributorsUrl { get; set; }
            public string SubscribersUrl { get; set; }
            public string SubscriptionUrl { get; set; }
            public string CommitsUrl { get; set; }
            public string GitCommitsUrl { get; set; }
            public string CommentsUrl { get; set; }
            public string IssueCommentUrl { get; set; }
            public string ContentsUrl { get; set; }
            public string CompareUrl { get; set; }
            public string MergesUrl { get; set; }
            public string ArchiveUrl { get; set; }
            public string DownloadsUrl { get; set; }
            public string IssuesUrl { get; set; }
            public string PullsUrl { get; set; }
            public string MilestonesUrl { get; set; }
            public string NotificationsUrl { get; set; }
            public string LabelsUrl { get; set; }
            public string ReleasesUrl { get; set; }
            public string DeploymentsUrl { get; set; }
            public string CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public string PushedAt { get; set; }
            public string GitUrl { get; set; }
            public string SshUrl { get; set; }
            public string CloneUrl { get; set; }
            public string SvnUrl { get; set; }
            public string Homepage { get; set; }
            public int Size { get; set; }
            public int StargazersCount { get; set; }
            public int WatchersCount { get; set; }
            public string Language { get; set; }
            public bool HasIssues { get; set; }
            public bool HasDownloads { get; set; }
            public bool HasWiki { get; set; }
            public bool HasPages { get; set; }
            public int ForksCount { get; set; }
            public string MirrorUrl { get; set; }
            public int OpenIssuesCount { get; set; }
            public int Forks { get; set; }
            public int OpenIssues { get; set; }
            public int Watchers { get; set; }
            public string DefaultBranch { get; set; }
            public int Stargazers { get; set; }
            public string MasterBranch { get; set; }
        }

        public class Sender
        {
            public string Login { get; set; }
            public int Id { get; set; }
            public string AvatarUrl { get; set; }
            public string GravatarId { get; set; }
            public string Url { get; set; }
            public string HtmlUrl { get; set; }
            public string FollowersUrl { get; set; }
            public string FollowingUrl { get; set; }
            public string GistsUrl { get; set; }
            public string StarredUrl { get; set; }
            public string SubscriptionsUrl { get; set; }
            public string OrganizationsUrl { get; set; }
            public string ReposUrl { get; set; }
            public string EventsUrl { get; set; }
            public string ReceivedEventsUrl { get; set; }
            public string Type { get; set; }
            public bool SiteAdmin { get; set; }
        }
    }
}
