using Hangfire;
using Hangfire.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace HfLibrary
{
    public class Client
    {
        private string ConectionString { get; } = @"Data Source=NUCBOX\SQLEXPRESS;Initial Catalog=hangfire;Integrated Security=True";
        private BackgroundJobClient hfClient { get; set; }
        public Client()
        {
            var entry = GlobalConfiguration.Configuration.UseSqlServerStorage(ConectionString);
            var current = entry.Entry;
            var states = current.GetMonitoringApi();
            List<RecurringJobDto> recurringJobs = JobStorage.Current.GetConnection().GetRecurringJobs();
            recurringJobs.ForEach(x => Console.WriteLine(x.Id));
            hfClient = new BackgroundJobClient(current);

            CreateJob();

            //var jobId = BackgroundJob.Enqueue(() => Console.WriteLine("Pozdrav z klienta !"));
            //var jobId2 = hfClient.ContinueWith(jobId, () => Console.WriteLine("Následuje po pozdravu z klienta !"));
            ////var job = new Hangfire.Common.Job(() => Console.WriteLine("Pozdrav z klienta !"));
            //Console.WriteLine($"jobId={jobId},jobId2={jobId2}");

            //System.Threading.Thread.Sleep(2000);

            //Console.WriteLine(hfClient.RetryAttempts);

            //if (hfClient.Delete(jobId, "Succeeded"))
            //    Console.WriteLine($"Succeeded deleted {jobId}");

            var monitoringApi = JobStorage.Current.GetMonitoringApi();
            var deletedJobs = monitoringApi.DeletedJobs(0, 10);
            // If no queue name was defined somewhere, probably this will be "default".
            // If no items have been queued, then the queue won't exist, and it will error here.
            var queue = monitoringApi.Queues().First().Name;

            var enqueudjobs = monitoringApi.SucceededJobs(0, 10);
            enqueudjobs.ForEach(x => hfClient.Delete(x.Key));
        }
        public void JobUpdate(string recurringJobId,Hangfire.Common.Job job, string interval)
        {
            var manager = new RecurringJobManager();
            //manager.RemoveIfExists("my-job-id");
            manager.AddOrUpdate(recurringJobId, job, interval);
            manager.Trigger(recurringJobId);
        }

        public void CreateJob()
        {
            var filterValue = "YYY";
            var value = Expression.Constant(filterValue);
            var prm = Expression.Parameter(typeof(SamplePlugin.Plugin), "output");
            ParameterExpression[] pe = new ParameterExpression[] { prm };
            Type[] types = new Type[] { typeof(string) , typeof(string) };
            var run = typeof(SamplePlugin.Plugin).GetMethod(nameof(SamplePlugin.Plugin.Kuk), types);
            var nameJob = string.Empty;
            if (false)
            {

                MethodCallExpression body = Expression.Call(Expression.Constant(new SamplePlugin.Plugin()),run,value,Expression.Constant("0"));
                Expression<Action<SamplePlugin.Plugin>> lambda = Expression.Lambda<Action<SamplePlugin.Plugin>>(body,"robot", pe);

                nameJob = hfClient.Create<SamplePlugin.Plugin>(lambda, new Hangfire.States.EnqueuedState());

                //hfClient.Schedule(lambda, TimeSpan.FromSeconds(30));

                //RecurringJob.AddOrUpdate("sample", lambda, Cron.Minutely);
            }
            else
            {

                Hangfire.Common.Job job = new Hangfire.Common.Job(run, "sss", "0");
                JobUpdate("testR1", job, "* * * * *");
                //nameJob = hfClient.Create(job, new Hangfire.States.EnqueuedState());
            }

            Console.WriteLine($"nameJob = {nameJob}");
            //hfClient.Requeue(nameJob);
        }
    }
}
