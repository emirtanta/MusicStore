using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.Models.DbModels
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Adı")]
        public string Title { get; set; }

        [DisplayName("Açıklama")]
        public string Description { get; set; }
        
        //barkod numarası
        [Required]
        [Range(1,10000)]
        [DisplayName("Barkod Numarası")]
        public string ISBN { get; set; }
        
        //sahibi
        public string Author { get; set; }
        [Range(1, 10000)]
        [DisplayName("Liste Fiyatı")]
        public double ListPrice { get; set; }

        //satış fiyatı
        [Required]
        [Range(1, 10000)]
        [DisplayName("Ürün Fiyatı")]
        public double Price { get; set; }

        //50 ürün fiyatı
        [Required]
        [Range(1, 10000)]
        [DisplayName("50 Ürün Fiyatı")]
        public double Price50 { get; set; }

        //100 ürün alıcaksa fiyat
        [Required]
        [Range(1, 10000)]
        [DisplayName("100 Ürün Fiyatı")]
        public double Price100 { get; set; }
        public string ImageUrl { get; set; }

        //kategori ile ürün arasındaki 1'e çok ilişki
        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        //kapak adı ve ürün arasındaki 1'e çok ilişki
        [Required]
        public int CoverTypeId { get; set; }

        [ForeignKey("CoverTypeId")]
        public CoverType CoverType { get; set; }
    }
}
