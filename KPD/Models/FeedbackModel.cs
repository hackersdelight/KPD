using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KPD.Models
{
	public class FeedbackModel
	{
		public string Name { get; set; }
		public string Email { get; set; }
		public string Message { get; set; }
		public FooterModel footer { get; set; }
	}
}