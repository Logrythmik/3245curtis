using System.Net.Mail;
using System.Web.Http;
using Logrythmik.App.Models;

namespace Logrythmik.App.Controllers
{
    [RoutePrefix("api/contact")]
    public class ContactController: ApiController
    {
        [Route(""), HttpPost]
        public IHttpActionResult Post(InquiryView inquiry)
        {
            var smtpCient = new SmtpClient();

            smtpCient.Send(new MailMessage(inquiry.Email, "info@324curtis.info",
                "New Email from: " + inquiry.Name + " at " + inquiry.Email,
                inquiry.Inquiry));

            return Ok();
        }
    }
}