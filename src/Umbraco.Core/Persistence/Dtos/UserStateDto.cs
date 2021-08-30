using NPoco;

namespace Umbraco.Core.Persistence.Dtos
{
    public class UserStateDto
    {
        [Column("num")]
        public int Count { get; set; }
    }
}
