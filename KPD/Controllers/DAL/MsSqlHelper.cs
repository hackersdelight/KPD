using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Data.SqlClient;
using System.Data;

namespace KPD.DAL.DbConnector
{
	internal class MsSqlHelper
	{
		private SqlConnectionStringBuilder connstringBuilder;
		private readonly string backupPath;
		private const string bakExt = ".bak";

		internal MsSqlHelper(string connectionString)
		{
			this.connstringBuilder = new SqlConnectionStringBuilder(connectionString);
			backupPath = GetParameterFromAppConfig("dbBackupPath");
		}

		internal MsSqlHelper(string connectionString, string backupPath)
		{
			this.connstringBuilder = new SqlConnectionStringBuilder(connectionString);
			this.backupPath = backupPath;
		}

		internal void RestoreDatabase()
		{
			string dbName = connstringBuilder.InitialCatalog;

			Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "Restore of the \"{0}\" database from \"{1}\" backup file started...",
				dbName,
				backupPath));
			connstringBuilder.Pooling = false;

			using (var conn = new SqlConnection(connstringBuilder.ConnectionString))
			{
				conn.Open();

				string q1 = "ALTER DATABASE "+ dbName +
										" SET OFFLINE WITH ROLLBACK IMMEDIATE";
				using (SqlCommand cmd = new SqlCommand(q1, conn))
				{
					cmd.ExecuteNonQuery();
					Trace.WriteLine("Connection to db dropped, db offline");
				}

				string q2 = "ALTER DATABASE " + dbName +
										" SET ONLINE";
				using (SqlCommand cmd = new SqlCommand(q2, conn))
				{
					cmd.ExecuteNonQuery();
					Trace.WriteLine("Connection to db dropped, db online");
				}

				string qry = "RESTORE DATABASE @db_name " +
										"FROM DISK = @back_up_file_name WITH REPLACE";

				using (var cmd = new SqlCommand(qry, conn))
				{
					cmd.CommandTimeout = 60;
					cmd.Parameters.Add("@db_name", SqlDbType.VarChar, 255).Value = dbName;
					cmd.Parameters.Add("@back_up_file_name", SqlDbType.VarChar, 255).Value = backupPath+dbName;

					cmd.ExecuteNonQuery();
					Trace.WriteLine("Database successfully restored in " + dbName);
				}
			}
		}

		internal void BackupDatabase()
		{
			string dbName = connstringBuilder.InitialCatalog;
			Trace.WriteLine(String.Format(CultureInfo.InvariantCulture, "Backup of the \"{0}\" database started.", dbName));
			using (var conn = new SqlConnection(connstringBuilder.ConnectionString))
			{
				try
				{
					conn.Open();
					const string qry = "BACKUP DATABASE @db_name " +
									   "TO DISK = @back_up_file_name " +
									   "WITH INIT";
					using (var cmd = new SqlCommand(qry, conn))
					{
						cmd.Parameters.Add("@db_name", SqlDbType.VarChar, 255).Value = dbName;
						cmd.Parameters.Add("@back_up_file_name", SqlDbType.VarChar, 255).Value = backupPath+dbName;

						cmd.ExecuteNonQuery();
						Trace.WriteLine(String.Format(CultureInfo.InvariantCulture, "Backup of the \"{0}\" succesfully done.", dbName));
					}
				}
				catch (Exception e)
				{
					Trace.WriteLine(String.Format(CultureInfo.InvariantCulture,
						"Backup of the \"{0}\" database failed: \n{1}",
						dbName,
						e.Message));

					throw;
				}
			}
		}

		internal static string GetParameterFromAppConfig(string param)
		{
			string value = ConfigurationManager.AppSettings[param];
			if (String.IsNullOrEmpty(value))
			{
				throw new Exception("value " + param + " wasn't found in app.config!");
			}
			else
			{
				return value;
			}
		}
	}
}
