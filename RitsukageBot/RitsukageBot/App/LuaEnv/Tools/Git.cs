using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.LuaEnv.Tools
{
    class Git
    {
        public static bool IsValid(string path) => Repository.IsValid(path);

        public static string Init(string path) => Repository.Init(path);

        public static bool Add(string repopath, string file = "*")
        {
            if (!IsValid(repopath))
                return false;
            using var repo = new Repository(repopath);
            if (file == "*")
                Commands.Stage(repo, "*");
            else
            {
                repo.Index.Add(file);
                repo.Index.Write();
            }
            return true;
        }

        public static Commit Commit(string repopath, string content, Signature author, Signature committer = null)
        {
            if (!IsValid(repopath))
                return null;
            using var repo = new Repository(repopath);
            return repo.Commit(content, author, committer ?? author); ;
        }

        public static string Fetch(string repopath, string remotename = "", string username = "", string password = "")
        {
            if (!IsValid(repopath))
                return "";
            string logMessage = "";
            using var repo = new Repository(repopath);
            FetchOptions options = null;
            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                options = new FetchOptions
                {
                    CredentialsProvider = new CredentialsHandler((url, usernameFromUrl, types) =>
                        new UsernamePasswordCredentials()
                        {
                            Username = "USERNAME",
                            Password = "PASSWORD"
                        })
                };
            }
            if (string.IsNullOrWhiteSpace(remotename))
            {
                foreach (Remote remote in repo.Network.Remotes)
                {
                    IEnumerable<string> refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                    Commands.Fetch(repo, remote.Name, refSpecs, options, logMessage);
                }
            }
            else
            {
                var remote = repo.Network.Remotes[remotename];
                var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                Commands.Fetch(repo, remote.Name, refSpecs, options, logMessage);
            }
            return logMessage;
        }

        public static string Clone(string url, string path, string username = "", string password = "")
        {
            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                var co = new CloneOptions
                {
                    CredentialsProvider = (_url, _user, _cred)
                        => new UsernamePasswordCredentials { Username = username, Password = password }
                };
                return Repository.Clone(url, path, co);
            }
            else
                return Repository.Clone(url, path);
        }

        public static Signature GetSignature(string name, string email)
            => new Signature(name, email, DateTime.Now);
    }
}
