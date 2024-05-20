using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

class Program
{
    static void Main(string[] args)
    {
        // Чтение данных из JSON файла
        string jsonString = File.ReadAllText("JSON_sample_1.json");
        var deals = JsonSerializer.Deserialize<List<Deal>>(jsonString);


        // Вызов метода GetNumbersOfDeals и вывод результатов
        Console.WriteLine("Номера сделок (синтаксис запросов):");
        var dealNumbers_QuerySyntax = GetNumbersOfDeals_QuerySyntax(deals);
        Console.WriteLine($"Количество найденных значений: {dealNumbers_QuerySyntax.Count}");
        Console.WriteLine($"Идентификаторы: {string.Join(", ", dealNumbers_QuerySyntax)}");

        Console.WriteLine("\nНомера сделок (синтаксис методов):");
        var dealNumbers_MethodSyntax = GetNumbersOfDeals_MethodSyntax(deals);
        Console.WriteLine($"Количество найденных значений: {dealNumbers_MethodSyntax.Count}");
        Console.WriteLine($"Идентификаторы: {string.Join(", ", dealNumbers_MethodSyntax)}");


        // Вызов метода GetSumsByMonth и вывод результатов
        Console.WriteLine("\nСуммы сделок по месяцам (синтаксис запросов):");
        var sumsByMonth_QuerySyntax = GetSumsByMonth_QuerySyntax(deals);
        foreach (var sumByMonth in sumsByMonth_QuerySyntax)
        {
            Console.WriteLine($"{sumByMonth.Month:yyyy-MM}: {sumByMonth.Sum}");
        }

        Console.WriteLine("\nСуммы сделок по месяцам (синтаксис методов):");
        var sumsByMonth_MethodSyntax = GetSumsByMonth_MethodSyntax(deals);
        foreach (var sumByMonth in sumsByMonth_MethodSyntax)
        {
            Console.WriteLine($"{sumByMonth.Month:yyyy-MM}: {sumByMonth.Sum}");
        }
    }

    // Метод GetNumbersOfDeals (синтаксис запросов)
    static IList<string> GetNumbersOfDeals_QuerySyntax(IEnumerable<Deal> deals)
    {
        // Фильтрует сделки по сумме (не меньше 100 рублей),
        // среди отфильтрованных берёт 5 сделок с самой ранней датой,
        // возвращает номера (поле Id) в отсортированном по сумме по убыванию виде
        var query = from deal in deals
                    where deal.Sum >= 100
                    orderby deal.Date
                    select deal.Id;

        return query.Take(5).OrderByDescending(id => id).ToList();
        /* Закомментированный возврат только уникальных записей
        return query.Take(5).Distinct().OrderByDescending(id => id).ToList();
        */
    }

    // Метод GetNumbersOfDeals (синтаксис методов)
    static IList<string> GetNumbersOfDeals_MethodSyntax(IEnumerable<Deal> deals)
    {
        // Фильтрует сделки по сумме (не меньше 100 рублей),
        // среди отфильтрованных берёт 5 сделок с самой ранней датой,
        // возвращает номера (поле Id) в отсортированном по сумме по убыванию виде
        return deals
            .Where(deal => deal.Sum >= 100)
            .OrderBy(deal => deal.Date)
            .Take(5)
            .Select(deal => deal.Id)
            .OrderByDescending(id => id)
            .ToList();
        /* Закомментированный возврат только уникальных записей
        return deals
            .Where(deal => deal.Sum >= 100)
            .OrderBy(deal => deal.Date)
            .Take(5)
            .Select(deal => deal.Id)
            .Distinct()
            .OrderByDescending(id => id)
            .ToList();
        */
    }

    // Метод GetSumsByMonth (синтаксис запросов)
    static IList<SumByMonth> GetSumsByMonth_QuerySyntax(IEnumerable<Deal> deals)
    {
        // Группирует сделки по месяцу и возвращает сумму сделок за каждый месяц
        var query = from deal in deals
                    group deal by new DateTime(deal.Date.Year, deal.Date.Month, 1) into g
                    orderby g.Key
                    select new SumByMonth(g.Key, g.Sum(deal => deal.Sum));

        return query.ToList();
    }

    // Метод GetSumsByMonth (синтаксис методов)
    static IList<SumByMonth> GetSumsByMonth_MethodSyntax(IEnumerable<Deal> deals)
    {
        // Группирует сделки по месяцу и возвращает сумму сделок за каждый месяц
        return deals
            .GroupBy(deal => new DateTime(deal.Date.Year, deal.Date.Month, 1))
            .OrderBy(g => g.Key)
            .Select(g => new SumByMonth(g.Key, g.Sum(deal => deal.Sum)))
            .ToList();
    }
}

// Класс Deal, представляющий структуру сделки
class Deal
{
    public int Sum { get; set; }
    public string Id { get; set; }
    public DateTime Date { get; set; }
}

// Запись SumByMonth, представляющая результат группировки сделок по месяцам
record SumByMonth(DateTime Month, int Sum);
