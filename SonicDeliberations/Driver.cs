
using Sonic.Jms;

namespace SonicDeliberations
{
	public class Driver
	{
		private static string _broker;
		private static string _queue;
		private static string _user;
		private static string _password;

		private static Sonic.Jms.Ext.Session requestSession;

		public static void Main(string[] args)
		{
			var connection = Connect();


			if (connection != null)
			{
				Publisher.Publisher.Publish(connection, "hej hej"); 
			}

			Consumer.Consumer.Consume();
		}

		public static Connection Connect()
		{
			Connection connect;
			
			MessageProducer requestProducer = null;

			try
			{
				ConnectionFactory factory = (new Sonic.Jms.Cf.Impl.ConnectionFactory(_broker));
				connect = factory.createConnection(_user, _password);
				

				connect.start();

				return connect;
			}
			catch (JMSException)
			{
			}
			finally
			{
				
			}

			return null;
		}
	}
}
