using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StrategyGame.Requests
{
    public class RegisterRequest
    {
        [Required]
        public string UserName { get; set; } // Name of the user
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }

}