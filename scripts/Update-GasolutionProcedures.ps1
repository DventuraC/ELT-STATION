[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateScript({ Test-Path $_ -PathType Leaf })]
    [string]$ConnectionsFile,

    [string]$ProcedureFile,

    [string[]]$Station,

    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$ProcedureName = 'dbo.RecuperarVentasEnergigas'
if ([string]::IsNullOrWhiteSpace($ProcedureFile)) { $ProcedureFile = Join-Path (Split-Path -Parent $PSCommandPath) 'gasolution-argentina-procedure.sql' }

# @FechaFinal is interpreted as a business day. The end is exclusive to retain
# all fractional seconds of that day.
$procedureSql = @'
ALTER PROCEDURE [dbo].[RecuperarVentasEnergigas]
    @FechaInicial datetime,
    @FechaFinal datetime
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Inicio datetime = CONVERT(date, @FechaInicial);
    DECLARE @FinExclusivo datetime = DATEADD(day, 1, CONVERT(date, @FechaFinal));

    ;WITH dataSource AS (
        SELECT
            v.Recibo,
            d.IdDocumento,
            v.IdTurno,
            d.FechaHoraDocumento AS FechaDeclaracion,
            v.FechaHora,
            v.HoraInicio,
            v.HoraFin,
            ISNULL(td.Codigo, d.IdTipoDocumento) AS TipoDocumento,
            d.Prefijo,
            d.Numero,
            v.IdFormaPago,
            v.IdCliente,
            v.Placa,
            '11620308' AS Producto,
            'GNV' AS Descripcion_Producto,
            v.IdManguera,
            v.Cantidad,
            v.Precio,
            v.ValorCambioPrecio,
            v.EsDescuentoClienteCredito,
            v.TipoRedondeo,
            v.ValorRedondeo,
            v.Valor,
            cl.RazonSocial,
            fp.Descripcion,
            cl.RUC,
            fe.TramaEnviada,
            ISNULL(
                CASE WHEN td.Codigo IN ('7', '8') THEN
                    CASE v.EsDescuentoClienteCredito
                        WHEN 1 THEN CASE v.ValorCambioPrecio
                            WHEN 0 THEN d.TotalPagarAbono
                            ELSE d.SubTotal
                        END
                        ELSE d.TotalPagar
                    END
                ELSE d.TotalPagar
                END,
                v.Valor
            ) AS TotalPagar
        FROM dbo.Documento AS d
        INNER JOIN dbo.TipoDocumento AS td ON d.IdTipoDocumento = td.IdTipoDocumento
        INNER JOIN dbo.Venta AS v ON d.Recibo = v.Recibo
        LEFT JOIN dbo.ClienteLocal AS cl ON v.IdCliente = cl.IdCliente
        LEFT JOIN dbo.FacturasElectronicas AS fe ON v.Recibo = fe.Recibo AND d.IdDocumento = fe.IdDocumento
        LEFT JOIN dbo.FormaPago AS fp ON fp.IdFormaPago = v.IdFormaPago
        CROSS APPLY (VALUES (
            CASE
                WHEN d.FechaHoraDocumento IS NOT NULL
                 AND CONVERT(date, v.FechaHora) <> CONVERT(date, d.FechaHoraDocumento)
                    THEN d.FechaHoraDocumento
                ELSE v.FechaHora
            END
        )) AS fechaNegocio(Fecha)
        WHERE fechaNegocio.Fecha >= @Inicio
          AND fechaNegocio.Fecha < @FinExclusivo
    )
    SELECT
        dts.*,
        ROUND(dts.TotalPagar / 1.18, 2) AS BaseImponible,
        dts.TotalPagar - ROUND(dts.TotalPagar / 1.18, 2) AS Impuesto,
        ROUND(dts.TotalPagar / 1.18, 2) / CASE dts.Cantidad WHEN 0 THEN 1 ELSE dts.Cantidad END AS PrecioSinIgv
    FROM dataSource AS dts
    LEFT JOIN dbo.Manguera AS m ON dts.IdManguera = m.IdManguera
    LEFT JOIN dbo.Cara AS c ON m.IdCara = c.IdCara
    LEFT JOIN dbo.Turno AS t ON dts.IdTurno = t.IdTurno
    LEFT JOIN dbo.Empleado AS e ON e.IdEmpleado = t.IdEmpleado
    LEFT JOIN dbo.TurnoHorario AS th ON t.IdTurnoHorario = th.IdTurnoHorario;
END;
'@

if (-not (Test-Path $ProcedureFile -PathType Leaf)) { throw "No se encontro el archivo del procedimiento: $ProcedureFile" }
$procedureSql = ((Get-Content -LiteralPath $ProcedureFile) | Where-Object { $_ -notmatch '^\s*GO\s*$' }) -join [Environment]::NewLine

function Get-ConnectionRecords {
    param([string]$Path)

    $records = [System.Collections.Generic.List[object]]::new()
    $lineNumber = 0
    foreach ($line in Get-Content -LiteralPath $Path) {
        $lineNumber++
        $trimmed = $line.Trim()
        if ([string]::IsNullOrWhiteSpace($trimmed) -or $trimmed.StartsWith('#')) { continue }

        $parts = $trimmed -split '\|', 5
        $emptyFields = @($parts | Where-Object { [string]::IsNullOrWhiteSpace($_) })
        if ($parts.Count -ne 5 -or $emptyFields.Count -gt 0) {
            throw "Linea $lineNumber invalida. Se requieren 5 valores: Estacion|Servidor|BaseDeDatos|Usuario|Contrasena."
        }

        $records.Add([pscustomobject]@{
            Station = $parts[0].Trim()
            Server = $parts[1].Trim()
            Database = $parts[2].Trim()
            UserName = $parts[3].Trim()
            Password = $parts[4]
        })
    }

    if ($records.Count -eq 0) { throw 'No se encontraron conexiones validas.' }
    return $records
}

$connections = Get-ConnectionRecords -Path $ConnectionsFile
if ($Station.Count -gt 0) {
    $connections = @($connections | Where-Object { $_.Station -in $Station })
    if ($connections.Count -eq 0) { throw 'Ninguna estacion solicitada existe en el archivo de conexiones.' }
}
$failures = [System.Collections.Generic.List[string]]::new()

foreach ($connection in $connections) {
    try {
        if ($DryRun) {
            Write-Host "[DRY-RUN] $($connection.Station): se actualizaria $ProcedureName en $($connection.Server)/$($connection.Database)."
            continue
        }

        $builder = [System.Data.SqlClient.SqlConnectionStringBuilder]::new()
        $builder['Data Source'] = $connection.Server
        $builder['Initial Catalog'] = $connection.Database
        $builder['User ID'] = $connection.UserName
        $builder['Password'] = $connection.Password
        $builder['Connect Timeout'] = 20
        $builder['Application Name'] = 'StationSales-Gasolution-ProcedureUpdater'

        $sqlConnection = [System.Data.SqlClient.SqlConnection]::new($builder.ConnectionString)
        try {
            $sqlConnection.Open()
            $command = $sqlConnection.CreateCommand()
            $command.CommandText = $procedureSql
            $command.CommandTimeout = 120
            [void]$command.ExecuteNonQuery()
        }
        finally {
            if ($null -ne $sqlConnection) { $sqlConnection.Dispose() }
        }

        Write-Host "[OK] $($connection.Station): $ProcedureName actualizado en $($connection.Server)/$($connection.Database)."
    }
    catch {
        $failures.Add($connection.Station)
        [Console]::Error.WriteLine(("[ERROR] {0}: {1}" -f $connection.Station, $_.Exception.Message))
    }
}

if ($failures.Count -gt 0) {
    [Console]::Error.WriteLine(("Fallaron {0} conexion(es): {1}" -f $failures.Count, ($failures -join ', ')))
    exit 1
}

Write-Host "Finalizado: $($connections.Count) procedimiento(s) actualizado(s)."
