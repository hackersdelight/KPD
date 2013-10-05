using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using KPD.Models;
using KPD.DAL;
using KPD.Constants;

namespace KPD.Controllers
{
	public class HomeController : BaseController
	{
		//
		// GET: /Home/
		private Language GetCurrentLanguage()
		{
			CultureInfo ci = (CultureInfo)Session["Culture"];
			if (ci == null)
				return Language.russian;
			if (ci.TwoLetterISOLanguageName == "en")
				return Language.english;
			else return Language.russian;
		}

		public ActionResult Index()
		{
			PageModel m = PageActions.Instance.GetPageByName(PageTitle.Main, GetCurrentLanguage());
			return View(m);
		}

		public ActionResult Solution()
		{
			PageModel m = PageActions.Instance.GetPageByName(PageTitle.Solution, GetCurrentLanguage());
			return View("Index", m);
		}

		public ActionResult Cooperation()
		{
			PageModel m = PageActions.Instance.GetPageByName(PageTitle.Main, GetCurrentLanguage());
			return View(m);
		}

		public ActionResult About()
		{
			PageModel m = PageActions.Instance.GetPageByName(PageTitle.About, GetCurrentLanguage());
			return View("Index", m);
		}

		public ActionResult Feedback()
		{
			FeedbackModel m = new FeedbackModel() { footer = PageActions.Instance.GetFooterContent(GetCurrentLanguage()) };
			return View(m);
		}

		public ActionResult Certification()
		{
			PageModel m = PageActions.Instance.GetPageByName(PageTitle.Certification, GetCurrentLanguage());
			return View("Index", m);
		}

		public ActionResult GosPromNadzor()
		{
			PageModel m = PageActions.Instance.GetPageByName(PageTitle.GosPromNadzor, GetCurrentLanguage());
			return View("Index", m);
		}

		public ActionResult Logistics()
		{
			PageModel m = PageActions.Instance.GetPageByName(PageTitle.Logistics, GetCurrentLanguage());
			return View("Index", m);
		}

		public ActionResult Translation()
		{
			PageModel m = PageActions.Instance.GetPageByName(PageTitle.Translation, GetCurrentLanguage());
			return View("Index", m);
		}
	}
}
