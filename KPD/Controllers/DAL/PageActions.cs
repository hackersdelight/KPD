using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Data.SqlClient;
using System.Data;
using KPD.DAL.DbConnector;
using KPD.Models;
using KPD.Constants;

namespace KPD.DAL
{
	internal class PageActions
	{
		private ConnectToMsSql connection;
		private static PageActions _instance = new PageActions();
		private static List<PageModel> contentCache = new List<PageModel>();
		private FooterModel ruFooterCache;
		private FooterModel enFooterCache;
		internal static PageActions Instance
		{
			get { return _instance; }
		}
		private PageActions()
		{
			connection = new ConnectToMsSql();
			ruFooterCache = null;
			enFooterCache = null;
		}

		private List<PageModel> GetAllPages()
		{
			if (contentCache.Count == 0)
			{
				try
				{
					connection.OpenConnection();
					DbCommand command = new DbCommand("select * from tb_Pages");
					DataSet set = connection.ExecSelect(command);
					foreach (DataRow row in set.Tables[0].Rows)
					{
						contentCache.Add(CreateContentCacheInstance(row));
					}
				}
				finally
				{
					connection.CloseConnection();
				}
			}
			return contentCache;
		}

		internal FooterModel GetFooterContent(Language lang)
		{
			if (lang == Language.russian)
			{
				if (ruFooterCache == null)
				{
					ruFooterCache = GetFooterInstance(lang);
				}
				return ruFooterCache;
			}
			else
			{
				if (enFooterCache == null)
				{
					enFooterCache = GetFooterInstance(lang);
				}
				return enFooterCache;
			}
		}

		internal PageModel GetPageByName(string name, Language lang)
		{
			List<PageModel> pages = GetAllPages();
			PageModel result = pages.First(x => (String.Equals(x.title, name, StringComparison.InvariantCultureIgnoreCase) && (x.language == lang)));
			result.footer = GetFooterContent(lang);
			return result;
		}

		private FooterModel GetFooterInstance(Language lang)
		{
			FooterModel fm = new FooterModel()
			{
				address = GetAddress(lang),
				contacts = GetContacts(lang),
				partners = GetIcons()
			};
			return fm;
		}

		private PageModel CreateContentCacheInstance(DataRow pageRow)
		{
			PageModel m = new PageModel()
			{
				id = Convert.ToInt32(pageRow["Id"]),
				title = pageRow["Name"].ToString(),
				content = pageRow["PageContent"].ToString(),
				language = (Language)Convert.ToInt32(pageRow["Language"])
			};
			m.footer = null;
			return m;
		}

		private string GetCooperation(Language lang)
		{
			string result = String.Empty;
			try
			{
				using (SqlCommand cmd = new SqlCommand("pr_SelectCooperation", connection.Current))
				{
					cmd.Parameters.AddWithValue("@type", (int)lang);
					cmd.CommandType = CommandType.StoredProcedure;
					connection.OpenConnection();
					using (SqlDataReader rdr = cmd.ExecuteReader())
					{
						while (rdr.Read())
						{
							result = Convert.ToString(rdr["CoopContent"]);
						}
						cmd.Cancel();
					}
				}
			}
			catch (Exception E)
			{
				Logger.Instance.WriteToLog(E.Message);
			}
			finally
			{
				connection.CloseConnection();
			}
			return result;
		}

		private string GetAddress(Language lang)
		{
			string result = String.Empty;
			using (SqlCommand cmd = new SqlCommand("pr_SelectAddress", connection.Current))
			{
				cmd.Parameters.AddWithValue("@id", (int)lang);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Connection.Open();
				try
				{
					using (SqlDataReader rdr = cmd.ExecuteReader())
					{
						while (rdr.Read())
						{
							if (lang == Language.english)
								result = String.Format("{0}, {1},<br/> {2},<br/>{3};<br/> Postal code:{4}", Convert.ToString(rdr["country"]), Convert.ToString(rdr["city"]), Convert.ToString(rdr["addr"]), Convert.ToString(rdr["office"]), Convert.ToString(rdr["postalCode"]));
							else
								result = String.Format("{0}, {1},<br/> {2},<br/>{3};<br/> Индекс: {4}", Convert.ToString(rdr["country"]), Convert.ToString(rdr["city"]), Convert.ToString(rdr["addr"]), Convert.ToString(rdr["office"]), Convert.ToString(rdr["postalCode"]));
						}
						cmd.Cancel();
					}
				}
				catch (Exception E)
				{
					Logger.Instance.WriteToLog(E.Message);
				}
				finally
				{
					cmd.Connection.Close();
				}
			}
			return result;
		}

		private string GetContacts(Language lang)
		{
			string CityPhone = GetPhone(lang);
			string MobilePhone = GetCellPhone(lang);
			string Fax = GetFax(lang);
			string Email = GetEmail();
			return String.Format("{0}{1}{2}{3}", CityPhone, MobilePhone, Fax, Email);
		}

		private string GetPhone(Language lang)
		{
			string result = String.Empty;
			using (SqlCommand cmd = new SqlCommand("pr_SelectPhone", connection.Current))
			{
				cmd.Parameters.AddWithValue("@type", (int)PhoneType.phone);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Connection.Open();
				try
				{
					using (SqlDataReader rdr = cmd.ExecuteReader()) // создает ридер, унаследованный от idisposable
					{
						while (rdr.Read())
						{
							if (lang == Language.english)
								result = String.Format(CultureInfo.InvariantCulture, "Phone: {0} {1}<br/>", Convert.ToString(rdr["code"]), Convert.ToString(rdr["number"]));
							else
								result = String.Format(CultureInfo.InvariantCulture, "Телефон: {0} {1}<br/>", Convert.ToString(rdr["code"]), Convert.ToString(rdr["number"]));
						}
						cmd.Cancel();
					}
				}
				catch (Exception E)
				{
					Logger.Instance.WriteToLog(E.Message);
				}
				finally
				{
					cmd.Connection.Close();
				}
				return result;
			}
		}

		private string GetCellPhone(Language lang)
		{
			string result = String.Empty;
			using (SqlCommand cmd = new SqlCommand("pr_SelectPhone", connection.Current))
			{
				cmd.Parameters.AddWithValue("@type", (int)PhoneType.mobile);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Connection.Open();
				try
				{
					using (SqlDataReader rdr = cmd.ExecuteReader()) // создает ридер, унаследованный от idisposable
					{
						while (rdr.Read())
						{
							if (lang == Language.russian)
								result = String.Format("Моб.тел.: {0} {1}<br/>", Convert.ToString(rdr["code"]), Convert.ToString(rdr["number"]));
							else
								result = String.Format("Mob.ph.: {0} {1}<br/>", Convert.ToString(rdr["code"]), Convert.ToString(rdr["number"]));
						}
						cmd.Cancel();
					}
				}
				catch (Exception E)
				{
					Logger.Instance.WriteToLog(E.Message);
				}
				finally
				{
					cmd.Connection.Close();
				}
			}
			return result;
		}

		private string GetFax(Language lang)
		{
			string result = String.Empty;
			using (SqlCommand cmd = new SqlCommand("pr_SelectPhone", connection.Current))
			{
				cmd.Parameters.AddWithValue("@type", (int)PhoneType.fax);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Connection.Open();
				try
				{
					using (SqlDataReader rdr = cmd.ExecuteReader()) // создает ридер, унаследованный от idisposable
					{
						while (rdr.Read())
						{
							if (lang == Language.russian)
								result = String.Format("Факс: {0} {1}<br/>", Convert.ToString(rdr["code"]), Convert.ToString(rdr["number"]));
							else
								result = String.Format("Fax: {0} {1}<br/>", Convert.ToString(rdr["code"]), Convert.ToString(rdr["number"]));
						}
						cmd.Cancel(); //ускоряет очистку ридера из памяти
					}
				}
				catch (Exception E)
				{
					Logger.Instance.WriteToLog(E.Message);
				}
				finally
				{
					cmd.Connection.Close();
				}
			}
			return result;
		}

		private string GetEmail()
		{
			string result = String.Empty;
			using (SqlCommand cmd = new SqlCommand("pr_SelectEmail", connection.Current))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Connection.Open();
				try
				{
					using (SqlDataReader rdr = cmd.ExecuteReader())
					{
						while (rdr.Read())
						{
							result = String.Format("E-mail:{0}", Convert.ToString(rdr["email"]));
						}
						cmd.Cancel();
					}
				}
				catch (Exception E)
				{
					Logger.Instance.WriteToLog(E.Message);
				}
				finally
				{
					cmd.Connection.Close();
				}
			}
			return result;
		}

		private string GetIcons()
		{
			string result = String.Empty;
			using (SqlCommand cmd = new SqlCommand("pr_SelectFooterIcons", connection.Current))
			{
				cmd.Parameters.AddWithValue("@id", 1);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Connection.Open();
				try
				{
					using (SqlDataReader rdr = cmd.ExecuteReader())
					{
						while (rdr.Read())
						{
							result = Convert.ToString(rdr["FootCont"]);
						}
						cmd.Cancel();
					}
				}
				catch (Exception E)
				{
					Logger.Instance.WriteToLog(E.Message);
				}
				finally
				{
					cmd.Connection.Close();
				}
			}
			return result;
		}
	}

}