using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace KPD.DAL.DbConnector
{
	internal class ConnectToMsSql : IConnectToDB
	{
		protected bool transactionIsActive;
		protected bool isOpened;
		protected SqlTransaction transaction;
		protected SqlConnection connection;

		internal SqlConnection Current
		{
			get { return connection; }
		}

		public bool IsOpened
		{
			get
			{
				return isOpened;
			}
		}

		public ConnectToMsSql()
		{
			transactionIsActive = false;
			isOpened = false;
		}

		public bool OpenConnection()
		{
			if (!(IsOpened))
			{
				connection = new SqlConnection(ConfigurationManager.ConnectionStrings["KPDConnectionString"].ConnectionString);
				connection.Open();
				isOpened = true;
			}
			else
			{
				Logger.Instance.WriteToLog("Warning: connection is already opened.");
			}
			return isOpened;
		}

		public bool OpenConnection(String newConnectionString)
		{
			if (!(isOpened))
			{
				connection = new SqlConnection(newConnectionString);
				connection.Open();
				isOpened = true;
			}
			else
			{
				Logger.Instance.WriteToLog("Warning: connection is already opened.");
			}
			return isOpened;
		}

		public void CloseConnection()
		{
			if (isOpened)
			{
				if (transactionIsActive)
				{
					RollbackTransaction();
				}
				connection.Close();
				isOpened = false;
			}
			else
			{
				Logger.Instance.WriteToLog("Error: there is no open connection."); ;
			}
		}

		public bool ExecNonQuery(DbCommand args)
		{
			if (isOpened)
			{
				if ((args != null) && (args.CommandText != String.Empty))
				{
					SqlCommand command = null;
					if (!transactionIsActive)
					{
						command = new SqlCommand(args.CommandText, connection);
					}
					else
					{
						command = new SqlCommand(args.CommandText, connection, transaction);
					}
					if (args.Parameters != null)
					{
						command.Parameters.AddRange(args.Parameters);
					}
					return (command.ExecuteNonQuery() > 0);
				}
				else
				{
					Logger.Instance.WriteToLog("Error in command arguments.");
				}
			}
			else
			{
				Logger.Instance.WriteToLog("Error: connection was not opened.");
			}
			return false;
		}

		public DataSet ExecSelect(DbCommand args)
		{
			if (isOpened)
			{
				if ((args != null) && (args.CommandText != String.Empty))
				{
					SqlCommand command = null;
					if (!transactionIsActive)
					{
						command = new SqlCommand(args.CommandText, connection);
					}
					else
					{
						command = new SqlCommand(args.CommandText, connection, transaction);
					}
					if (args.Parameters != null)
					{
						command.Parameters.AddRange(args.Parameters);
					}
					var dataAdapter = new SqlDataAdapter(command);
					var dataSet = new DataSet(connection.Database);
					dataAdapter.Fill(dataSet);
					return dataSet;
				}
				else
				{
					Logger.Instance.WriteToLog("Error in command arguments.");
				}
			}
			else
			{
				Logger.Instance.WriteToLog("Error: connection was not opened.");
			}
			return null;
		}

		public void BeginTransaction()
		{
			if (isOpened)
			{
				if (!transactionIsActive)
				{
					transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
					transactionIsActive = true;
				}
				else
				{
					Logger.Instance.WriteToLog("Error: transaction is already began.");
				}
			}
			else
			{
				Logger.Instance.WriteToLog("Error: connection was not opened.");
			}
		}

		public void CommitTransaction()
		{
			if (isOpened)
			{
				if (transactionIsActive)
				{
					transaction.Commit();
					transactionIsActive = false;
				}
				else
				{
					Logger.Instance.WriteToLog("Error: there is no active transaction.");
				}
			}
			else
			{
				Logger.Instance.WriteToLog("Error: connection was not opened.");
			}
		}

		public void RollbackTransaction()
		{
			if (isOpened)
			{
				if (transactionIsActive)
				{
					transaction.Rollback();
					transactionIsActive = false;
				}
				else
				{
					Logger.Instance.WriteToLog("Error: there is no active transaction.");
				}
			}
			else
			{
				Logger.Instance.WriteToLog("Error: connection was not opened.");
			}
		}
	}
}
