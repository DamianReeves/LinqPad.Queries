<Query Kind="Program">
  <NuGetReference>NetMQ</NuGetReference>
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>NetMQ</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

static void Main(string[] args)
{
	using (NetMQContext context = NetMQContext.Create())
	{
		Task serverTask = Task.Factory.StartNew(() => Server(context));
		Task clientTask = Task.Factory.StartNew(() => Client(context));
		Task.WaitAll(serverTask, clientTask);
	}
}
 
  static void Server(NetMQContext context)
  {
    using (NetMQSocket serverSocket = context.CreateResponseSocket())
    {
      serverSocket.Bind("tcp://*:5555");
 
      while (true)
      {
        string message = serverSocket.ReceiveString();
 
        Console.WriteLine("Receive message {0}", message);
 
        serverSocket.Send(string.Format("Hello {0}", message));          
 
        if (message == "exit")
        {
          break;
        }
      }
    }      
  }
 
  static void Client(NetMQContext context)
  {
	using (var socket = context.CreateDealerSocket())
	{
	socket.Connect("tcp://127.0.0.1:5556");
	
	SecureChannel secureChannel = new SecureChannel(ConnectionEnd.Client);
	
	// we are not using signed certificate so we need to validate the certificate of the server
	// by default the secure channel is checking that the source of the certitiface is root certificate authority
	secureChannel.SetVerifyCertificate(c =&gt;  true);
	
	IList outgoingMessages = new List();
	
	// call the process message with null as the incoming message 
	// because the client is initiating the connection
	secureChannel.ProcessMessage(null, outgoingMessages);
	
	// the process message method fill the outgoing messages list with 
	// messages to send over the socket
	foreach (NetMQMessage outgoingMessage in outgoingMessages)
	{
		socket.SendMessage(outgoingMessage);
	}
	outgoingMessages.Clear();
	
	// waiting for a message from the server
	NetMQMessage incomingMessage= socket.ReceiveMessage();
	
	// calling ProcessMessage until ProcessMessage return true and the SecureChannel is ready
	// to encrypt and decrypt messages
	while (!secureChannel.ProcessMessage(incomingMessage, outgoingMessages))
	{
		foreach (NetMQMessage outgoingMessage in outgoingMessages)
		{
		socket.SendMessage(outgoingMessage);
		}
		outgoingMessages.Clear();
	
		incomingMessage = socket.ReceiveMessage();  
	}
	
	foreach (NetMQMessage outgoingMessage in outgoingMessages)
	{
		socket.SendMessage(outgoingMessage);
	}
	outgoingMessages.Clear();
	
	// you can now use the secure channel to encrypt messages
	NetMQMessage plainMessage = new NetMQMessage();
	plainMessage.Append("Hello");
	
	// encrypting the message and sending it over the socket
	socket.SendMessage(secureChannel.EncryptApplicationMessage(plainMessage));
	}
  }