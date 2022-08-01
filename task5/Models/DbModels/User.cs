using System;
using System.ComponentModel.DataAnnotations;

namespace task5.Models.DbModels
{
    public class User
    {
        [Required]
        public string Id { get; set; }

        public DateTime Create { get; set; }
    }
}
