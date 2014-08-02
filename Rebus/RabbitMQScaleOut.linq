<Query Kind="Program">
  <NuGetReference>Rebus.Autofac</NuGetReference>
  <NuGetReference>Rebus.RabbitMQ</NuGetReference>
  <Namespace>Autofac</Namespace>
  <Namespace>Autofac.Builder</Namespace>
  <Namespace>Rebus</Namespace>
  <Namespace>Rebus.Autofac</Namespace>
  <Namespace>Rebus.Bus</Namespace>
  <Namespace>Rebus.Configuration</Namespace>
  <Namespace>Rebus.Logging</Namespace>
  <Namespace>Rebus.RabbitMQ</Namespace>
  <Namespace>Rebus.Transports</Namespace>
  <Namespace>System.Timers</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

void Main()
{
  using (var adapter = new BuiltinContainerAdapter())
  {
      Configure.With(adapter)
          .Logging(l => l.ColoredConsole(LogLevel.Warn))
          .Transport(t => t.UseRabbitMqInOneWayMode("amqp://localhost").ManageSubscriptions())
          .CreateBus()
          .Start();

      var keepRunning = true;
      
      while (keepRunning)
      {
          Console.WriteLine(@"a) Publish 10 jobs
b) Publish 100 jobs
c) Publish 1000 jobs

q) Quit");
          var key = Char.ConvertFromUtf32(Console.Read()).ToLowerInvariant();

          switch (key)
          {
              case "a":
                  Publish(10, adapter.Bus);
                  break;
              case "b":
                  Publish(100, adapter.Bus);
                  break;
              case "c":
                  Publish(1000, adapter.Bus);
                  break;
              case "q":
                  Console.WriteLine("Quitting");
                  keepRunning = false;
                  break;
          }
      }
  }
}

// Methods
void Publish(int numberOfJobs, IBus bus)
{
  Console.WriteLine("Publishing {0} jobs", numberOfJobs);

  var jobs = Enumerable.Range(0, numberOfJobs)
      .Select(i => new Job { JobNumber = i });

  Parallel.ForEach(jobs, bus.Publish);
}

// Classes
class PrintDateTime : IHandleMessages<DateTime>
{
   public void Handle(DateTime currentDateTime)
   {
       Console.WriteLine("The time is {0}", currentDateTime);
   }
}

public class Job
{
   public int JobNumber { get; set; }
}        