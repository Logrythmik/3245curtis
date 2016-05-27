using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web.Http;
using Logrythmik.App.Models;
using Twilio.Mvc;
using Twilio.TwiML;

namespace Logrythmik.App.Controllers
{
    [RoutePrefix("ivr")]
    public class IVRController : TwillioAPIController
    {
        [Route("")]
        public IHttpActionResult Get()
        {
            return Ok("Server is healthy!");
        }
        
        [Route("start"), HttpPost]
        public HttpResponseMessage Start(VoiceRequest request)
        {
            var response = new TwilioResponse();

            // "Thank you for calling about the home at 1 3 4 West Ellsworth Avenue."
            response.Play("/audio/thank_you.mp3");

            response.Sms("Curtis call notification: From " + request.From,
                new
                {
                    to = MyCell,
                    from = ThisNumber
                });
            
            response.Redirect("menu");
           
            return TWiML(response);
        }

        [Route("menu"), HttpPost]
        public HttpResponseMessage Menu(VoiceRequest request)
        {
            var response = new TwilioResponse();

            response.BeginGather(new {action = "menu_selection"});
            
            // "For general information about this property, press 1. " +
            // "If you are an agent or a broker, and you would like to schedule a viewing, press 2. "+
            // "If you are not an agent and would like to schedule a viewing, press 3. " +
            // "To request more information about the home or to make an offer, press 4."
            // "For a quicker response, text your question to this number directly."
            response.Play("/audio/menu.mp3");

            response.EndGather();
            response.Pause();
            response.Redirect("menu");

            return TWiML(response);
        }

        [Route("menu_selection"), HttpPost]
        public HttpResponseMessage Menu_Selection(VoiceRequest request)
        {
            var response = new TwilioResponse();

            if (request.Digits == "4")
            {
                response.Redirect("/ivr/record/"+ CallType.Unknown);
                return TWiML(response);
            }

            var record = true;
            var type = CallType.Unknown;

            response.BeginGather(new { action = "record/"+ type });

            // "To skip these recordings and leave a message, press 1 at any time. "
            response.Play("/audio/skip.mp3");

            switch (request.Digits)
            {
                case ("1"):
                    {
                        // "The house sits on the corner of West Ellsworth and Bannock located in the heart of the historic Baker neighborhood. " +
                        // "There is over 1700 finished square feet in this classic contemporary town-home, built in the 1890s. " +
                        // "The house has been updated with modern finishes, like the walnut cabinets, granite counter-tops and stainless steel appliances. " +
                        // "There are two bedrooms upstairs and a non-conforming master bedroom in the basement that could also serve as a theater room, den or man-cave. " +
                        // "The kitchen has granite counter-tops and new stainless steel appliances. " +
                        // "This location is perfect for urban commuters. It's only a few blocks from the light-rail station, and the zero bus (which runs downtown every 15 minutes). " +
                        // "Located two blocks off Broadway, the house is a stone's throw from many great restaurants, boutiques, coffee shops and an ice-cream parlor. " +
                        // "Not to mention the thriving nightlife.  " +
                        // "There is beautiful hardwood floors, brand-new high-quality carpet, sky lights and two great travertine tiled bathrooms, " +
                        // "one with a full tub the other with a two-person shower. " +
                        // "The asking price is 375 thousand. "
                        response.Play("/audio/description.mp3");
                        
                        record = false;

                        break;
                    }
                case ("2"):
                    {
                        // "This house is for sale by owner. The owner will work with buyers agents. " + 
                        // "To gain access to the home, please leave a message with your full name, company name, mobile number, buyers name and the date and time you would like to view the home. " + 
                        // "When approved, you will receive a text with the lock-box access code. "
                        response.Play("/audio/agents.mp3");

                        type = CallType.Agent;

                        break;
                    }
                case ("3"):
                    {
                        // "To schedule a viewing please leave a message with your full name, drivers license number, cell-phone number and the date and time you would like to view the home. " +
                        // "When approved, you will receive a text with the lock-box access code. "+
                        // "To speak directly to the owner, press 2. "
                        response.Play("/audio/non-agent.mp3");

                        type = CallType.NonAgent;

                        break;
                    }

                case ("4"):
                    {
                        type = CallType.Request;

                        break;
                    }
            }
            
            response.EndGather();

            response.Redirect(record ? "/ivr/record/" + type : "menu");
            
            return TWiML(response);
        }


        [Route("record/{type}"), HttpPost]
        public HttpResponseMessage Record(VoiceRequest request, CallType type)
        {
            var response = new TwilioResponse();

            if (request.Digits == "2")
            {
                response.Dial(MyCell);
                return TWiML(response);
            }

            // "Please leave your message after the beep. Press pound when you are done recording. "
            response.Play("/audio/record.mp3");

            response.Record(new
            {
                playBeep = "true",
                action = "/ivr/record_callback/" + type
            });

            return TWiML(response);
        }

        [Route("record_callback/{type}"), HttpPost]
        public HttpResponseMessage Record_Callback(VoiceRequest request, CallType type)
        {
            var smtpCient = new SmtpClient();

            var message = new MailMessage("ellsworth@jasonwicker.com", "baker@jasonwicker.com",
                "New " + type + "  Voice-mail from: " + request.From,
                "You have a new " + type + " voice-mail for the Ellsworth house. \r\nClick here to listen: " +
                request.RecordingUrl);

            var webClient = new WebClient();

            using (var stream = webClient.OpenRead(request.RecordingUrl))
            {
                message.Attachments.Add(new Attachment(stream, "recording.mp3"));

                smtpCient.Send(message);
            }

            var response = new TwilioResponse();

            response.Sms("Ellsworth " + type + " voice-mail notification: From " + request.From,
               new
               {
                   to = MyCell,
                   from = ThisNumber
               });

            // "Thank you for calling. I will contact you as soon as possible. "
            response.Play("/audio/closing.mp3");

            response.Hangup();

            return TWiML(response);
        }
    }
}
