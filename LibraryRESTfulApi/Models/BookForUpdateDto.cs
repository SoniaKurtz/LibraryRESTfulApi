using System.ComponentModel.DataAnnotations;

namespace LibraryRESTfulApi.Models
{
    public class BookForUpdateDto : BookForManipulationDto
    {
        [Required(ErrorMessage ="You should fill out a description.")]
        public override string Description { get => base.Description; set => base.Description = value; }
    }
}
