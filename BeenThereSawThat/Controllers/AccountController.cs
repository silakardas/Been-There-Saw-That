using Microsoft.AspNetCore.Mvc;
using BeenThereSawThat.Models;
using Microsoft.Data.SqlClient;
using System;
using System.IO;
using System.Collections.Generic;

namespace BeenThereSawThat.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public AccountController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        // --- KAYIT İŞLEMLERİ ---
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE KullaniciAdi = @KullaniciAdi OR Eposta = @Eposta";
                    SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                    checkCommand.Parameters.AddWithValue("@KullaniciAdi", model.KullaniciAdi);
                    checkCommand.Parameters.AddWithValue("@Eposta", model.Eposta);

                    connection.Open();
                    if ((int)checkCommand.ExecuteScalar() > 0)
                    {
                        ModelState.AddModelError("", "Bu kullanıcı adı veya e-posta zaten kullanılıyor.");
                        return View(model);
                    }

                    string insertQuery = @"INSERT INTO Users 
                        (Ad, Soyad, DogumGunu, DogumAyi, DogumYili, Cinsiyet, Eposta, KullaniciAdi, Sifre, KayitTarihi) 
                        VALUES (@Ad, @Soyad, @DogumGunu, @DogumAyi, @DogumYili, @Cinsiyet, @Eposta, @KullaniciAdi, @Sifre, @KayitTarihi)";

                    SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
                    insertCommand.Parameters.AddWithValue("@Ad", model.Ad);
                    insertCommand.Parameters.AddWithValue("@Soyad", model.Soyad);
                    insertCommand.Parameters.AddWithValue("@DogumGunu", model.Gun);
                    insertCommand.Parameters.AddWithValue("@DogumAyi", model.Ay);
                    insertCommand.Parameters.AddWithValue("@DogumYili", model.Yil);
                    insertCommand.Parameters.AddWithValue("@Cinsiyet", model.Cinsiyet ?? (object)DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@Eposta", model.Eposta);
                    insertCommand.Parameters.AddWithValue("@KullaniciAdi", model.KullaniciAdi);
                    insertCommand.Parameters.AddWithValue("@Sifre", model.Sifre);
                    insertCommand.Parameters.AddWithValue("@KayitTarihi", DateTime.Now);

                    insertCommand.ExecuteNonQuery();
                }
                TempData["SuccessMessage"] = "Kayıt başarılı! Giriş yapabilirsiniz.";
                return RedirectToAction("Login");
            }
            return View(model);
        }

        // --- GİRİŞ İŞLEMLERİ ---
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT Id, KullaniciAdi FROM Users WHERE KullaniciAdi = @KullaniciAdi AND Sifre = @Sifre";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@KullaniciAdi", model.KullaniciAdi);
                    command.Parameters.AddWithValue("@Sifre", model.Sifre);

                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        int userId = reader.GetInt32(0);
                        HttpContext.Session.SetInt32("UserId", userId);
                        HttpContext.Session.SetString("Username", reader.GetString(1));

                        if (model.BeniHatirla)
                        {
                            Response.Cookies.Append("UserId", userId.ToString(), new CookieOptions { Expires = DateTimeOffset.Now.AddDays(30) });
                        }
                        return RedirectToAction("Profile");
                    }
                    ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
                }
            }
            return View(model);
        }

        // --- PROFİL VE LİSTELEME (ÖDEV İÇİN GÜNCELLENEN KISIM) ---
        [HttpGet]
        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            ProfileViewModel model = new ProfileViewModel();
            model.RecentMovies = new List<MovieViewModel>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // 1. Kullanıcı Bilgilerini Getir
                string userQuery = "SELECT KullaniciAdi, Ad, Soyad, ProfilePictureUrl, BannerUrl, Bio, FavoriteMovie FROM Users WHERE Id = @UserId";
                SqlCommand userCommand = new SqlCommand(userQuery, connection);
                userCommand.Parameters.AddWithValue("@UserId", userId);

                connection.Open();
                using (SqlDataReader userReader = userCommand.ExecuteReader())
                {
                    if (userReader.Read())
                    {
                        model.Username = userReader.GetString(0);
                        model.Ad = userReader.GetString(1);
                        model.Soyad = userReader.GetString(2);
                        model.ProfilePictureUrl = userReader.IsDBNull(3) ? null : userReader.GetString(3);
                        model.BannerUrl = userReader.IsDBNull(4) ? null : userReader.GetString(4);
                        model.Bio = userReader.IsDBNull(5) ? null : userReader.GetString(5);
                        model.FavoriteMovie = userReader.IsDBNull(6) ? null : userReader.GetString(6);
                    }
                }

                // 2. Filmleri Tüm Detaylarıyla Listele (Önemli: Poster, Rating, Yönetmen ve Not dahil)
                string moviesQuery = @"SELECT Id, Title, Date, Rating, Genre, Director, Note, PosterPath 
                                     FROM Movies WHERE UserId = @UserId ORDER BY Date DESC";
                SqlCommand moviesCommand = new SqlCommand(moviesQuery, connection);
                moviesCommand.Parameters.AddWithValue("@UserId", userId);

                using (SqlDataReader moviesReader = moviesCommand.ExecuteReader())
                {
                    while (moviesReader.Read())
                    {
                        model.RecentMovies.Add(new MovieViewModel
                        {
                            MovieId = moviesReader.GetInt32(0),
                            Title = moviesReader.GetString(1),
                            Date = moviesReader.GetDateTime(2),
                            Rating = moviesReader.IsDBNull(3) ? "0" : moviesReader.GetValue(3).ToString(),
                            Genre = moviesReader.IsDBNull(4) ? "" : moviesReader.GetString(4),
                            Director = moviesReader.IsDBNull(5) ? "Bilinmiyor" : moviesReader.GetString(5),
                            Note = moviesReader.IsDBNull(6) ? "" : moviesReader.GetString(6),
                            PosterPath = moviesReader.IsDBNull(7) ? "/images/default-movie.jpg" : moviesReader.GetString(7)
                        });
                    }
                }
            }
            return View(model);
        }

        // --- PROFİL DÜZENLEME ---
        [HttpGet]
        public IActionResult EditProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            EditProfileViewModel model = new EditProfileViewModel();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT KullaniciAdi, Ad, Soyad, Bio, FavoriteMovie, ProfilePictureUrl, BannerUrl FROM Users WHERE Id = @UserId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    model.Username = reader.GetString(0);
                    model.Ad = reader.GetString(1);
                    model.Soyad = reader.GetString(2);
                    model.Bio = reader.IsDBNull(3) ? null : reader.GetString(3);
                    model.FavoriteMovie = reader.IsDBNull(4) ? null : reader.GetString(4);
                    model.ProfilePictureUrl = reader.IsDBNull(5) ? null : reader.GetString(5);
                    model.BannerUrl = reader.IsDBNull(6) ? null : reader.GetString(6);
                }
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult EditProfile(EditProfileViewModel model, IFormFile ProfilePictureFile, IFormFile BannerFile)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            string profilePicturePath = model.ProfilePictureUrl;
            string bannerPath = model.BannerUrl;

            // Dosya yükleme (Profil & Banner)
            if (ProfilePictureFile != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ProfilePictureFile.FileName);
                var filePath = Path.Combine(_env.WebRootPath, "uploads", "profiles", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                using (var stream = new FileStream(filePath, FileMode.Create)) { ProfilePictureFile.CopyTo(stream); }
                profilePicturePath = "/uploads/profiles/" + fileName;
            }

            if (BannerFile != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(BannerFile.FileName);
                var filePath = Path.Combine(_env.WebRootPath, "uploads", "banners", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                using (var stream = new FileStream(filePath, FileMode.Create)) { BannerFile.CopyTo(stream); }
                bannerPath = "/uploads/banners/" + fileName;
            }

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                string query = @"UPDATE Users SET KullaniciAdi = @Username, Ad = @Ad, Soyad = @Soyad, 
                                Bio = @Bio, FavoriteMovie = @FavoriteMovie, ProfilePictureUrl = @ProfilePictureUrl, BannerUrl = @BannerUrl 
                                WHERE Id = @UserId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", model.Username);
                command.Parameters.AddWithValue("@Ad", model.Ad);
                command.Parameters.AddWithValue("@Soyad", model.Soyad);
                command.Parameters.AddWithValue("@Bio", model.Bio ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@FavoriteMovie", model.FavoriteMovie ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ProfilePictureUrl", profilePicturePath ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@BannerUrl", bannerPath ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@UserId", userId);

                connection.Open();
                command.ExecuteNonQuery();
            }
            return RedirectToAction("Profile");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("UserId");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult DeleteAccount()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                
                SqlCommand deleteMovies = new SqlCommand("DELETE FROM Movies WHERE UserId = @UserId", connection);
                deleteMovies.Parameters.AddWithValue("@UserId", userId);
                deleteMovies.ExecuteNonQuery();

                SqlCommand deleteUser = new SqlCommand("DELETE FROM Users WHERE Id = @UserId", connection);
                deleteUser.Parameters.AddWithValue("@UserId", userId);
                deleteUser.ExecuteNonQuery();
            }
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}