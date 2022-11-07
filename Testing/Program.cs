// See https://aka.ms/new-console-template for more information
using System.Net;
using System.Net.Sockets;
using MessagePack;
using Testing;

Console.WriteLine("Hello, World!");

var DefaultHostname = IPAddress.Parse("127.0.0.1");
const int DefaultPort = 13002;
var ep = new IPEndPoint(DefaultHostname, DefaultPort);

Console.WriteLine("Listening");
var listener = new TcpListener(DefaultHostname, DefaultPort);
listener.Start();
var listenerTask = listener.AcceptTcpClientAsync();

Console.WriteLine("Connecting");
var c1 = new TcpClient();
c1.Connect(ep);

var c2 = await listenerTask;

// should be connected
Console.WriteLine(c1.Connected);
Console.WriteLine(c2.Connected);
Console.WriteLine();
//Console.WriteLine("Press enter to send messages");
//Console.ReadLine();

// messages
var m1 = new Msg1 { Blah = Guid.NewGuid(), X = 1.2f, Y = 3.4f };
var m2 = new Msg2 { Name = "Bob", Pass = "test" };
var m3 = new Msg1 { Blah = Guid.NewGuid(), X = 5.6f, Y = 7.8f };

var d1 = Serialise(m1);
var d2 = Serialise(m2);
var d3 = Serialise(m3);

var rcv = new MessageStreamReader(c2.GetStream());

//ar t = new Taskcv.Update);

var stream = c1.GetStream();
stream.Write(d1);
stream.Write(d2);
stream.Write(d3);
stream.Flush();

Thread.Sleep(200);

//await t;
while (rcv.msgs.Count < 1)
{
	rcv.Update();
}

foreach (var msg in rcv.msgs)
{
	Console.WriteLine($"{msg}");
}

//
Console.WriteLine("done");
Console.ReadLine();

byte[] Serialise<T>(T msg) where T : IMsg
{
	var smsg = MessagePackSerializer.Serialize(msg);

	//var shdr = new Header() { Length = smsg.Length };

	var a = BitConverter.GetBytes(smsg.Length);
	var b = BitConverter.GetBytes(msg.MessageType);

	var cbuf = new byte[8 + smsg.Length];
	Array.Copy(a, 0, cbuf, 0, 4);
	Array.Copy(b, 0, cbuf, 4, 4);
	Array.Copy(smsg, 0, cbuf, 8, smsg.Length);

	return cbuf;
}

public interface IMsg
{
	int MessageType { get; }
}

[MessagePackObject(keyAsPropertyName: true)]
public class Header
{
	public int Length { get; set; }

	public int MessageType { get; set; }

	[IgnoreMember]
	public const int Size = 4;
}

[MessagePackObject(keyAsPropertyName: true)]
public class Msg1 : IMsg
{
	[IgnoreMember]
	public int MessageType => 0;

	public Guid Blah { get; set; }

	public float X { get; set; }
	public float Y { get; set; }

	public override string ToString()
		=> $"[Msg1] Blah={Blah} X={X} Y={Y}";
}

[MessagePackObject(keyAsPropertyName: true)]
public class Msg2 : IMsg
{
	[IgnoreMember]
	public int MessageType => 1;

	public string Name { get; set; }

	public string Pass { get; set; }

	public override string ToString()
		=> $"[Msg2] Name={Name} Pass={Pass}";
}
