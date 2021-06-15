namespace Retro.Web.Models
{
    public class SuccessResponse : ApiResponse
    {
        public SuccessResponse(string message = "")
        {
            Success = true;
            Message = message;
        }
    }
}