namespace PhonoWriterWord.Database.Controllers
{
    public abstract class AbstractController
    {
        protected DatabaseController DatabaseController { get; }

        public AbstractController(DatabaseController controller)
        {
            DatabaseController = controller;
        }
    }
}
