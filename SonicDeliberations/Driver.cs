
using Sonic.Jms;

namespace SonicDeliberations
{
	public class Driver
	{
		private static string _broker;
		private static string _user;
		private static string _password;

		private static Sonic.Jms.Ext.Session requestSession;

		public static void Main(string[] args)
		{
			_broker = "localhost:2506";
			_user = "";
			_password = "";

			var connection = Connect();


			if (connection != null)
			{
				var published = Publisher.Publisher.Publish(connection, "Hej hej Pindsvin", "SampleQ2");

				if (published)
				{
					var reply = Consumer.Consumer.Consume(connection, "SampleQ2");
				}
			}

			
		}

		public static Connection Connect()
		{
			Connection connect;

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
