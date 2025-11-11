using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;
using Microsoft.EntityFrameworkCore;

namespace nomad_gis_V2.Models;

[Table("UserClearedCells")]
[PrimaryKey(nameof(UserId), nameof(CellId))]
public class UserClearedCell
{
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public string CellId { get; set; } = string.Empty;

    [Column(TypeName = "geography(Polygon, 4326)")]
    public Polygon geom { get; set; } = null!;
}