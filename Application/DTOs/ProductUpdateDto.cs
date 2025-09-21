using Microsoft.AspNetCore.Http;

namespace Application.DTOs
{
    public class ProductUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        public Guid? CategoryId { get; set; }
        public IFormFile? Image { get; set; }

    }
}
