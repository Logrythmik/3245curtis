using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Twilio.TwiML;

namespace Logrythmik.App.Controllers
{
    public class TwillioAPIController : ApiController
    {
        protected string ThisNumber;
        protected string MyCell;

        public TwillioAPIController()
        {
            ThisNumber = ConfigurationManager.AppSettings.Get("Seller:Phone");
            MyCell = ConfigurationManager.AppSettings.Get("App:Phone");
        }


        protected HttpResponseMessage TWiML(TwilioResponse response)
        {
            return Request.CreateResponse(HttpStatusCode.OK, response.Element, "text/xml");
        }
    }
}