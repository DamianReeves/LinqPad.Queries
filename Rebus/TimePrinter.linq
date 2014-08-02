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
</Query>

void Main()
{
  using (var adapter = new BuiltinContainerAdapter())
  using (var timer = new System.Timers.Timer())
  {
      adapter.Register(() => new PrintDateTime());

      var bus = Configure.With(adapter)
                         .Logging(l => l.None())
                         .Transport(t => 
						 	t.UseRabbitMq("amqp://localhost","my-app.input","my-app.error")
							 .ManageSubscriptions())
                         .MessageOwnership(d => d.Use(new LinqPadMessageOwnership()))
                         .CreateBus()
                         .Start();

      timer.Elapsed += delegate { bus.Send(DateTime.Now); };
      timer.Interval = 1000;
      timer.Start();

      Console.WriteLine("Press enter to quit");
      Console.ReadLine();
  }
}

// Define other methods and classes here
class PrintDateTime : IHandleMessages<DateTime>
{
   public void Handle(DateTime currentDateTime)
   {
       Console.WriteLine("The time is {0}", currentDateTime);
   }
}

class LinqPadMessageOwnership : IDetermineMessageOwnership{
	public string GetEndpointFor(Type messageType){
		return "my-app.input";
	}
}
	
	