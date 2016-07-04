using System.Xml;
using Sonic.Jms;
using Sonic.Jms.Ext;
using Connection = Sonic.Jms.Connection;
using Session = Sonic.Jms.Ext.Session;
using SessionMode = Sonic.Jms.SessionMode;

namespace Publisher
{
	public static class Publisher
	{
		private static string _broker;
		private static string _queue;
		private static string _user;
		private static string _password;

		private static Session requestSession;

		public static void Main(string[] args)
		{

		}

		public static bool Publish(Connection connection, string message)
		{
			const int timeout = 10000;
			requestSession = (Session)connection.createSession(false, SessionMode.AUTO_ACKNOWLEDGE);

			var destination = requestSession.createQueue(_queue);
			var messageProducer = requestSession.createProducer(destination);

			var xmlMessage = requestSession.createXMLMessage();
			xmlMessage.setDocument(CreateXmlDocument(message));

			messageProducer.send(xmlMessage, DefaultMessageProperties.DEFAULT_DELIVERY_MODE, DefaultMessageProperties.DEFAULT_PRIORITY, timeout);
			
			return true;
		}

		private static XmlDocument CreateXmlDocument(string message)
		{
			var x = new XmlDocument();
			x.LoadXml(message);
			return x;
		}
	}
}
