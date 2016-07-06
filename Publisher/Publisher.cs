using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Sonic.Jms;
using Connection = Sonic.Jms.Connection;
using Session = Sonic.Jms.Ext.Session;
using SessionMode = Sonic.Jms.SessionMode;

namespace Publisher
{
	public static class Publisher
	{
		private static string _queue;
		private static Session _session;

		public static void Main(string[] args)
		{

		}

		public static bool Publish(Connection connection, string message, string queueName)
		{
			_queue = queueName;

			const int timeout = 100000;
			_session = (Session)connection.createSession(false, SessionMode.AUTO_ACKNOWLEDGE);

			var destination = _session.createQueue(_queue);
			var messageProducer = _session.createProducer(destination);

			var xmlMessage = _session.createXMLMessage();
			xmlMessage.setDocument(CreateXmlDocument(message));

			messageProducer.send(xmlMessage, DefaultMessageProperties.DEFAULT_DELIVERY_MODE, DefaultMessageProperties.DEFAULT_PRIORITY, timeout);
			
			return true;
		}

		private static XmlDocument CreateXmlDocument(string message)
		{
			var x = new XmlDocument();
			var serialized = message.Serialize();
			x.LoadXml(serialized);
			return x;
		}

		private static string Serialize<T>(this T value)
		{
			var xmlserializer = new XmlSerializer(typeof(T));
			var stringWriter = new Utf8StringWriter();

			using (var writer = XmlWriter.Create(stringWriter))
			{
				xmlserializer.Serialize(writer, value);
				return stringWriter.ToString();
			}
		}
	}

	public class Utf8StringWriter : StringWriter
	{
		public override Encoding Encoding => Encoding.UTF8;
	}
}
