namespace TubeMiniApp.API.Models;

// Модели для импорта данных из JSON файлов testData

public class NomenclatureImport
{
    public List<NomenclatureEl> ArrayOfNomenclatureEl { get; set; } = new();
}

public class NomenclatureEl
{
    public string ID { get; set; } = string.Empty;
    public string IDCat { get; set; } = string.Empty;
    public string IDType { get; set; } = string.Empty;
    public string IDTypeNew { get; set; } = string.Empty;
    public string ProductionType { get; set; } = string.Empty;
    public string IDFunctionType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Gost { get; set; } = string.Empty;
    public string FormOfLength { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string SteelGrade { get; set; } = string.Empty;
    public decimal Diameter { get; set; }
    public decimal ProfileSize2 { get; set; }
    public decimal PipeWallThickness { get; set; }
    public int Status { get; set; }
    public decimal Koef { get; set; } // Коэффициент для расчета веса
}

public class PricesImport
{
    public List<PricesEl> ArrayOfPricesEl { get; set; } = new();
}

public class PricesEl
{
    public string ID { get; set; } = string.Empty;
    public string IDStock { get; set; } = string.Empty;
    public decimal PriceT { get; set; } // Цена за тонну
    public decimal PriceLimitT1 { get; set; } // Порог для скидки 1 (тонны)
    public decimal PriceT1 { get; set; } // Цена со скидкой 1
    public decimal PriceLimitT2 { get; set; } // Порог для скидки 2 (тонны)
    public decimal PriceT2 { get; set; } // Цена со скидкой 2
    public decimal PriceM { get; set; } // Цена за метр
    public decimal PriceLimitM1 { get; set; } // Порог для скидки 1 (метры)
    public decimal PriceM1 { get; set; } // Цена со скидкой 1
    public decimal PriceLimitM2 { get; set; } // Порог для скидки 2 (метры)
    public decimal PriceM2 { get; set; } // Цена со скидкой 2
    public decimal NDS { get; set; } // НДС в процентах
}

public class RemnantsImport
{
    public List<RemnantsEl> ArrayOfRemnantsEl { get; set; } = new();
}

public class RemnantsEl
{
    public string ID { get; set; } = string.Empty;
    public string IDStock { get; set; } = string.Empty;
    public decimal InStockT { get; set; } // В наличии (тонны)
    public decimal InStockM { get; set; } // В наличии (метры)
    public decimal SoonArriveT { get; set; } // Скоро прибудет (тонны)
    public decimal SoonArriveM { get; set; } // Скоро прибудет (метры)
    public decimal ReservedT { get; set; } // Зарезервировано (тонны)
    public decimal ReservedM { get; set; } // Зарезервировано (метры)
    public bool UnderTheOrder { get; set; } // Под заказ
    public decimal AvgTubeLength { get; set; } // Средняя длина трубы
    public decimal AvgTubeWeight { get; set; } // Средний вес трубы
}

public class StocksImport
{
    public List<StockEl> ArrayOfStockEl { get; set; } = new();
}

public class StockEl
{
    public string IDStock { get; set; } = string.Empty;
    public string Stock { get; set; } = string.Empty;
    public string StockName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public string IDDivision { get; set; } = string.Empty;
    public bool CashPayment { get; set; }
    public bool CardPayment { get; set; }
    public string FIASId { get; set; } = string.Empty;
    public string OwnerInn { get; set; } = string.Empty;
    public string OwnerKpp { get; set; } = string.Empty;
    public string OwnerFullName { get; set; } = string.Empty;
    public string OwnerShortName { get; set; } = string.Empty;
    public string RailwayStation { get; set; } = string.Empty;
    public string ConsigneeCode { get; set; } = string.Empty;
}
