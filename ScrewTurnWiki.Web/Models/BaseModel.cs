using ScrewTurn.Wiki.Web.Code.InfoMessages;

namespace ScrewTurn.Wiki.Web.Models
{
    public class BaseModel
    {
        public BaseModel()
        {
            Message = new InfoMessage();
        }

        public InfoMessage Message { get; set; }
    }
}