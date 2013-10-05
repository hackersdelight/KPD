using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace KPD.DAL.DbConnector
{
	internal class RequestParameter
	{
		internal string ColumnName { get; set; }
		internal string ParameterName { get; set; }
		internal SqlParameter Parameter { get; set; }
	}
}
