using SlackAPI;

namespace DependencyInjectionWorkshop.Adapters
{
    public class SlackAdapter
    {
        public void NofifyUser(string accountId)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel",
                $"{accountId} login invalid.", "my bot name");
        }
    }
}