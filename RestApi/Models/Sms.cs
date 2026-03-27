using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestApi.Models
{
    public class Sms
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string SmsKey { get; set; }

        public string Salt { get; set; }
    }
}