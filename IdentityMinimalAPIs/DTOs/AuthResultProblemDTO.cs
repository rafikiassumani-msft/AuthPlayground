using Microsoft.AspNetCore.Mvc;

namespace IdentityMinimalAPIs.DTOs
{
    public class AuthResultProblemDTO: ProblemDetails
    {
        public bool Succeeded { get; set; } 
        public string RequestId { get; set; }
        public DateTime? TimeStamp { get; set; }
    }
}
