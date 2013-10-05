using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;

namespace KPD.Controllers
{
	public class BaseController : Controller
	{
		//
		// GET: /Base/
		public ActionResult SwitchLocale(string lang, string returnUrl)
		{
			Session["Culture"] = new CultureInfo(lang);
			return Redirect(returnUrl);
		}
	}
}
