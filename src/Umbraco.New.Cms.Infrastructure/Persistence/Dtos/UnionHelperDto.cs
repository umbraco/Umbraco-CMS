using NPoco;

namespace Umbraco.New.Cms.Infrastructure.Persistence.Dtos;

internal class UnionHelperDto
{
    [Column("id")]
    public int Id { get; set; }

    [Column("otherId")]
    public int OtherId { get; set; }

    [Column("alias")]
    public string? Alias { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("isDependency")]
    public bool IsDependency { get; set; }

    [Column("dual")]
    public bool Dual { get; set; }
}
