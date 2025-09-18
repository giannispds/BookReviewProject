using System.ComponentModel.DataAnnotations;

namespace BookReviews.Models
{
    public class BaseEntity
    {
        [Key]
        public Guid Id{ get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdated { get; set; }

        public void InitNew()
        {
            var now = DateTime.UtcNow;
            LastUpdated = now;
            Created = now;
        }

        public void WasUpdated() => LastUpdated = DateTime.UtcNow;

        public void ValidateId()
        {
            if (Id.Equals(Guid.Empty))
                throw new ArgumentException("Id is invalid");
        }
    }
}
