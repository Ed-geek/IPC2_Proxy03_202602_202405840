namespace IPC2_Proxy03_202602_202405840.Models;

public class Factura
{
    public string Referencia { get; set; } = "";
    public string NitEmisor { get; set; } = "";
    public string NitReceptor { get; set; } = "";
    public decimal Valor { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
    public DateTime Fecha { get; set; }
    public string CodigoAprobacion { get; set; } = "";
    public bool EsValida { get; set; }
    public bool DescartadaPorFormato { get; set; }   
    public List<string> Errores { get; set; } = new();
}