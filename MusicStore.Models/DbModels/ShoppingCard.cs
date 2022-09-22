using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.Models.DbModels
{
    public class ShoppingCard
    {
        public ShoppingCard()
        {
            Count = 1; //Count değeri 0 olmasın diye tanımlandı

        }

        [Key]
        public int Id { get; set; }

        //kullanıcının id bilgisini tutar (ApplicationUser ile 1'çok ilişiki)
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }

        //ürün ile 1'e çok ilişki
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [Range(1,1000,ErrorMessage ="Lütfen 1 ile 1000 arasında bir değer giriniz")]
        public int Count { get; set; }
        [NotMapped]
        public double Price { get; set; }
    }
}
