using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LivriaBackend.commerce.Application.Resources; 

namespace LivriaBackend.commerce.Interfaces.REST.Resources
{
    public record CreateOrderResource(
        [Required(ErrorMessage = "EmptyField")]
        [Range(0, int.MaxValue, ErrorMessage = "MinimumValueError")]
        int UserClientId,
        
        [Required(ErrorMessage = "EmptyField")]
        [EmailAddress(ErrorMessageResourceType = typeof(DataAnnotations), ErrorMessageResourceName = "EmailError")]
        string UserEmail,
        
        [Required(ErrorMessage = "EmptyField")]
        [Phone(ErrorMessageResourceType = typeof(DataAnnotations), ErrorMessageResourceName = "PhoneError")]
        string UserPhone,
        
        [Required(ErrorMessage = "EmptyField")]
        [StringLength(255, ErrorMessage = "MaxLengthError")]
        string UserFullName,
        
        [Required(ErrorMessage = "EmptyField")]
        [StringLength(255, ErrorMessage = "MaxLengthError")]
        string RecipientName,

        [Required(ErrorMessage = "EmptyField")]
        [StringLength(255, ErrorMessage = "MaxLengthError")] 
        string Status,

        
        [Required(ErrorMessage = "EmptyField")]
        bool IsDelivery,
        
        ShippingResource? ShippingDetails,

        [Required(ErrorMessage = "EmptyField")]
        [StringLength(20, ErrorMessage = "MaxLengthError")]
        string PaymentMethod = "external"
    );
    
}