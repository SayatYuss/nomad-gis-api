using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace nomad_gis_V2.Migrations
{
    /// <inheritdoc />
    public partial class AddPostGisToMapPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Сначала добавляем новую колонку, но разрешаем ей быть NULL (nullable: true)
            migrationBuilder.AddColumn<Point>(
                name: "Location",
                table: "MapPoints",
                type: "geography(Point, 4326)",
                nullable: true); // <-- ВАЖНО: true

            // 2. Выполняем SQL-запрос, чтобы скопировать данные из (lon, lat) в (Location)
            //    PostGIS ST_MakePoint ожидает (Longitude, Latitude)
            migrationBuilder.Sql(
                @"UPDATE ""MapPoints"" 
                SET ""Location"" = ST_MakePoint(""Longitude"", ""Latitude"")::geography(Point, 4326)
                WHERE ""Longitude"" IS NOT NULL AND ""Latitude"" IS NOT NULL;");

            // 3. Теперь, когда у всех строк есть значение, меняем колонку на NOT NULL
            migrationBuilder.AlterColumn<Point>(
                name: "Location",
                table: "MapPoints",
                type: "geography(Point, 4326)",
                nullable: false); // <-- ВАЖНО: теперь false

            // 4. Только теперь, когда данные в безопасности, удаляем старые колонки
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "MapPoints");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "MapPoints");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Добавляем старые колонки, но разрешаем им быть NULL
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "MapPoints",
                type: "double precision",
                nullable: true); // <-- true

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "MapPoints",
                type: "double precision",
                nullable: true); // <-- true

            // 2. Копируем данные ОБРАТНО из Point в (lat, lon)
            //    (ST_Y - это Latitude, ST_X - это Longitude)
            migrationBuilder.Sql(
                @"UPDATE ""MapPoints"" 
          SET ""Latitude"" = ST_Y(""Location""::geometry), 
              ""Longitude"" = ST_X(""Location""::geometry)
          WHERE ""Location"" IS NOT NULL;");

            // 3. Делаем колонки NOT NULL (с значением по умолчанию на всякий случай)
            migrationBuilder.AlterColumn<double>(
               name: "Latitude",
               table: "MapPoints",
               type: "double precision",
               nullable: false,
               defaultValue: 0.0);

            migrationBuilder.AlterColumn<double>(
                name: "Longitude",
                table: "MapPoints",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            // 4. Удаляем колонку "Location"
            migrationBuilder.DropColumn(
                name: "Location",
                table: "MapPoints");
        }
    }
}
