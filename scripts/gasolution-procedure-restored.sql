ALTER PROCEDURE [dbo].[RecuperarVentasEnergigas]
    @FechaInicial datetime,
    @FechaFinal datetime
AS
BEGIN
    SET NOCOUNT ON;

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
        WHERE CASE
            WHEN d.FechaHoraDocumento IS NOT NULL
             AND FORMAT(v.FechaHora, 'ddMMyyyy') <> FORMAT(d.FechaHoraDocumento, 'ddMMyyyy')
                THEN d.FechaHoraDocumento
            ELSE v.FechaHora
        END BETWEEN @FechaInicial AND @FechaFinal
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
GO
