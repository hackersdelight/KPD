using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using KPD.Constants;

namespace KPD.Models
{
	public class PageModel
	{
		public int id { get; set; }
		public string title { get; set; }
		public string content { get; set; }
		public Language language { get; set; }
		public FooterModel footer { get; set; }
	}

	public class FooterModel
	{
		public string contacts { get; set; }
		public string partners { get; set; }
		public string address { get; set; }
	}
}