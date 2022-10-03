using Microsoft.AspNetCore.Mvc;

namespace IdentityMinimalAPIs.DTOs
{
    public class AuthResultDTO
    {
        public bool Succeeded { get; set; }
        public int StatusCode { get; set; }
        public string? Message { get; set; } 
        public IList<string>? Errors { get; set; }   
        public DateTime? TimeStamp { get; set; }
    }
}
