# Guía rápida de operación

## Antes de iniciar

1. Instale .NET SDK 6 y restaure las herramientas:

   ```powershell
   dotnet tool restore
   dotnet restore
   ```

2. Copie `src/StationSales.Worker/appsettings.Production.example.json` como `appsettings.Production.json` en la misma carpeta.
3. Reemplace todos los valores `replace-with-*` por secretos reales. El archivo de producción está ignorado por Git.
4. Cree o actualice la base central:

   ```powershell
   dotnet tool run dotnet-ef database update --project src/StationSales.Infrastructure --startup-project src/StationSales.Worker
   ```

5. Cargue la configuración operativa. El seed es idempotente, identifica registros por código y conserva los watermarks existentes:

   ```powershell
   dotnet run --project src/StationSales.Worker -- seed
   ```

El seed configura 18 fuentes OpenComb y 15 fuentes Gasolution, sus estaciones, referencias de secreto, estado activo, fecha inicial, recuperación de 5 días e intervalo de 180 minutos.

## Comandos del worker

Ejecute todos los comandos con el ambiente de producción cuando use `appsettings.Production.json`:

```powershell
$env:DOTNET_ENVIRONMENT = 'Production'
```

| Comando | Uso | Resultado |
| --- | --- | --- |
| `seed` | `dotnet run --project src/StationSales.Worker -- seed` | Crea o actualiza la configuración sin mover watermarks. |
| `extract` | `dotnet run --project src/StationSales.Worker -- extract` | Ejecuta OpenComb y Gasolution configurados. |
| Extracción por estación | `dotnet run --project src/StationSales.Worker -- extract --source GASOLUTION_EDS_ARGENTINA` | Ejecuta solo una fuente activa por su código. |
| Solo OpenComb | `dotnet run --project src/StationSales.Worker -- extract-opencomb` | Ejecuta fuentes OpenComb que estén vencidas según su intervalo. |
| Solo Gasolution | `dotnet run --project src/StationSales.Worker -- extract-gasolution` | Ejecuta fuentes Gasolution que estén vencidas según su intervalo. |
| Recuperación OpenComb | `dotnet run --project src/StationSales.Worker -- backfill-opencomb --from 2026-09-01 --to 2026-09-27` | Reprocesa el rango local de Lima para todas las fuentes OpenComb activas, sin avanzar watermarks. |
| Recuperación OpenComb por estación | `dotnet run --project src/StationSales.Worker -- backfill-opencomb --source OPENCOMB_EDS_CALLAO --from 2026-09-01 --to 2026-09-27` | Reprocesa una fuente OpenComb activa, sin avanzar su watermark. |
| Recuperación Gasolution | `dotnet run --project src/StationSales.Worker -- backfill-gasolution --from 2026-09-01 --to 2026-09-27` | Reprocesa el rango local de Lima en paralelo de cuatro, sin avanzar watermarks. |
| Transformación | `dotnet run --project src/StationSales.Worker -- transform` | Reserva el punto de entrada de normalización Core; los contratos Core deben aprobarse antes de implementarla. |
| Publicación | `dotnet run --project src/StationSales.Worker -- publish --destination sqlserver` | Punto de entrada de publicación MART. |
| Flujo completo | `dotnet run --project src/StationSales.Worker -- run-all` | Ejecuta extracción, transformación y publicación. |

Para iniciar los workers programados, ejecute el proyecto sin argumentos:

```powershell
dotnet run --project src/StationSales.Worker
```

El worker toma las fuentes activas vencidas y aísla las fallas por estación. La concurrencia máxima está en `Workers:*:MaxDegreeOfParallelism`.

## Estado y seguimiento

La auditoría está en el esquema `etl` de la base central:

- `etl.extraction_run`: estado, ventana, filas leídas/escritas y error técnico.
- `etl.pipeline_data_source`: configuración, intervalo, recuperación y watermark (`LastSuccessfulDate`).
- `raw.opencomb_station_sale` y `raw.gasolution_station_sale`: datos replicados de origen.

Una fuente sin datos termina como `SucceededWithNoData`; no avanza su watermark. Las fuentes con error técnico terminan como `Failed` y no bloquean las demás estaciones.

`StartDate` es el límite inferior de la extracción. La ventana de recuperación nunca consulta fechas anteriores a ese valor; si una ejecución falla antes de establecer un watermark, el siguiente intento vuelve a comenzar en `StartDate`.

## RAW y carga masiva

Cada RAW tiene una clave única `(DataSourceId, SourceRecordId)` y un `RowHash`. La escritura usa una tabla staging y `SqlBulkCopy`: inserta registros nuevos y actualiza solamente contenido que cambió. Esto permite reprocesar la ventana de recuperación sin duplicar filas.

OpenComb almacena también `ebidata` en `raw.opencomb_station_sale.ebidata`.

## Procedimientos Gasolution

Los scripts están en `scripts`:

- `gasolution-argentina-procedure.sql`: versión actualizada del procedimiento.
- `gasolution-procedure-restored.sql`: versión restaurada basada en la consulta anterior.
- `Update-GasolutionProcedures.ps1`: despliega un procedimiento en varias estaciones. El archivo de conexiones con contraseñas no se versiona.

Ejemplo de validación sin cambios:

```powershell
.\scripts\Update-GasolutionProcedures.ps1 `
  -ConnectionsFile .\scripts\gasolution-connections.txt `
  -ProcedureFile .\scripts\gasolution-procedure-restored.sql `
  -Station 'EDS ARGENTINA' `
  -DryRun
```

## Seguridad

No suba `appsettings.Production.json`, archivos de conexiones ni contraseñas. Versione solo el archivo `appsettings.Production.example.json` y use `SecretReference` para enlazar cada fuente con su secreto local.
