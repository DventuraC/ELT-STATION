# ELT de ventas de estaciones

Solución .NET 6 con DDD ligero para cargar ventas de OpenComb y Gasolution en RAW SQL Server, transformarlas a CORE y publicar destinos MART independientes.

La infraestructura, el control de ejecuciones, las capas `etl`, `raw`, `core` y las abstracciones están implementadas. No se han inventado columnas, consultas ni hashes funcionales de proveedores: deben completarse en los extractores y DTOs cuando se entreguen los contratos de OpenComb y Gasolution.

Consulte [docs/quick-start.md](docs/quick-start.md) para configurar secretos, aplicar migraciones, ejecutar el seed y operar las extracciones. Los comandos incluyen `seed`, `extract`, `extract-opencomb`, `extract-gasolution`, `backfill-gasolution`, `transform`, `publish` y `run-all`.

La migración inicial está en `src/StationSales.Infrastructure/Migrations`. Para crear la base central: `dotnet tool restore` y `dotnet tool run dotnet-ef database update --project src/StationSales.Infrastructure --startup-project src/StationSales.Worker`.

Las credenciales temporales de cada fuente se resuelven desde `DataSourceSecrets:{SecretReference}` en `appsettings.Production.json`, archivo que se excluye de Git. La base central conserva únicamente `SecretReference`.
