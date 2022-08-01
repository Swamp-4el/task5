using System;
using System.ComponentModel.DataAnnotations;

namespace task5.Models.DbModels
{
    public class Message
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Data { get; set; }

        [Required]
        public DateTime Time { get; set; }

        public string SenderId { get; set; }

        public User Sender { get; set; }

        public string RecipientId { get; set; }

        public User Recipient { get; set; }
    }
}
