using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LivriaBackend.commerce.Interfaces.REST.Resources;
using LivriaBackend.users.Application.Resources;

namespace LivriaBackend.users.Interfaces.REST.Resources
{
    
    public record UserClientResource(
        int Id,
        
        [StringLength(100, MinimumLength = 3, ErrorMessage = "LengthError")]
        string Display,
        
        [StringLength(50, MinimumLength = 3, ErrorMessage = "LengthError")]
        string Username,
        
        [StringLength(100, ErrorMessage = "MaxLengthError")]
        [EmailAddress(ErrorMessageResourceType = typeof(DataAnnotations), ErrorMessageResourceName = "EmailError")]
        string Email,
        
        string Icon,
        
        string Phrase,
        
        string Subscription,
        
        DateTime? PlanChangeDate,
        
        bool HasPayed,
        
        decimal Wallet
        
    ) : UserResource(Id, Display, Username, Email); 
}