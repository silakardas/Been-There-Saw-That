using System.Collections.Generic;

namespace BeenThereSawThat.Models
{
    public class ProfileViewModel
    {
        public string Username { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }

        public string ProfilePictureUrl { get; set; }
        public string BannerUrl { get; set; }

        public string Bio { get; set; }
        public string FavoriteMovie { get; set; }

       
        public List<MovieViewModel> RecentMovies { get; set; }
    }
}
