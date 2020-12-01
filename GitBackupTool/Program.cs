using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;

namespace GitBackupTool
{
    class Program
    {
        static void Main(string[] args)
        {


            var mins = 60;
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "TIMERMINS.txt"))
            {
                var minsStr = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "TIMERMINS.txt");
                int.TryParse(minsStr, out mins);
            }

            var tmr = new Timer(mins * 1000 * 60);
            tmr.Elapsed += Tmr_Elapsed;
            tmr.Start();
            Console.WriteLine("Timer Start for " + mins + " Mins.");

            ReadAllDatasAndFork();

            while (true)
            {

            }
        }

        private static void Tmr_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Start Fork  => " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            ReadAllDatasAndFork();
        }

        private static void ReadAllDatasAndFork()
        {

            var files = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "targets").GetFiles();
            List<ForkInfo> forkPool = new List<ForkInfo>();
            if (files != null)
            {

                foreach (var f in files)
                {
                    var str = File.ReadAllText(f.FullName);
                    forkPool.Add(JsonConvert.DeserializeObject<ForkInfo>(str));
                }

                foreach (var forkInfo in forkPool)
                {
                    StartFork(forkInfo.SelfToken, forkInfo.SelfUserName, forkInfo.TargetUserName, forkInfo.Id);
                }


            }


        }

        private static void StartFork(string selfToken, string selfName, string targetName, string id)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("GitFork Tool.");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;

            Dictionary<string, long> targetRepos = new Dictionary<string, long>();

            while (string.IsNullOrEmpty(selfName))
            {
                Console.WriteLine("Please give me your Github User Name");
                Console.Write(">");
                selfName = Console.ReadLine();
            }

            while (string.IsNullOrEmpty(selfToken))
            {
                Console.WriteLine("Please give me your GithubToken");
                Console.Write(">");
                selfToken = Console.ReadLine();
            }
            while (string.IsNullOrEmpty(targetName))
            {
                Console.WriteLine("Please give me Target User Name");
                Console.Write(">");
                targetName = Console.ReadLine();
            }


            var client = new GitHubClient(new ProductHeaderValue("APPLEPEN"));
            //從網站上取得的 personal access token https://github.com/settings/tokens 
            var tokenAuth = new Credentials(selfToken); // NOTE: not real token

            client.Credentials = tokenAuth;

            //下面那是你自己的 github name.
            var repository = client.Repository.GetAllForUser(targetName).Result;

            foreach (var repo in repository)
            {
                targetRepos.Add(repo.Name, repo.Id);
                Console.WriteLine(repo.Id + ":" + repo.Name + "\r\n");
            }

            foreach (var repo in targetRepos)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Delete " + repo.Key);
                try
                {
                    var res = client.Repository.Delete(selfName, repo.Key);
                    System.Threading.Thread.Sleep(5000);
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(repo.Key + " is Not Existed.");
                }

            }

            Console.ForegroundColor = ConsoleColor.Green;
            foreach (var repo in targetRepos)
            {
                Console.WriteLine("Fork  " + repo.Key);
                var res2 = client.Repository.Forks.Create(targetName, repo.Key, new NewRepositoryFork()).Result;

            }
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine(id + " Finish.....");
        }
    }



}

