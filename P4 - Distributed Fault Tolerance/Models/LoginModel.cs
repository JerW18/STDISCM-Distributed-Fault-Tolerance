using System.ComponentModel.DataAnnotations;

namespace P4___Distributed_Fault_Tolerance.Models
{
    public class LoginModel
    {
        [Required] public required string Email { get; set; }
        [Required, DataType(DataType.Password)] public required string Password { get; set; }
    }
}
