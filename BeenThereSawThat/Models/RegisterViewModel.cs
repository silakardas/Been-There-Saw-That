using System.ComponentModel.DataAnnotations;

namespace BeenThereSawThat.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        public string Ad { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        public string Soyad { get; set; }

        
        public int Gun { get; set; }
        public string Ay { get; set; }
        public int Yil { get; set; }

        [Required(ErrorMessage = "Cinsiyet seçimi zorunludur.")]
        public string Cinsiyet { get; set; }

        [Required(ErrorMessage = "E-posta adresi gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçersiz e-posta formatı.")]
        public string Eposta { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı gereklidir.")]
        public string KullaniciAdi { get; set; }

        [Required(ErrorMessage = "Şifre gereklidir.")]
        [DataType(DataType.Password)]
        public string Sifre { get; set; }
    }
}