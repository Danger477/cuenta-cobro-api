using Microsoft.AspNetCore.Mvc;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iText.Kernel.Pdf;
using iTextLayout = iText.Layout;
using iTextElement = iText.Layout.Element;

namespace CuentaCobroAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CuentaCobroController : ControllerBase
    {
        [HttpPost("generar")]
        public IActionResult GenerarPDF([FromBody] FechaRequest request)
        {
            var rutaPlantilla = Path.Combine(Directory.GetCurrentDirectory(), "Plantilla", "Cuenta de cobro.docx");
            if (!System.IO.File.Exists(rutaPlantilla))
                return NotFound("La plantilla no se encontró.");

            var rutaTemporal = Path.Combine(Path.GetTempPath(), $"CuentaCobro_{Guid.NewGuid()}.docx");
            System.IO.File.Copy(rutaPlantilla, rutaTemporal, true);

            // 1. Reemplaza las fechas con OpenXml
            using (var doc = WordprocessingDocument.Open(rutaTemporal, true))
            {
                var body = doc.MainDocumentPart!.Document.Body!;
                foreach (var texto in body.Descendants<Text>())
                {
                    if (texto.Text.Contains("{{FECHA_CORTA}}"))
                        texto.Text = texto.Text.Replace("{{FECHA_CORTA}}", request.Fecha);
                }   
                doc.MainDocumentPart.Document.Save();
            }

            // 2. Extrae el texto del docx y genera PDF con iText7
            var rutaPDF = Path.ChangeExtension(rutaTemporal, ".pdf");

            using (var doc = WordprocessingDocument.Open(rutaTemporal, false))
            using (var writer = new PdfWriter(rutaPDF))
            using (var pdf = new PdfDocument(writer))
            using (var layout = new iTextLayout.Document(pdf))
            {
                var parrafos = doc.MainDocumentPart!.Document.Body!.Descendants<Paragraph>();
                foreach (var parrafo in parrafos)
                {
                    var texto = parrafo.InnerText;
                    layout.Add(new iTextElement.Paragraph(texto));
                }
            }

            var bytesPDF = System.IO.File.ReadAllBytes(rutaPDF);
            System.IO.File.Delete(rutaTemporal);
            System.IO.File.Delete(rutaPDF);

            return File(bytesPDF, "application/pdf", "CuentaCobro.pdf");
        }
    }

    public class FechaRequest
    {
        public string Fecha { get; set; } = string.Empty;
    }
}