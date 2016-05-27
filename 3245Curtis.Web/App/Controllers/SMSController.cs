using System.Net.Http;
using System.Web.Http;
using Twilio.Mvc;
using Twilio.TwiML;

namespace Logrythmik.App.Controllers
{
    [RoutePrefix("sms")]
    public class SMSController : TwillioAPIController
    {
        [Route("")]
        public HttpResponseMessage Post(SmsRequest request)
        {
            var response = new TwilioResponse();

            if (request.From == MyCell)
            {
                if (request.Body.ToLower().StartsWith("+1"))
                {
                    response.Sms(request.Body.Substring(12),
                        new
                        {
                            to = request.Body.Substring(0, 12),
                            from = ThisNumber
                        });
                }
                response.Reject("invalid command");
            }
            else
            {
                response.Sms(request.From + ": " + request.Body,
                    new
                    {
                        to = MyCell,
                        from = ThisNumber
                    });
            }

            return TWiML(response);
        }
    }
}