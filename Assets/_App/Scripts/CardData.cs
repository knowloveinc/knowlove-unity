
namespace Knowlove
{
    [System.Serializable]
    public class CardData
    {
        public string text;
        public string parentheses;
        public int id;

        public bool isPrompt;
        public string promptMessage;
        public CardPromptButton[] promptButtons;

        public PathNodeAction action;
        public int rollCheck = 0;
        public ProceedAction rollPassed;
        public ProceedAction rollFailed;
    }
}