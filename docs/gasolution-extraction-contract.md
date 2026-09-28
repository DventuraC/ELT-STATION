# Contrato de extracción Gasolution — borrador Argentina

Fuente: `dbo.RecuperarVentasEnergigas(@FechaInicial, @FechaFinal)` en `Hadi`.
Las fechas delimitan días de negocio de Lima: inicio inclusivo y día final completo.

## Identidad y ventana

| Concepto | Campo / regla |
| --- | --- |
| Documento de origen | Clave condicional descrita abajo |
| Fecha de negocio | `FechaHora` |
| Marca de actualización | No disponible en el resultado; se registra `NULL` |
| Ventana | `FechaHora >= FechaInicial 00:00` y `< FechaFinal + 1 día` |
| Zona horaria | `America/Lima` |

La clave estable de la fila/documento es condicional:

```text
si Serie y Numero existen:  DOC|Recibo|Serie|Numero
si no existen:              VF|Recibo|IdVentaFacturacion
```

El prefijo (`DOC` o `VF`) evita colisiones entre ambos casos. No se convierten nulos a texto vacío.

La validación de septiembre de 2026 confirmó el modelo: 5,727 filas usaron la rama `DOC`, 3,267 la rama `VF`, no hubo filas sin clave y las 8,994 claves condicionales fueron únicas. Por tanto, esta clave será `SourceRecordId` de RAW y también el identificador estable de documento mientras el SP devuelva una fila por documento.

La validación de septiembre de 2026 devolvió 8,994 filas y 8,916 recibos distintos: hay 78 filas adicionales asociadas a recibos repetidos. La clave condicional las distingue sin usar un ordinal de lectura.

## Campos entregados

| Grupo | Campos |
| --- | --- |
| Venta | `Recibo`, `FechaHora`, `HoraInicio`, `HoraFin`, `LecturaInicial`, `LecturaFinal`, `Precio`, `Cantidad`, `Valor`, `Descuento`, `Recaudo` |
| Producto y surtidor | `Producto`, `Manguera`, `Surtidor`, `Cara` |
| Cliente y pago | `RUC`, `RazonSocial`, `Descripcion` (forma de pago), `Placa` |
| Turno | `NumeroTurno`, `Cedula`, `Despachador` |
| Documento | `IdVentaFacturacion`, `TipoDocumento`, `CodigoSunat`, `Serie`, `Numero`, `FechaHoraDocumento`, `PrecioDocumento`, `BaseImponibleDocumento`, `ImpuestoDocumento`, `SubtotalDocumento`, `TotalPagarDocumento`, `RedondeoDonacionDocumento` |
| Ajustes y fiscal | `EsDescuentoClienteCredito`, `ValorCambioPrecio`, `TipoRedondeo`, `ValorRedondeo`, `NroConfirmacionCDC`, `CodigoQr`, `TramaEnviada` |

## Validaciones pendientes en la primera ejecución

1. Confirmar el mismo resultado de unicidad en las demás estaciones Gasolution.
2. Verificar que un futuro cambio no produzca más de una fila para una misma clave condicional.
3. Tipos reales de cada columna y valores nulos.
