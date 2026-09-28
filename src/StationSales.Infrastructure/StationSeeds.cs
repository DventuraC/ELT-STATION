using StationSales.Domain;

namespace StationSales.Infrastructure;

internal static class StationSeeds
{
    private static readonly DateTime SeedDate = new(2026, 9, 27, 0, 0, 0, DateTimeKind.Utc);
    public static readonly Station[] All =
    {
        Create(1, "EDS_VENEZUELA", "EDS VENEZUELA", "20506151547"), Create(2, "EDS_ICA", "EDS ICA", "20506151547"), Create(3, "EDS_LA_MOLINA", "EDS LA MOLINA", "20506151547"), Create(4, "EDS_VICTORIA", "EDS VICTORIA", "20506151547"), Create(5, "EDS_VICTORIA_2", "EDS VICTORIA 2", "20506151547"), Create(6, "EDS_BARRANCO", "EDS BARRANCO", "20511706361"), Create(7, "EDS_MARINA", "EDS MARINA", "20506151547"), Create(8, "EDS_NUEVO_CHIMBOTE", "EDS NUEVO CHIMBOTE", "20506151547"), Create(9, "EDS_ARGENTINA", "EDS ARGENTINA", "20506151547"), Create(10, "EDS_CALLAO", "EDS CALLAO", "20506151547"), Create(11, "EDS_CHINCHA", "EDS CHINCHA", "20506151547"), Create(12, "EDS_PUENTE_PIEDRA", "EDS PUENTE PIEDRA", "20506151547"), Create(13, "EDS_LURIN", "EDS LURIN", "20506151547"), Create(14, "EDS_ATE", "EDS ATE", "20506151547"), Create(15, "EDS_CHIMBOTE_2", "EDS CHIMBOTE 2", "20506151547"), Create(16, "EDS_TRUJILLO", "EDS TRUJILLO", "20506151547"), Create(17, "EDS_TOMAS_VALLE", "EDS TOMAS VALLE", "20516752310"), Create(18, "EDS_CAMPOY", "EDS CAMPOY", "20516752310"), Create(19, "EDS_VENEZUELA_2", "EDS VENEZUELA 2", "20506151547"), Create(20, "EDS_PAITA", "EDS PAITA", "20506151547")
    };
    private static Station Create(long id, string code, string name, string ruc) => new() { Id = id, Code = code, Name = name, BusinessRuc = ruc, IsActive = true, CreatedAtUtc = SeedDate, UpdatedAtUtc = SeedDate };
}
