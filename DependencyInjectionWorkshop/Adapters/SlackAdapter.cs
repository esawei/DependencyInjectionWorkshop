using SlackAPI;

namespace DependencyInjectionWorkshop.Adapters
{
    public interface INotification
    {
        void Notify(string message);
    }

    public class SlackAdapter : INotification
    {
        public void Notify(string message)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", "my message", "my bot name");
        }
    }
}