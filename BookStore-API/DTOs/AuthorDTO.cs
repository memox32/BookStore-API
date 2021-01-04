using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore_API.DTOs
{
  public class AuthorDTO
  {
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Bio { get; set; }

    public virtual IList<BookDTO> Books { get; set; }
  }

  public class AuthorCreateDTO
  {
    [Required]
    [DisplayName("First Name")]
    public string FirstName { get; set; }
    [Required]
    [DisplayName("Last Name")]
    public string LastName { get; set; }
    [Required]
    [DisplayName("Biography")]
    [StringLength(250)]
    public string Bio { get; set; }
  }

  public class AuthorUpdateDTO
  {
    [Required]
    public int Id { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    public string Bio { get; set; }
  }
}
