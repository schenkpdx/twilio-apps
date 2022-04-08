using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

class Program
{
    static void Main(string[] args)
    {
        var accountSid = "";
        var authToken = "";
        var fromPhone = "+19717173981";
        var toPhone = "";

        TwilioClient.Init(accountSid, authToken);

        var message = MessageResource.Create(
            body: "This is a test message", 
            from: new PhoneNumber(fromPhone),
            to: new PhoneNumber(toPhone)
        );

        Console.WriteLine(message.Sid);
    }
}