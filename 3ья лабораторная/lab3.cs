using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Exel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

class Program
{
    // Создание экземпляра класса UserInterface для взаимодействия с пользователем
    private static UserInterface ui = new UserInterface();

    // Флаг, определяющий режим обработки данных (true - из JSON файлов, false - из Excel файла)
    private static bool processingMode = false;
    // Путь к файлу Excel с данными
    private static string filePath = "D:\\Program\\Development\\PROJECT\\C#\\test\\bin\\Debug\\net8.0\\data.xlsx";

    static void Main(string[] args)
    {
        // Подписка на событие UserInput класса UserInterface
        ui.UserInput += OnUserInput;

        // Объявление переменных для хранения списков резервуаров, установок и заводов
        IList<Tank> tanks = null;
        IList<Unit> units = null;
        IList<Factory> factories = null;

        // Чтение данных в зависимости от режима
        if (processingMode)
        {
            // Чтение данных из JSON файлов, если флаг processingMode установлен в true
            tanks = ReadJsonFile<IList<Tank>>("tanks.json");
            units = ReadJsonFile<IList<Unit>>("units.json");
            factories = ReadJsonFile<IList<Factory>>("factories.json");
        }
        else
        {
            // Чтение данных из файла Excel, если флаг processingMode установлен в false
            tanks = ReadTanksFromExcel(filePath);
            units = ReadUnitsFromExcel(filePath);
            factories = ReadFactoriesFromExcel(filePath);
        }

        // Флаг для контроля выхода из цикла меню
        bool exit = false;
        while (!exit)
        {
            // Вывод меню с доступными операциями
            ui.WriteLine("\nМеню:");
            ui.WriteLine("1. Добавить");
            ui.WriteLine("2. Изменить");
            ui.WriteLine("3. Удалить");
            ui.WriteLine("4. Поиск резервуаров (с указанием принадлежности установке и заводу)");
            ui.WriteLine("5. Количество резервуаров и установок");
            ui.WriteLine("6. Общий объем резервуаров");
            ui.WriteLine("7. Список всех резервуаров");
            ui.WriteLine("0. Выход");

            // Запрос выбора операции у пользователя
            string choice = ui.ReadLine("Введите номер операции: ");
            try
            {
                switch (choice)
                {
                    case "1":
                        // Вызов метода для добавления нового элемента
                        AddItem(tanks, units, factories);
                        break;
                    case "2":
                        // Вызов метода для изменения существующего элемента
                        UpdateItem(tanks, units, factories);
                        break;
                    case "3":
                        // Вызов метода для удаления элемента
                        DeleteItem(tanks, units, factories);
                        break;
                    case "4":
                        // Запрос ввода названия резервуара для поиска
                        ui.Write("\nВведите название резервуара для поиска: ");
                        string searchName = ui.ReadLine();
                        // Поиск резервуара по введенному названию
                        var foundTank = tanks.FirstOrDefault(t => t.Name.Contains(searchName));
                        if (foundTank != null)
                        {
                            // Поиск установки, которой принадлежит найденный резервуар (синтаксис запросов)
                            var foundUnit_QuerySyntax = FindUnit_QuerySyntax((IReadOnlyCollection<Unit>)units, (IReadOnlyCollection<Tank>)tanks, foundTank.Name);
                            // Поиск завода, которому принадлежит найденная установка (синтаксис запросов)
                            var foundFactory_QuerySyntax = FindFactory_QuerySyntax((IReadOnlyCollection<Factory>)factories, foundUnit_QuerySyntax);
                            // Вывод информации о принадлежности найденного резервуара установке и заводу (синтаксис запросов)
                            ui.WriteLine($"Найдено: {foundTank.Name}. Принадлежит установке {foundUnit_QuerySyntax.Name} и заводу {foundFactory_QuerySyntax.Name} (синтаксис запросов)");

                            // Поиск установки, которой принадлежит найденный резервуар (синтаксис методов)
                            var foundUnit_MethodSyntax = FindUnit_MethodSyntax((IReadOnlyCollection<Unit>)units, (IReadOnlyCollection<Tank>)tanks, foundTank.Name);
                            // Поиск завода, которому принадлежит найденная установка (синтаксис методов)
                            var foundFactory_MethodSyntax = FindFactory_MethodSyntax((IReadOnlyCollection<Factory>)factories, foundUnit_MethodSyntax);
                            // Вывод информации о принадлежности найденного резервуара установке и заводу (синтаксис методов)
                            ui.WriteLine($"Найдено: {foundTank.Name}. Принадлежит установке {foundUnit_MethodSyntax.Name} и заводу {foundFactory_MethodSyntax.Name} (синтаксис методов)\n");
                        }
                        else
                        {
                            // Вывод сообщения, если резервуар не найден
                            ui.WriteLine("Резервуар не найден");
                        }
                        break;
                    case "5":
                        // Вывод количества резервуаров и установок
                        ui.WriteLine($"Количество резервуаров: {tanks.Count}, " +
                            $"установок: {units.Count}");
                        break;
                    case "6":
                        // Получение общего объема всех резервуаров (синтаксис запросов)
                        var totalVolume_QuerySyntax = GetTotalVolume_QuerySyntax((IReadOnlyCollection<Tank>)tanks);
                        ui.WriteLine($"Общий объем резервуаров: {totalVolume_QuerySyntax} (синтаксис запросов)");

                        // Получение общего объема всех резервуаров (синтаксис методов)
                        var totalVolume_MethodSyntax = GetTotalVolume_MethodSyntax((IReadOnlyCollection<Tank>)tanks);
                        ui.WriteLine($"Общий объем резервуаров: {totalVolume_MethodSyntax} (синтаксис методов)\n");
                        break;
                    case "7":
                        // Вывод информации о каждом резервуаре с указанием установки и завода
                        ui.WriteLine("Все резервуары:");

                        // Итерация по каждому резервуару в списке tanks
                        foreach (var tank in tanks)
                        {
                            // Поиск установки, к которой принадлежит текущий резервуар
                            // Используется метод First() для нахождения первой установки, у которой свойство Id совпадает со свойством UnitId текущего резервуара
                            // Предполагается, что каждый резервуар принадлежит только одной установке
                            var unit = units.First(u => u.Id == tank.UnitId);

                            // Поиск завода, к которому принадлежит найденная установка
                            // Используется метод First() для нахождения первого завода, у которого свойство Id совпадает со свойством FactoryId найденной установки
                            // Предполагается, что каждая установка принадлежит только одному заводу
                            var factory = factories.First(f => f.Id == unit.FactoryId);

                            // Вывод информации о текущем резервуаре
                            ui.WriteLine($"ID: {tank.Id}, {tank.Name} ({unit.Name}, {factory.Name})");
                        }

                        // Завершение выполнения текущей итерации цикла и переход к следующей итерации (если есть еще резервуары в списке)
                        break;
                    case "0":
                        // Установка флага выхода из цикла меню
                        exit = true;
                        // Сериализация всех объектов в JSON файл
                        SerializeToJson((IReadOnlyCollection<Tank>)tanks, (IReadOnlyCollection<Unit>)units, (IReadOnlyCollection<Factory>)factories);
                        break;
                    default:
                        // Вывод сообщения о неверном выборе операции
                        ui.WriteLine("Неверный выбор. Попробуйте еще раз.");
                        break;
                }
            }
            catch (InvalidOperationException ex)
            {
                // Обработка исключения InvalidOperationException
                ui.WriteLine($"Ошибка при выполнении операции: {ex.Message}");
                return;
            }
            catch (ArgumentNullException ex)
            {
                // Обработка исключения ArgumentNullException
                ui.WriteLine($"Ошибка при выполнении операции: {ex.Message}");
                return;
            }
            catch (Exception ex)
            {
                // Обработка непредвиденных исключений
                ui.WriteLine($"Непредсказуемая ошибкка: {ex.Message}");
            }
        }
    }

    // Обработчик события UserInput, вызываемый при вводе пользователем числового значения
    private static void OnUserInput(object sender, UserInputEventArgs e)
    {
        // Вывод сообщения о введенном пользователем значении и времени ввода
        ui.WriteLine($"Пользователь ввел {e.Input} в {e.Timestamp:HH:mm:ss}");
    }

    // Метод для чтения данных из JSON файла
    public static T ReadJsonFile<T>(string fileName)
    {
        try
        {
            // Чтение содержимого JSON файла
            string jsonString = File.ReadAllText(fileName);
            // Десериализация JSON в объект типа T
            return JsonSerializer.Deserialize<T>(jsonString);
        }
        catch (FileNotFoundException ex)
        {
            // Обработка исключения, если файл не найден
            ui.WriteLine($"Файл {fileName} не найден. {ex.Message}");
            return default(T);
        }
        catch (JsonException ex)
        {
            // Обработка исключения, если возникла ошибка при десериализации JSON
            ui.WriteLine($"Ошибка десериализации JSON из файла {fileName}. {ex.Message}");
            return default(T);
        }
        catch (FormatException ex)
        {
            // Обработка исключения, если формат данных в файле некорректный
            ui.WriteLine($"Некорректный формат данных в файле {fileName}. {ex.Message}");
            return default(T);
        }
    }

    // Метод для чтения данных о резервуарах из файла Excel
    private static IList<Tank> ReadTanksFromExcel(string filePath)
    {
        // Создание нового списка для хранения резервуаров
        var tanks = new List<Tank>();

        // Создание объекта приложения Excel
        Exel.Application excelApp = new Exel.Application();
        // Открытие рабочей книги Excel по указанному пути
        Exel.Workbook workbook = excelApp.Workbooks.Open(filePath);
        // Получение листа "Tanks" из рабочей книги
        Exel.Worksheet worksheet = workbook.Sheets["Tanks"];
        // Получение используемого диапазона ячеек на листе
        Exel.Range range = worksheet.UsedRange;

        // Итерация по строкам, начиная со второй (предполагается, что первая строка содержит заголовки)
        for (int row = 2; row <= range.Rows.Count; row++)
        {
            // Создание нового объекта Tank и заполнение его свойств значениями из ячеек
            Tank tank = new Tank
            {
                Id = Convert.ToInt32(((Exel.Range)range.Cells[row, 1]).Value2),
                Name = ((Exel.Range)range.Cells[row, 2]).Value2.ToString(),
                Description = ((Exel.Range)range.Cells[row, 3]).Value2.ToString(),
                Volume = Convert.ToInt32(((Exel.Range)range.Cells[row, 4]).Value2),
                MaxVolume = Convert.ToInt32(((Exel.Range)range.Cells[row, 5]).Value2),
                UnitId = Convert.ToInt32(((Exel.Range)range.Cells[row, 6]).Value2)
            };
            // Добавление объекта Tank в список tanks
            tanks.Add(tank);
        }

        // Закрытие рабочей книги
        workbook.Close();
        // Закрытие приложения Excel
        excelApp.Quit();
        // Освобождение ресурсов, связанных с объектами Excel
        Marshal.ReleaseComObject(worksheet);
        Marshal.ReleaseComObject(workbook);
        Marshal.ReleaseComObject(excelApp);

        // Возврат списка резервуаров
        return tanks;
    }

    // Метод для чтения данных об установках из файла Excel
    private static IList<Unit> ReadUnitsFromExcel(string filePath)
    {
        // Создание нового списка для хранения установок
        var units = new List<Unit>();

        // Создание объекта приложения Excel
        Exel.Application excelApp = new Exel.Application();
        // Открытие рабочей книги Excel по указанному пути
        Exel.Workbook workbook = excelApp.Workbooks.Open(filePath);
        // Получение листа "Units" из рабочей книги
        Exel.Worksheet worksheet = workbook.Sheets["Units"];
        // Получение используемого диапазона ячеек на листе
        Exel.Range range = worksheet.UsedRange;

        // Итерация по строкам, начиная со второй (предполагается, что первая строка содержит заголовки)
        for (int row = 2; row <= range.Rows.Count; row++)
        {
            // Создание нового объекта Unit и заполнение его свойств значениями из ячеек
            Unit unit = new Unit
            {
                Id = Convert.ToInt32(((Exel.Range)range.Cells[row, 1]).Value2),
                Name = ((Exel.Range)range.Cells[row, 2]).Value2.ToString(),
                Description = ((Exel.Range)range.Cells[row, 3]).Value2.ToString(),
                FactoryId = Convert.ToInt32(((Exel.Range)range.Cells[row, 4]).Value2)
            };
            // Добавление объекта Unit в список units
            units.Add(unit);
        }

        // Закрытие рабочей книги
        workbook.Close();
        // Закрытие приложения Excel
        excelApp.Quit();
        // Освобождение ресурсов, связанных с объектами Excel
        Marshal.ReleaseComObject(worksheet);
        Marshal.ReleaseComObject(workbook);
        Marshal.ReleaseComObject(excelApp);

        // Возврат списка установок
        return units;
    }

    // Метод для чтения данных о заводах из файла Excel
    private static IList<Factory> ReadFactoriesFromExcel(string filePath)
    {
        // Создание нового списка для хранения заводов
        var factories = new List<Factory>();

        // Создание объекта приложения Excel
        Exel.Application excelApp = new Exel.Application();
        // Открытие рабочей книги Excel по указанному пути
        Exel.Workbook workbook = excelApp.Workbooks.Open(filePath);
        // Получение листа "Factories" из рабочей книги
        Exel.Worksheet worksheet = workbook.Sheets["Factories"];
        // Получение используемого диапазона ячеек на листе
        Exel.Range range = worksheet.UsedRange;

        // Итерация по строкам, начиная со второй (предполагается, что первая строка содержит заголовки)
        for (int row = 2; row <= range.Rows.Count; row++)
        {
            // Создание нового объекта Factory и заполнение его свойств значениями из ячеек
            Factory factory = new Factory
            {
                Id = Convert.ToInt32(((Exel.Range)range.Cells[row, 1]).Value2),
                Name = ((Exel.Range)range.Cells[row, 2]).Value2.ToString(),
                Description = ((Exel.Range)range.Cells[row, 3]).Value2.ToString()
            };
            // Добавление объекта Factory в список factories
            factories.Add(factory);
        }

        // Закрытие рабочей книги
        workbook.Close();
        // Закрытие приложения Excel
        excelApp.Quit();
        // Освобождение ресурсов, связанных с объектами Excel
        Marshal.ReleaseComObject(worksheet);
        Marshal.ReleaseComObject(workbook);
        Marshal.ReleaseComObject(excelApp);

        // Возврат списка заводов
        return factories;
    }

    static void AddItem(IList<Tank> tanks, IList<Unit> units, IList<Factory> factories)
    {
        // Флаги для проверки корректности введенных ID резервуара и установки
        bool isValidTankId, isValidUnitId;
        // Переменные для хранения ID резервуара и установки
        int tankId, unitId;

        do
        {
            // Запрос ввода ID резервуара или выбора автоматического подбора
            ui.Write("Введите ID резервуара, auto - для автоматического подбора или 0 для выхода: ");
            string input = ui.ReadLine();
            // Если введен 0, выход из метода
            if (input == "0")
                return;

            // Если выбран автоматический подбор ID резервуара
            if (input.ToLower() == "auto")
            {
                // Начальное значение ID резервуара
                tankId = 1;
                while (true)
                {
                    // Проверка, существует ли резервуар с текущим ID
                    if (!tanks.Any(t => t.Id == tankId))
                    {
                        // Если не существует, выход из цикла
                        break;
                    }
                    else
                    {
                        // Если существует, увеличение ID на 1 и продолжение поиска
                        tankId++;
                    }
                }
                // Выход из цикла ввода ID резервуара
                break;
            }

            // Проверка корректности введенного ID резервуара
            isValidTankId = int.TryParse(input, out tankId) && !tanks.Any(t => t.Id == tankId);
            // Если ID некорректный или занят, вывод сообщения об ошибке и повторный ввод
            if (!isValidTankId)
                ui.WriteLine("Некорректный или занятый ID резервуара. Повторите ввод или введите 0 для отмены.");
        } while (!isValidTankId);

        // Запрос ввода названия резервуара
        ui.Write("Введите название резервуара: ");
        string tankName = ui.ReadLine();
        // Запрос ввода описания резервуара
        ui.Write("Введите описание резервуара: ");
        string tankDescription = ui.ReadLine();
        // Запрос ввода объема резервуара
        ui.Write("Введите объем резервуара: ");
        int tankVolume = int.Parse(ui.ReadLine());
        // Запрос ввода максимального объема резервуара
        ui.Write("Введите максимальный объем резервуара: ");
        int tankMaxVolume = int.Parse(ui.ReadLine());

        do
        {
            // Запрос ввода ID установки
            ui.Write("Введите ID установки или 0 для отмены: ");
            string input = ui.ReadLine();
            // Если введен 0, выход из метода
            if (input == "0")
                return;

            // Проверка корректности введенного ID установки
            isValidUnitId = int.TryParse(input, out unitId) && units.Any(u => u.Id == unitId);
            // Если ID некорректный или отсутствует, вывод сообщения об ошибке и повторный ввод
            if (!isValidUnitId)
                ui.WriteLine("Некорректный или отсутствующий ID установки. Повторите ввод или введите 0 для отмены.");
        } while (!isValidUnitId);

        // Создание нового объекта Tank с введенными данными
        Tank newTank = new Tank
        {
            Id = tankId,
            Name = tankName,
            Description = tankDescription,
            Volume = tankVolume,
            MaxVolume = tankMaxVolume,
            UnitId = unitId
        };

        // Добавление нового резервуара в список tanks
        tanks.Add(newTank);

        // Если включен режим обработки JSON файлов
        if (processingMode)
        {
            // Сериализация списка резервуаров в JSON файл
            SerializeToJsonFile("tanks.json", tanks);
        }
        else
        {
            // Если включен режим обработки Excel файлов
            // Запись данных в Excel файл
            WriteToExcel(filePath, tanks, units, factories);
        }

        // Вывод сообщения об успешном добавлении резервуара
        ui.WriteLine("Резервуар успешно добавлен.");
    }

    static void UpdateItem(IList<Tank> tanks, IList<Unit> units, IList<Factory> factories)
    {
        // Запрос ввода ID резервуара для изменения
        ui.Write("Введите ID резервуара для изменения: ");
        int tankId = int.Parse(ui.ReadLine());

        // Поиск резервуара с указанным ID в списке tanks
        Tank tankToUpdate = tanks.FirstOrDefault(t => t.Id == tankId);

        // Если резервуар найден
        if (tankToUpdate != null)
        {
            // Флаг для проверки корректности введенного ID установки
            bool isValidUnitId;
            // Переменная для хранения ID установки
            int unitId;

            // Запрос ввода нового названия резервуара
            ui.Write("Введите новое название резервуара: ");
            tankToUpdate.Name = ui.ReadLine();
            // Запрос ввода нового описания резервуара
            ui.Write("Введите новое описание резервуара: ");
            tankToUpdate.Description = ui.ReadLine();
            // Запрос ввода нового объема резервуара
            ui.Write("Введите новый объем резервуара: ");
            tankToUpdate.Volume = int.Parse(ui.ReadLine());
            // Запрос ввода нового максимального объема резервуара
            ui.Write("Введите новый максимальный объем резервуара: ");
            tankToUpdate.MaxVolume = int.Parse(ui.ReadLine());

            do
            {
                // Запрос ввода ID установки
                ui.Write("Введите ID установки или 0 для отмены: ");
                string input = ui.ReadLine();
                // Если введен 0, выход из метода
                if (input == "0")
                    return;

                // Проверка корректности введенного ID установки
                isValidUnitId = int.TryParse(input, out unitId) && units.Any(u => u.Id == unitId);
                // Если ID некорректный или отсутствует, вывод сообщения об ошибке и повторный ввод
                if (!isValidUnitId)
                    ui.WriteLine("Некорректный или отсутствующий ID установки. Повторите ввод или введите 0 для отмены.");
            } while (!isValidUnitId);

            // Если включен режим обработки JSON файлов
            if (processingMode)
            {
                // Сериализация списка резервуаров в JSON файл
                SerializeToJsonFile("tanks.json", tanks);
            }
            else
            {
                // Если включен режим обработки Excel файлов
                // Запись данных в Excel файл
                WriteToExcel(filePath, tanks, units, factories);
            }

            // Вывод сообщения об успешном изменении резервуара
            ui.WriteLine("Резервуар успешно изменен.");
        }
        else
        {
            // Если резервуар с указанным ID не найден, вывод сообщения об ошибке
            ui.WriteLine("Резервуар с указанным ID не найден.");
        }
    }

    static void DeleteItem(IList<Tank> tanks, IList<Unit> units, IList<Factory> factories)
    {
        // Запрос ввода ID резервуара для удаления
        ui.Write("Введите ID резервуара для удаления: ");
        int tankId = int.Parse(ui.ReadLine());

        // Поиск резервуара с указанным ID в списке tanks
        Tank tankToDelete = tanks.FirstOrDefault(t => t.Id == tankId);

        // Если резервуар найден
        if (tankToDelete != null)
        {
            // Удаление резервуара из списка tanks
            tanks.Remove(tankToDelete);

            // Если включен режим обработки JSON файлов
            if (processingMode)
            {
                // Сериализация списка резервуаров в JSON файл
                SerializeToJsonFile("tanks.json", tanks);
            }
            else
            {
                // Если включен режим обработки Excel файлов
                // Запись данных в Excel файл
                WriteToExcel(filePath, tanks, units, factories);
            }

            // Вывод сообщения об успешном удалении резервуара
            ui.WriteLine("Резервуар успешно удален.");
        }
        else
        {
            // Если резервуар с указанным ID не найден, вывод сообщения об ошибке
            ui.WriteLine("Резервуар с указанным ID не найден.");
        }
    }

    private static void WriteToExcel(string filePath, IList<Tank> tanks, IList<Unit> units, IList<Factory> factories)
    {
        // Создание объекта приложения Excel
        Exel.Application excelApp = new Exel.Application();
        // Открытие рабочей книги Excel по указанному пути
        Exel.Workbook workbook = excelApp.Workbooks.Open(filePath);

        // Запись данных резервуаров
        // Получение листа "Tanks" из рабочей книги
        Exel.Worksheet tankSheet = workbook.Sheets["Tanks"];
        // Получение используемого диапазона ячеек на листе "Tanks"
        Exel.Range tankRange = tankSheet.UsedRange;
        // Очистка содержимого используемого диапазона ячеек
        tankRange.Clear();
        // Запись заголовков столбцов
        tankRange.Cells[1, 1] = "Id";
        tankRange.Cells[1, 2] = "Name";
        tankRange.Cells[1, 3] = "Description";
        tankRange.Cells[1, 4] = "Volume";
        tankRange.Cells[1, 5] = "MaxVolume";
        tankRange.Cells[1, 6] = "UnitId";

        // Запись данных резервуаров в ячейки
        for (int i = 0; i < tanks.Count; i++)
        {
            tankRange.Cells[i + 2, 1] = tanks[i].Id;
            tankRange.Cells[i + 2, 2] = tanks[i].Name;
            tankRange.Cells[i + 2, 3] = tanks[i].Description;
            tankRange.Cells[i + 2, 4] = tanks[i].Volume;
            tankRange.Cells[i + 2, 5] = tanks[i].MaxVolume;
            tankRange.Cells[i + 2, 6] = tanks[i].UnitId;
        }

        // Запись данных установок
        // Получение листа "Units" из рабочей книги
        Exel.Worksheet unitSheet = workbook.Sheets["Units"];
        // Получение используемого диапазона ячеек на листе "Units"
        Exel.Range unitRange = unitSheet.UsedRange;
        // Очистка содержимого используемого диапазона ячеек
        unitRange.Clear();
        // Запись заголовков столбцов
        unitRange.Cells[1, 1] = "Id";
        unitRange.Cells[1, 2] = "Name";
        unitRange.Cells[1, 3] = "Description";
        unitRange.Cells[1, 4] = "FactoryId";

        // Запись данных установок в ячейки
        for (int i = 0; i < units.Count; i++)
        {
            unitRange.Cells[i + 2, 1] = units[i].Id;
            unitRange.Cells[i + 2, 2] = units[i].Name;
            unitRange.Cells[i + 2, 3] = units[i].Description;
            unitRange.Cells[i + 2, 4] = units[i].FactoryId;
        }

        // Запись данных заводов
        // Получение листа "Factories" из рабочей книги
        Exel.Worksheet factorySheet = workbook.Sheets["Factories"];
        // Получение используемого диапазона ячеек на листе "Factories"
        Exel.Range factoryRange = factorySheet.UsedRange;
        // Очистка содержимого используемого диапазона ячеек
        factoryRange.Clear();
        // Запись заголовков столбцов
        factoryRange.Cells[1, 1] = "Id";
        factoryRange.Cells[1, 2] = "Name";
        factoryRange.Cells[1, 3] = "Description";

        // Запись данных заводов в ячейки
        for (int i = 0; i < factories.Count; i++)
        {
            factoryRange.Cells[i + 2, 1] = factories[i].Id;
            factoryRange.Cells[i + 2, 2] = factories[i].Name;
            factoryRange.Cells[i + 2, 3] = factories[i].Description;
        }

        // Сохранение изменений в рабочей книге
        workbook.Save();
        // Закрытие рабочей книги
        workbook.Close();
        // Закрытие приложения Excel
        excelApp.Quit();
        // Освобождение ресурсов, связанных с объектами Excel
        Marshal.ReleaseComObject(factorySheet);
        Marshal.ReleaseComObject(unitSheet);
        Marshal.ReleaseComObject(tankSheet);
        Marshal.ReleaseComObject(workbook);
        Marshal.ReleaseComObject(excelApp);
    }

    // Метод для поиска установки по имени резервуара (синтаксис методов)
    public static Unit FindUnit_MethodSyntax(IReadOnlyCollection<Unit> units, IReadOnlyCollection<Tank> tanks, string tankName)
    {
        // Проверка на null для параметров units, tanks и tankName
        if (units == null || tanks == null || string.IsNullOrEmpty(tankName))
            // Если какой-либо из параметров равен null или tankName пустая строка, выбрасывается исключение ArgumentNullException
            throw new ArgumentNullException("Один или несколько параметров имеют значение null.");

        // Поиск установки, используя методы расширения LINQ
        // Метод FirstOrDefault возвращает первый элемент последовательности, удовлетворяющий условию, или значение по умолчанию, если таких элементов нет
        // Условие: установка u, для которой существует резервуар t с именем tankName и идентификатором установки, равным идентификатору установки u
        var foundUnit = units.FirstOrDefault(u => tanks.Any(t => t.Name == tankName && t.UnitId == u.Id));

        // Проверка, найдена ли установка
        if (foundUnit == null)
            // Если установка не найдена, выбрасывается исключение InvalidOperationException с сообщением
            throw new InvalidOperationException($"Установка для резервуара '{tankName}' не найдена.");

        // Возвращение найденной установки
        return foundUnit;
    }

    // Метод для поиска завода по установке (синтаксис методов)
    public static Factory FindFactory_MethodSyntax(IReadOnlyCollection<Factory> factories, IIdentifiable unit)
    {
        // Проверка на null для параметров factories и unit
        if (factories == null || unit == null)
            // Если какой-либо из параметров равен null, выбрасывается исключение ArgumentNullException
            throw new ArgumentNullException("Один или несколько параметров имеют значение null.");

        // Поиск завода, используя методы расширения LINQ
        // Метод FirstOrDefault возвращает первый элемент последовательности, удовлетворяющий условию, или значение по умолчанию, если таких элементов нет
        // Условие: завод f, идентификатор которого равен идентификатору установки unit
        var foundFactory = factories.FirstOrDefault(f => f.Id == unit.Id);

        // Проверка, найден ли завод
        if (foundFactory == null)
            // Если завод не найден, выбрасывается исключение InvalidOperationException с сообщением
            throw new InvalidOperationException($"Завод для установки '{unit.Name}' не найден.");

        // Возвращение найденного завода
        return foundFactory;
    }

    // Метод для получения общего объема всех резервуаров (синтаксис методов)
    public static int GetTotalVolume_MethodSyntax(IReadOnlyCollection<Tank> tanks)
    {
        // Проверка на null для параметра tanks
        if (tanks == null)
            // Если параметр tanks равен null, выбрасывается исключение ArgumentNullException
            throw new ArgumentNullException("Параметр 'tanks' имеет значение null.");

        // Вычисление общего объема резервуаров, используя метод Sum из LINQ
        // Метод Sum суммирует значения объема (Volume) для каждого резервуара t в коллекции tanks
        var totalVolume = tanks.Sum(t => t.Volume);

        // Проверка, есть ли резервуары
        if (totalVolume == 0)
            // Если общий объем равен 0, выбрасывается исключение InvalidOperationException с сообщением
            throw new InvalidOperationException("Общий объем резервуаров не может быть вычислен. Нет резервуаров.");

        // Возвращение общего объема резервуаров
        return totalVolume;
    }

    // Метод для поиска установки по имени резервуара (синтаксис запросов)
    public static Unit FindUnit_QuerySyntax(IReadOnlyCollection<Unit> units, IReadOnlyCollection<Tank> tanks, string tankName)
    {
        // Проверка на null для параметров units, tanks и tankName
        if (units == null || tanks == null || string.IsNullOrEmpty(tankName))
            // Если какой-либо из параметров равен null или tankName пустая строка, выбрасывается исключение ArgumentNullException
            throw new ArgumentNullException("Один или несколько параметров имеют значение null.");

        // Создание запроса для поиска установки по имени резервуара
        // Запрос выполняет соединение (join) между коллекциями units и tanks по условию unit.Id equals tank.UnitId
        // Затем фильтрует результаты по условию tank.Name == tankName
        // И выбирает (select) объекты unit, удовлетворяющие условиям
        var query = from unit in units
                    join tank in tanks on unit.Id equals tank.UnitId
                    where tank.Name == tankName
                    select unit;

        // Получение первого найденного элемента из результатов запроса или значения по умолчанию, если элементов нет
        var foundUnit = query.FirstOrDefault();
        // Проверка, найдена ли установка
        if (foundUnit == null)
            // Если установка не найдена, выбрасывается исключение InvalidOperationException с сообщением
            throw new InvalidOperationException($"Установка для резервуара '{tankName}' не найдена.");

        // Возвращение найденной установки
        return foundUnit;
    }

    // Метод для поиска завода по установке (синтаксис запросов)
    public static Factory FindFactory_QuerySyntax(IReadOnlyCollection<Factory> factories, IIdentifiable unit)
    {
        // Проверка на null для параметров factories и unit
        if (factories == null || unit == null)
            // Если какой-либо из параметров равен null, выбрасывается исключение ArgumentNullException
            throw new ArgumentNullException("Один или несколько параметров имеют значение null.");

        // Создание запроса для поиска завода по установке
        // Запрос фильтрует коллекцию factories по условию factory.Id == unit.Id
        // И выбирает (select) объекты factory, удовлетворяющие условию
        var query = from factory in factories
                    where factory.Id == unit.Id
                    select factory;

        // Получение первого найденного элемента из результатов запроса или значения по умолчанию, если элементов нет
        var foundFactory = query.FirstOrDefault();
        // Проверка, найден ли завод
        if (foundFactory == null)
            // Если завод не найден, выбрасывается исключение InvalidOperationException с сообщением
            throw new InvalidOperationException($"Завод для установки '{unit.Name}' не найден.");

        // Возвращение найденного завода
        return foundFactory;
    }

    // Метод для получения общего объема всех резервуаров (синтаксис запросов)
    public static int GetTotalVolume_QuerySyntax(IReadOnlyCollection<Tank> tanks)
    {
        // Проверка на null для параметра tanks
        if (tanks == null)
            // Если параметр tanks равен null, выбрасывается исключение ArgumentNullException
            throw new ArgumentNullException("Параметр 'tanks' имеют значение null.");

        // Создание запроса для получения общего объема резервуаров
        // Запрос выбирает (select) значения объема (Volume) для каждого резервуара tank в коллекции tanks
        var query = from tank in tanks
                    select tank.Volume;

        // Вычисление суммы значений объема из результатов запроса
        var totalVolume = query.Sum();
        // Проверка, есть ли резервуары
        if (totalVolume == 0)
            // Если общий объем равен 0, выбрасывается исключение InvalidOperationException с сообщением
            throw new InvalidOperationException("Общий объем резервуаров не может быть вычислен. Нет резервуаров.");

        // Возвращение общего объема резервуаров
        return totalVolume;
    }

    // Метод для сериализации всех объектов в JSON файл
    public static void SerializeToJson(IReadOnlyCollection<Tank> tanks, IReadOnlyCollection<Unit> units, IReadOnlyCollection<Factory> factories)
    {
        // Создание анонимного объекта для хранения коллекций tanks, units и factories
        var data = new
        {
            Tanks = tanks,
            Units = units,
            Factories = factories
        };

        try
        {
            // Сериализация объекта data в формат JSON с отступами для удобочитаемости
            string jsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            // Запись сериализованной строки JSON в файл "data.json"
            File.WriteAllText("data.json", jsonString);
        }
        catch (ArgumentException ex)
        {
            // Обработка исключения ArgumentException при ошибке сериализации данных в JSON
            ui.WriteLine($"Ошибка сериализации данных в JSON. {ex.Message}");
        }
    }

    // Обобщенный метод для сериализации объекта в JSON файл
    static void SerializeToJsonFile<T>(string fileName, T data)
    {
        try
        {
            // Сериализация объекта data в формат JSON с отступами для удобочитаемости
            string jsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            // Запись сериализованной строки JSON в файл с указанным именем fileName
            File.WriteAllText(fileName, jsonString);
        }
        catch (Exception ex)
        {
            // Обработка исключения Exception при ошибке сериализации данных в JSON файл
            ui.WriteLine($"Ошибка сериализации данных в JSON файл {fileName}. {ex.Message}");
        }
    }
}

// Класс UserInterface, представляющий пользовательский интерфейс
public class UserInterface
{
    // Событие UserInput, возникающее при вводе пользователем числового значения
    public event EventHandler<UserInputEventArgs> UserInput;

    // Метод для вывода сообщения на консоль с переводом строки
    public void WriteLine(string message)
    {
        Console.WriteLine(message);
    }

    // Метод для вывода сообщения на консоль без перевода строки
    public void Write(string message)
    {
        Console.Write(message);
    }

    // Метод для чтения строки ввода пользователя с выводом приглашения prompt
    public string ReadLine(string prompt)
    {
        Console.Write(prompt);
        string input = Console.ReadLine();

        // Если введенное значение является целым числом или числом с плавающей точкой,
        // вызывается метод OnUserInput для генерации события UserInput
        if (int.TryParse(input, out int value) || double.TryParse(input, out double d_value))
            OnUserInput(input);

        return input;
    }

    // Метод для чтения строки ввода пользователя без вывода приглашения
    public string ReadLine()
    {
        string input = Console.ReadLine();

        // Если введенное значение является целым числом,
        // вызывается метод OnUserInput для генерации события UserInput
        if (int.TryParse(input, out int value))
            OnUserInput(input);

        return input;
    }

    // Виртуальный метод для генерации события UserInput
    protected virtual void OnUserInput(string input)
    {
        UserInput?.Invoke(this, new UserInputEventArgs(input, DateTime.Now));
    }
}

// Класс UserInputEventArgs, представляющий аргументы события UserInput
public class UserInputEventArgs : EventArgs
{
    // Свойство, содержащее введенное пользователем значение
    public string Input { get; }
    // Свойство, содержащее временную метку ввода
    public DateTime Timestamp { get; }

    // Конструктор класса UserInputEventArgs
    public UserInputEventArgs(string input, DateTime timestamp)
    {
        Input = input;
        Timestamp = timestamp;
    }
}

// Интерфейс IIdentifiable, определяющий общие свойства для классов Factory, Unit и Tank
public interface IIdentifiable
{
    int Id { get; set; }
    string Name { get; set; }
}

// Класс Unit, представляющий установку, реализует интерфейс IIdentifiable
public class Unit : IIdentifiable
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int FactoryId { get; set; }
}

// Класс Factory, представляющий завод, реализует интерфейс IIdentifiable
public class Factory : IIdentifiable
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

// Класс Tank, представляющий резервуар, реализует интерфейс IIdentifiable
public class Tank : IIdentifiable
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Volume { get; set; }
    public int MaxVolume { get; set; }
    public int UnitId { get; set; }
}
