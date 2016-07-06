using Sonic.Jms;
using Connection = Sonic.Jms.Connection;
using Session = Sonic.Jms.Ext.Session;

namespace Consumer
{
	public class Consumer
	{
		private static Session _session;

		public static void Main(string[] args)
		{

		}

		public static Message Consume(Connection connection, string queueName)
		{
			_session = (Session)connection.createSession(false, SessionMode.AUTO_ACKNOWLEDGE);
			var receiveQueue = _session.createQueue(queueName);
			var consumer = _session.createConsumer(receiveQueue);

			var response = consumer.receive();

			return response;
		}
	}
}
