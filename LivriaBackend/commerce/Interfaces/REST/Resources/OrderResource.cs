using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using LivriaBackend.users.Application.Resources;

namespace LivriaBackend.commerce.Interfaces.REST.Resources
{
    public record OrderResource(
        [Range(0, int.MaxValue, ErrorMessage = "MinimumValueError")]
        int Id,
        
        [StringLength(6, ErrorMessage = "MaxLengthError")]
        string Code,
        
        [Range(0, int.MaxValue, ErrorMessage = "MinimumValueError")]
        int UserClientId,
        
        [StringLength(255, ErrorMessage = "MaxLengthError")]
        string UserEmail,
        
        [Phone(ErrorMessageResourceType = typeof(DataAnnotations), ErrorMessageResourceName = "PhoneError")]
        string UserPhone,
        
        [StringLength(255, ErrorMessage = "MaxLengthError")]
        string UserFullName,
        
        [StringLength(255, ErrorMessage = "MaxLengthError")]
        string RecipientName,
        
        [StringLength(255, ErrorMessage = "MaxLengthError")]
        string Status,
        
        bool IsDelivery,
        
        string PaymentMethod,
        
        ShippingResource Shipping, 
        
        decimal Total,
        
        [DataType(DataType.DateTime)]
        [Range(typeof(DateTime), "1/1/1900", "12/12/3000", ErrorMessage = "DateOutOfRange")]
        DateTime Date, 
        
        IEnumerable<OrderItemResource> Items
    );
}