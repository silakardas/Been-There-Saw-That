using BeenThereSawThat.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.IO;

namespace BeenThereSawThat.Controllers
{
    public class MovieController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public MovieController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        [HttpGet]
        public IActionResult AddMovie()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        public IActionResult AddMovie(MovieViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            ModelState.Remove("Poster");
            ModelState.Remove("PosterPath");

            if (!ModelState.IsValid)
                return View(model);

            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            string finalPosterPath = model.PosterPath;

            if (model.Poster != null && model.Poster.Length > 0)
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(model.Poster.FileName);
                string folder = Path.Combine(_env.WebRootPath, "uploads", "posters");
                Directory.CreateDirectory(folder);

                string path = Path.Combine(folder, fileName);
                using var stream = new FileStream(path, FileMode.Create);
                model.Poster.CopyTo(stream);

                finalPosterPath = "/uploads/posters/" + fileName;
            }

            using SqlConnection con = new SqlConnection(connectionString);
            con.Open();

            SqlCommand cmd;

            if (model.MovieId > 0)
            {
                cmd = new SqlCommand(@"
                    UPDATE Movies SET 
                        Title=@Title,
                        Date=@Date,
                        Rating=@Rating,
                        Genre=@Genre,
                        Director=@Director,
                        Note=@Note,
                        PosterPath=@PosterPath
                    WHERE Id=@Id AND UserId=@UserId", con);

                cmd.Parameters.AddWithValue("@Id", model.MovieId);
            }
            else
            {
                cmd = new SqlCommand(@"
                    INSERT INTO Movies 
                    (Title, Date, Rating, Genre, Director, Note, PosterPath, UserId, CreatedDate)
                    VALUES
                    (@Title, @Date, @Rating, @Genre, @Director, @Note, @PosterPath, @UserId, @CreatedDate)", con);

                cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
            }

            cmd.Parameters.AddWithValue("@Title", model.Title);
            cmd.Parameters.AddWithValue("@Date", model.Date);
            cmd.Parameters.AddWithValue("@Rating", model.Rating);
            cmd.Parameters.AddWithValue("@Genre", model.Genre);
            cmd.Parameters.AddWithValue("@Director", model.Director);
            cmd.Parameters.AddWithValue("@Note", model.Note ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@PosterPath", finalPosterPath ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@UserId", userId);

            cmd.ExecuteNonQuery();

            return RedirectToAction("Profile", "Account");
        }

        [HttpGet]
        public IActionResult EditMovie(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            MovieViewModel model = null;
            string cs = _configuration.GetConnectionString("DefaultConnection");

            using SqlConnection con = new SqlConnection(cs);
            using SqlCommand cmd = new SqlCommand(@"
                SELECT Id, Title, Date, Rating, Genre, Director, Note, PosterPath
                FROM Movies WHERE Id=@Id AND UserId=@UserId", con);

            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@UserId", userId);

            con.Open();
            using SqlDataReader r = cmd.ExecuteReader();

            if (r.Read())
            {
                model = new MovieViewModel
                {
                    MovieId = r.GetInt32(0),
                    Title = r.GetString(1),
                    Date = r.GetDateTime(2),
                    Rating = r.IsDBNull(3) ? null : r.GetString(3),
                    Genre = r.IsDBNull(4) ? null : r.GetString(4),
                    Director = r.IsDBNull(5) ? null : r.GetString(5),
                    Note = r.IsDBNull(6) ? null : r.GetString(6),
                    PosterPath = r.IsDBNull(7) ? null : r.GetString(7)
                };
            }

            if (model == null)
                return NotFound();

            return View("AddMovie", model);
        }

        [HttpPost]
        public IActionResult DeleteMovie(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            using SqlCommand cmd = new SqlCommand(
                "DELETE FROM Movies WHERE Id=@Id AND UserId=@UserId", con);

            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@UserId", userId);

            con.Open();
            cmd.ExecuteNonQuery();

            return RedirectToAction("Profile", "Account");
        }

        [HttpGet]
        public IActionResult MovieDetail(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            MovieViewModel model = null;
            string cs = _configuration.GetConnectionString("DefaultConnection");

            using SqlConnection con = new SqlConnection(cs);
            using SqlCommand cmd = new SqlCommand(@"
                SELECT Id, Title, Date, Rating, Genre, Director, Note, PosterPath
                FROM Movies WHERE Id=@Id AND UserId=@UserId", con);

            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@UserId", userId);

            con.Open();
            using SqlDataReader r = cmd.ExecuteReader();

            if (r.Read())
            {
                model = new MovieViewModel
                {
                    MovieId = r.GetInt32(0),
                    Title = r.GetString(1),
                    Date = r.GetDateTime(2),
                    Rating = r.IsDBNull(3) ? null : r.GetString(3),
                    Genre = r.IsDBNull(4) ? null : r.GetString(4),
                    Director = r.IsDBNull(5) ? null : r.GetString(5),
                    Note = r.IsDBNull(6) ? null : r.GetString(6),
                    PosterPath = r.IsDBNull(7) ? null : r.GetString(7)
                };
            }

            if (model == null)
                return NotFound();

            return View(model);
        }
    }
}