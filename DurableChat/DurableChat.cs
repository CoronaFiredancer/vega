/*
 * Copyright (c) 2001-2008 Progress Software Corporation -  All Rights Reserved
 * 
 * Sample Application
 * 
 * Writing a Basic JMS Application that uses:
 * - Publish and Subscribe
 * - Durable Subsciptions
 * - Persistent Messages
 * 
 * This sample publishes and subscribes to a specified topic.
 * Text you enter is published to the topic with the user name.
 * The message will persist for ten minutes if the subscriber is not available.
 * If the subscriber reconnects within that timem, the message is delivered.
 * 
 * Usage:
 * DurableChat -b <broker:port> -u <username> -p <password>
 * -b broker:port points to your message broker
 * Default: localhost:2506
 * -u username    must be unique (but is not checked)
 * -p password    password for user (not checked)
 * 
 * Suggested demonstration:
 * - In separate console windows, start instances of the application
 * under unique user names.For example:
 * DurableChat -b localhost:2506 -u ACCOUNTING
 * DurableChat -b localhost:2506 -u LEGAL
 * - Enter text and then press Enter to publish the message.
 * - See messages appear under the various user names as you
 * enter messages in each console window.
 * - Stop a session by pressing CTRL+C or CTRL-Z+<Enter> in 
 * its console window.
 * - Keep sending messages in other sessions.
 * - Restart the subscriber username session that was stopped.
 * - Note that the "missed" messages are still available if the restart is
 * within thirty minutes.
 */

using System;
using System.IO;
using System.Text;
using Sonic.Jms;

namespace DurableChat
{
	public sealed class DurableChat : MessageListener
	{
		private const string AppTopic = "Kamstrup_DEV_Carsten.Aras";
		//private const string AppTopic = "jms.samples.durablechat";
		//private const string PropertyName = "Department";
		private const string DefaultBrokerName = "SonicQueue";
		private const string DefaultPassword = "Administrator";
		private const string ClientId = "DurableChatter";
		private const long MessageLifespan = 1800000; //30 minutes

		private Connection _connection;
		private Session _pubSession;
		private Session _subSession;

		// Performs the chatter
		private void DurableChatter(string broker, string username, string password)
		{
			MessageProducer publisher = null;

			// Create a connection:
			try
			{
				username = "Administrator";
				ConnectionFactory factory = (new Sonic.Jms.Cf.Impl.ConnectionFactory(broker));
				_connection = factory.createConnection(username, password);
				// Durable Subscriptions are indexed by username, clientID and 
				// subscription name. It is a good proactice to set the clientID:
				_connection.setClientID(ClientId);
			}
			catch (JMSException jmse)
			{
				Console.Error.WriteLine("Failed to connect to broker - " + broker + " : " + jmse.Message);
				Exit();
			}

			// Create sessions, publisher and subscriber
			try
			{
				_pubSession = _connection.createSession(false, SessionMode.AUTO_ACKNOWLEDGE);
				var topic = _pubSession.createTopic(AppTopic);
				publisher = _pubSession.createProducer(topic);

				_subSession = _connection.createSession(false, SessionMode.AUTO_ACKNOWLEDGE);
				MessageConsumer subscriber = _subSession.createDurableSubscriber(topic, "SampleSubscription");
				subscriber.setMessageListener(this);
			}
			catch (JMSException jmse)
			{
				Console.Error.WriteLine("Failed to setup sessions, publisher and subscriber - " + jmse.Message);
				Exit();
			}

			// Start connection
			try
			{
				_connection.start();
			}
			catch (JMSException jmse)
			{
				Console.Error.WriteLine("Failed to start connection - " + jmse.Message);
				Exit();
			}

			// Process user input
			try
			{
				Console.Out.WriteLine("\nEnter text messages to clients that subscribe to the " + AppTopic + " topic." + "\nPress Enter to publish each message.\n");
				Stream stdin = Console.OpenStandardInput();
				StreamReader stdinReader = new StreamReader(stdin);
				while (true)
				{
					string s = stdinReader.ReadLine();

					if ((object)s == null)
					{
						Exit();
					}
					else if (s.Length > 0)
					{
						TextMessage msg = _pubSession.createTextMessage();
						msg.setText(username + ": " + s);
						// Publish the message persistantly:
						publisher?.send(msg,
							DeliveryMode.PERSISTENT,
							DefaultMessageProperties.DEFAULT_PRIORITY,
							MessageLifespan); //Time to Live
					}
				}
			}
			catch (IOException ioe)
			{
				Console.Error.WriteLine(ioe.Message);
				Exit();
			}
			catch (JMSException jmse)
			{
				Console.Error.WriteLine(jmse.Message);
				Exit();
			}
		}

		/// <summary>
		/// Message Handler*
		/// </summary>
		public void onMessage(Message aMessage)
		{
			try
			{
				// Cast the message as a text message.
				TextMessage textMessage = (TextMessage)aMessage;

				// This handler reads a single String from the
				// message and prints it to the standard output.
				string stringRenamed = textMessage.getText();
				Console.Out.WriteLine(stringRenamed);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine("Failed to parse received message - " + e.Message);
			}
		}



		//
		// NOTE: the remainder of this sample deals with reading arguments
		// and does not utilize any JMS classes or code.
		//

		// Cleanup resources and then exit. 
		private void Exit()
		{
			try
			{
				_connection?.close();
			}
			catch (Exception e)
			{
				Console.Error.WriteLine("Failure in closing connection - " + e.Message);
			}

			Environment.Exit(0);
		}

		/// <summary>
		/// Main program entry point. 
		/// </summary>
		[STAThread]
		public static void Main(string[] argv)
		{
			// Is there anything to do?
			if (argv.Length == 0)
			{
				PrintUsage();
				Environment.Exit(1);
			}

			// Values to be read from parameters
			string broker = DefaultBrokerName;
			string username = null;
			string password = DefaultPassword;

			// Check parameters
			for (int i = 0; i < argv.Length; i++)
			{
				string arg = argv[i];

				if (arg.Equals("-b"))
				{
					if (i == argv.Length - 1 || argv[i + 1].StartsWith("-"))
					{
						Console.Error.WriteLine("error: missing broker name:port");
						Environment.Exit(1);
					}
					broker = argv[++i];
					continue;
				}

				if (arg.Equals("-u"))
				{
					if (i == argv.Length - 1 || argv[i + 1].StartsWith("-"))
					{
						Console.Error.WriteLine("error: missing user name");
						Environment.Exit(1);
					}
					username = argv[++i];
					continue;
				}

				if (arg.Equals("-p"))
				{
					if (i == argv.Length - 1 || argv[i + 1].StartsWith("-"))
					{
						Console.Error.WriteLine("error: missing password");
						Environment.Exit(1);
					}
					password = argv[++i];
					continue;
				}

				if (arg.Equals("-h"))
				{
					PrintUsage();
					Environment.Exit(1);
				}

				// Invalid argument
				Console.Error.WriteLine("error: unexpected argument: " + arg);
				PrintUsage();
				Environment.Exit(1);
			}

			// Check values read in.
			if ((object)username == null)
			{
				Console.Error.WriteLine("error: user name must be supplied");
				PrintUsage();
			}

			// Start the JMS client for the "chat".
			DurableChat durableChat = new DurableChat();
			durableChat.DurableChatter(broker, username, password);
		}

		/// <summary>
		/// Prints the usage. 
		/// </summary>
		private static void PrintUsage()
		{
			StringBuilder use = new StringBuilder();
			use.Append("usage: DurableChat (options) ...\n\n");
			use.Append("options:\n");
			use.Append("  -b name:port Specify name:port of broker.\n");
			use.Append("               Default broker: " + DefaultBrokerName + "\n");
			use.Append("  -u name      Specify unique user name. (Required)\n");
			use.Append("  -p password  Specify password for user.\n");
			use.Append("               Default password: " + DefaultPassword + "\n");
			use.Append("  -h           This help screen.\n");
			Console.Error.WriteLine(use);
		}
	}
}
