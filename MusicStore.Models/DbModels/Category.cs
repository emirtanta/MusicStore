using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicStore.Models.DbModels
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage ="Kategori Adı Zorunludur")]
        [DisplayName("Kategori Adı")]
        [StringLength(250,MinimumLength =3,ErrorMessage ="Kategori adı en az 3 en fazla 250 karakter olmalıdır")]
        public string CategoryName { get; set; }
    }
}
