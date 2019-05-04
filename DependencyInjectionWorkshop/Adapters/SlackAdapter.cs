using SlackAPI;

namespace DependencyInjectionWorkshop.Adapters
{
    public class SlackAdapter
    {
        public void Notify(string message)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", "my message", "my bot name");
        }
    }
}