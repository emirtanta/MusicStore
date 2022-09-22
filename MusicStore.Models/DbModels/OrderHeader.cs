using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.Models.DbModels
{
    public class OrderHeader
    {
        [Key]
        public int Id { get; set; }

        //siparişlerin kime ait olduğu bilgisi
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        //nakliye tarihi
        [Required]
        public DateTime ShippingDate { get; set; }

        [Required]
        public double OrderTotal { get; set; }

        //ürün numarası
        public string TrackingNumber { get; set; }

        public string Carrier { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime PaymentDate { get; set; }

        //son ödeme tarihi
        public DateTime PaymentDueDate { get; set; }
        public string TransactionId { get; set; }

        [Required]
        public string PhoneNumber { get; set; }
        
        [Required]
        public string StreetAddress { get; set; }
        
        [Required]
        public string City { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public string PostCode { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
