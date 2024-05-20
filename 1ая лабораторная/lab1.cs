// Объявление класса Program
class Program
{
    // Метод Main - точка входа в программу
    static void Main(string[] args)
    {
        // Получение массива резервуаров
        var tanks = GetTanks();
        // Получение массива установок
        var units = GetUnits();
        // Получение массива заводов
        var factories = GetFactories();

        // Вывод количества резервуаров и установок
        Console.WriteLine($"Количество резервуаров: {tanks.Length}, установок: {units.Length} \n");

        // Поиск установки, которой принадлежит "Резервуар 2"
        var foundUnit = FindUnit(units, tanks, "Резервуар 2");
        // Поиск завода, которому принадлежит найденная установка
        var factory = FindFactory(factories, foundUnit);

        // Вывод информации о принадлежности "Резервуара 2" установке и заводу
        Console.WriteLine($"Резервуар 2 принадлежит установке {foundUnit.Name} и заводу {factory.Name} \n");

        // Получение общего объема всех резервуаров
        var totalVolume = GetTotalVolume(tanks);
        // Вывод общего объема резервуаров
        Console.WriteLine($"Общий объем резервуаров: {totalVolume} \n");

        // Вывод информации о каждом резервуаре с указанием установки и завода
        Console.WriteLine("Все резервуары:");
        foreach (var tank in tanks)
        {
            var unit = units.First(u => u.Id == tank.UnitId);
            var factory2 = factories.First(f => f.Id == unit.FactoryId);
            Console.WriteLine($"{tank.Name} ({unit.Name}, {factory2.Name})");
        }

        // Запрос ввода названия резервуара для поиска
        Console.Write("\nВведите название резервуара для поиска: ");
        string searchName = Console.ReadLine();
        // Поиск резервуара по введенному названию
        var foundTank = tanks.FirstOrDefault(t => t.Name.Contains(searchName));
        if (foundTank != null)
        {
            // Вывод информации о найденном резервуаре
            Console.WriteLine($"Найден резервуар: {foundTank.Name}");
        }
        else
        {
            // Вывод сообщения, если резервуар не найден
            Console.WriteLine("Резервуар не найден");
        }
    }

    // Метод для получения массива резервуаров
    public static Tank[] GetTanks()
    {
        // Возвращает массив объектов Tank, созданных с помощью new
        return new Tank[]
        {
            new Tank { Id = 1, Name = "Резервуар 1", Description = "Надземный - вертикальный", Volume = 1500, MaxVolume = 2000, UnitId = 1 },
            new Tank { Id = 2, Name = "Резервуар 2", Description = "Надземный - горизонтальный", Volume = 2500, MaxVolume = 3000, UnitId = 1 },
            new Tank { Id = 3, Name = "Дополнительный резервуар 24", Description = "Надземный - горизонтальный", Volume = 3000, MaxVolume = 3000, UnitId = 2 },
            new Tank { Id = 4, Name = "Резервуар 35", Description = "Надземный - вертикальный", Volume = 3000, MaxVolume = 3000, UnitId = 2 },
            new Tank { Id = 5, Name = "Резервуар 47", Description = "Подземный - двустенный", Volume = 4000, MaxVolume = 5000, UnitId = 2 },
            new Tank { Id = 6, Name = "Резервуар 256", Description = "Подводный", Volume = 500, MaxVolume = 500, UnitId = 3 }
        };
    }

    // Метод для получения массива установок
    public static Unit[] GetUnits()
    {
        // Возвращает массив объектов Unit, созданных с помощью new
        return new Unit[]
        {
            new Unit { Id = 1, Name = "ГФУ-2", Description = "Газофракционирующая установка", FactoryId = 1 },
            new Unit { Id = 2, Name = "ABT-6", Description = "Атмосферно-вакуумная трубчатка", FactoryId = 1 },
            new Unit { Id = 3, Name = "ABT-10", Description = "Атмосферно-вакуумная трубчатка", FactoryId = 2 }
        };
    }

    // Метод для получения массива заводов
    public static Factory[] GetFactories()
    {
        // Возвращает массив объектов Factory, созданных с помощью new
        return new Factory[]
        {
            new Factory { Id = 1, Name = "НПЗ#1", Description = "Первый нефтеперерабатывающий завод" },
            new Factory { Id = 2, Name = "НПЗ#2", Description = "Второй нефтеперерабатывающий завод" }
        };
    }

    // Метод для поиска установки по имени резервуара
    public static Unit FindUnit(Unit[] units, Tank[] tanks, string tankName)
    {
        // Перебираем все резервуары в массиве tanks
        foreach (var tank in tanks)
        {
            // Если имя текущего резервуара совпадает с заданным именем
            if (tank.Name == tankName)
            {
                // Перебираем все установки в массиве units
                foreach (var unit in units)
                {
                    // Если идентификатор текущей установки совпадает с UnitId найденного резервуара
                    if (unit.Id == tank.UnitId)
                    {
                        // Возвращаем найденную установку
                        return unit;
                    }
                }
            }
        }
        // Если установка не найдена, возвращаем null
        return null;
    }

    // Метод для поиска завода по установке
    public static Factory FindFactory(Factory[] factories, Unit unit)
    {
        // Перебираем все заводы в массиве factories
        foreach (var factory in factories)
        {
            // Если идентификатор текущего завода совпадает с FactoryId переданной установки
            if (factory.Id == unit.FactoryId)
            {
                // Возвращаем найденный завод
                return factory;
            }
        }
        // Если завод не найден, возвращаем null
        return null;
    }

    // Метод для получения общего объема всех резервуаров
    public static int GetTotalVolume(Tank[] tanks)
    {
        // Инициализируем переменную для хранения общего объема
        int totalVolume = 0;
        // Перебираем все резервуары в массиве tanks
        foreach (var tank in tanks)
        {
            // Добавляем объем текущего резервуара к общему объему
            totalVolume += tank.Volume;
        }
        // Возвращаем общий объем всех резервуаров
        return totalVolume;
    }

}

// Класс Unit, представляющий установку
public class Unit
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int FactoryId { get; set; }
}

// Класс Factory, представляющий завод
public class Factory
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

// Класс Tank, представляющий резервуар
public class Tank
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Volume { get; set; }
    public int MaxVolume { get; set; }
    public int UnitId { get; set; }
}
