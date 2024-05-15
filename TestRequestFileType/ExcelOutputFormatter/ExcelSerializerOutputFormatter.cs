using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System.Reflection;
using TestRequestFileType.ExcelOutputFormatter.Attributes;

namespace TestRequestFileType.ExcelOutputFormatter;

// https://learn.microsoft.com/en-us/aspnet/core/web-api/advanced/custom-formatters?view=aspnetcore-8.0
public class ExcelSerializerOutputFormatter : OutputFormatter
{
    public ExcelSerializerOutputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
    }

    //Content-Disposition: inline; filename="myfile.txt"

    public override void WriteResponseHeaders(OutputFormatterWriteContext context)
    {
        base.WriteResponseHeaders(context);
        context.HttpContext.Response.Headers.Add("Content-Disposition", "inline; filename=\"myfile.xlsx\""); // TODO: Add Filename attribute.
    }

    public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
    {
        var httpContext = context.HttpContext;

        if (context.Object is not IEnumerable<object> rows)
            return;

        // Get the first row for headers (could extract object type from IEnumerable instead):
        var genericType = rows.GetType()
            .GetInterfaces()
            .Where(type => type.IsGenericType)
            .Where(type => type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            .Select(type => type.GetGenericArguments()[0])
            .SingleOrDefault();

        if (genericType is null)
            return;

        var properties = genericType.GetProperties()
            .Where(property => property.GetCustomAttribute<ExcelColumnAttribute>() is not null)
            .ToArray();

        using var package = new XLWorkbook();
        var worksheet = package.Worksheets.Add("Sheet_1");

        // Create header row:
        var rowIndex = 1;
        for (int i = 0; i < properties.Length; i++)
        {
            var columnIndex = i + 1;

            var property = properties[i];
            var attribute = property.GetCustomAttribute<ExcelColumnAttribute>()!;

            worksheet.Cell(rowIndex, columnIndex).Value = attribute.Name ?? property.Name;
        }

        worksheet.Row(rowIndex).Style.Font.Bold = true;
        worksheet.Row(rowIndex).Style.Protection.Locked = true;

        rowIndex++;

        foreach (var row in rows)
        {
            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var value = property.GetValue(row);

                var columnIndex = i + 1;
                switch (value)
                {
                    case string @string:
                        worksheet.Cell(rowIndex, columnIndex).Value = @string ?? "";
                        worksheet.Cell(rowIndex, columnIndex).Style.NumberFormat.Format = "@"; // "@": Text, if string is a number, it will not be converted.
                        break;
                    case DateOnly dateOnly:
                        worksheet.Cell(rowIndex, columnIndex).Value = dateOnly.ToDateTime(TimeOnly.MinValue);
                        worksheet.Cell(rowIndex, columnIndex).Style.DateFormat.Format = "dd.MM.yyyy";
                        break;
                    case System.DateTime dateTime:
                        worksheet.Cell(rowIndex, columnIndex).Value = dateTime;
                        worksheet.Cell(rowIndex, columnIndex).Style.DateFormat.Format = "dd.MM.yyyy HH.mm";
                        break;
                    case bool @bool:
                        worksheet.Cell(rowIndex, columnIndex).Value = @bool ? "Ja" : "Nei";
                        break;
                    case int @int:
                        worksheet.Cell(rowIndex, columnIndex).Value = @int;
                        worksheet.Cell(rowIndex, columnIndex).Style.NumberFormat.Format = "0";
                        break;
                    case decimal @decimal:
                        worksheet.Cell(rowIndex, columnIndex).Value = @decimal;
                        worksheet.Cell(rowIndex, columnIndex).Style.NumberFormat.Format = "0.00";
                        break;
                }
            }
            rowIndex++;
        }

        for (int i = 0; i < properties.Length; i++)
        {
            var columnIndex = i + 1;
            worksheet.Column(columnIndex).AdjustToContents();
        }
        worksheet.Column(1).AdjustToContents();
        worksheet.Column(2).AdjustToContents();
        worksheet.Column(4).AdjustToContents();

        var bodyWriter = httpContext.Response.BodyWriter.AsStream();
        package.SaveAs(bodyWriter);
    }
}
