namespace Retro.Web.Models
{
    public class ErrorResponse : ApiResponse
    {
        public ErrorResponse(string message)
        {
            Success = false;
            Message = message;
        }
    }
}