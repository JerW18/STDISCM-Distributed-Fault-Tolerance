using System.ComponentModel.DataAnnotations;

namespace P4___Distributed_Fault_Tolerance.Models
{
    public class RegisterModel
    {
        [Required] public required string IdNumber { get; set; }
        [Required] public required string FirstName { get; set; }
        [Required] public required string LastName { get; set; }
        [Required] public required string Email { get; set; }
        [Required, DataType(DataType.Password)] public required string Password { get; set; }
        [Required, DataType(DataType.Password)] public required string ConfirmPassword { get; set; }
        [Required] public required string Role { get; set; }
    }
}
