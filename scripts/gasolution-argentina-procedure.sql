ALTER PROCEDURE [dbo].[RecuperarVentasEnergigas]
    @FechaInicial datetime,
    @FechaFinal datetime
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Inicio datetime = CONVERT(date, @FechaInicial);
    DECLARE @FinExclusivo datetime = DATEADD(day, 1, CONVERT(date, @FechaFinal));

    SELECT
        v.Recibo,
        cl.RUC,
        cl.RazonSocial,
        fp.Descripcion,
        v.FechaHora,
        v.HoraInicio,
        v.HoraFin,
        th.NumeroTurno,
        e.Cedula,
        e.Nombre AS Despachador,
        vh.Placa AS Placa,
        m.Descripcion AS Manguera,
        s.Descripcion AS Surtidor,
        c.Descripcion AS Cara,
        v.LecturaInicial,
        v.LecturaFinal,
        v.Precio,
        v.Cantidad,
        p.Nombre AS Producto,
        v.Valor,
        v.Descuento,
        v.AbonoCredito AS Recaudo,
        v.EsDescuentoClienteCredito,
        v.ValorCambioPrecio,
        v.TipoRedondeo,
        v.ValorRedondeo,
        v.NroConfirmacionCDC,
        vf.IdVentaFacturacion,
        td.Descripcion AS TipoDocumento,
        td.Codigo AS CodigoSunat,
        d.Prefijo AS Serie,
        d.Numero,
        d.FechaHoraDocumento,
        d.Precio AS PrecioDocumento,
        d.BaseImponible AS BaseImponibleDocumento,
        d.Impuesto AS ImpuestoDocumento,
        d.Subtotal AS SubtotalDocumento,
        d.TotalPagar AS TotalPagarDocumento,
        d.RedondeoDonacion AS RedondeoDonacionDocumento,
        fe.CodigoQr,
        fe.TramaEnviada
    FROM dbo.Venta AS v
    LEFT JOIN dbo.Producto AS p ON v.IdProducto = p.IdProducto
    LEFT JOIN dbo.VentaFacturacion AS vf ON v.Recibo = vf.Recibo
    LEFT JOIN dbo.ClienteLocal AS cl ON v.IdCliente = cl.IdCliente OR vf.RUC = cl.RUC
    LEFT JOIN dbo.Documento AS d ON v.Recibo = d.Recibo
    LEFT JOIN dbo.TipoDocumento AS td ON d.IdTipoDocumento = td.IdTipoDocumento
    LEFT JOIN dbo.Vehiculo AS vh ON v.IdVehiculo = vh.IdVehiculo
    LEFT JOIN dbo.FormaPago AS fp ON fp.IdFormaPago = v.IdFormaPago
    LEFT JOIN dbo.Manguera AS m ON v.IdManguera = m.IdManguera
    LEFT JOIN dbo.Cara AS c ON m.IdCara = c.IdCara
    LEFT JOIN dbo.Surtidor AS s ON s.IdSurtidor = c.IdSurtidor
    LEFT JOIN dbo.Turno AS t ON v.IdTurno = t.IdTurno
    LEFT JOIN dbo.Empleado AS e ON e.IdEmpleado = t.IdEmpleado
    LEFT JOIN dbo.TurnoHorario AS th ON t.IdTurnoHorario = th.IdTurnoHorario
    LEFT JOIN dbo.FacturasElectronicas AS fe ON v.Recibo = fe.Recibo AND d.IdDocumento = fe.IdDocumento
    WHERE v.FechaHora >= @Inicio
      AND v.FechaHora < @FinExclusivo;
END;
GO
