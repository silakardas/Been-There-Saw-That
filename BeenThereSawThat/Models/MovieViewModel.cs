using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace BeenThereSawThat.Models
{
    public class MovieViewModel
    {
        public int MovieId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public string? Rating { get; set; } // int değil, string? olmalı

        [Required]
        public string Genre { get; set; }

        [Required]
        public string Director { get; set; }

        public string? Note { get; set; }

        public string? PosterPath { get; set; }

        public IFormFile? Poster { get; set; }
    }
}