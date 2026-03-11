using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using DormitorySite.Models;

namespace DormitorySite.Services
{
    public static class ReceiptGenerator
    {
        public static byte[] GeneratePdfReceipt(Student student, Floor floor)
        {
            // QuestPDF потребує налаштування ліцензії (Community - безкоштовно)
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Size(PageSizes.A5); // Чек зазвичай меншого формату
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

                    page.Header().Text("КВИТАНЦІЯ ПРО ОПЛАТУ ГУРТОЖИТКУ")
                        .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Item().Text($"Дата: {DateTime.Now:dd.MM.yyyy}");
                        col.Item().LineHorizontal(1);
                        
                        col.Item().PaddingTop(10).Text($"ПІБ мешканця: {student.FullName}");
                        col.Item().Text($"Кімната: №{student.RoomNumber}");
                        col.Item().Text($"Курс: {student.Course}");
                        
                        col.Item().PaddingTop(10).Background(Colors.Grey.Lighten4).Padding(5).Column(innerCol => {
                            innerCol.Item().Text($"Оплачено місяців: {student.PaidMonths}");
                            decimal total = floor.Strategy.CalculateTotal(student.PaidMonths);
                            innerCol.Item().Text($"ЗАГАЛЬНА СУМА: {total} грн").FontSize(14).SemiBold();
                        });

                        col.Item().PaddingTop(20).Text("Печатка закладу: ________________").Italic();
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Сторінка ");
                        x.CurrentPageNumber();
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}