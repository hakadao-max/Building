using Suburb;

namespace Test.Trigger
{
    public class MTAutoOpenCloseTrigger : MTTrigger
    {
        public SimpleOpenClose openCloseCom;

        protected override void OnEnter()
        {
            base.OnEnter();
            openCloseCom.Open();
        }

        protected override void OnExit()
        {
            base.OnExit();
            openCloseCom.Close();
        }
    }
}