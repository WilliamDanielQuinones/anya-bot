namespace App;

public class KarutaCard
{
    [CsvHelper.Configuration.Attributes.Name("code")]
    public string Code { get; set; }

    [CsvHelper.Configuration.Attributes.Name("number")]
    public string Number { get; set; }

    [CsvHelper.Configuration.Attributes.Name("edition")]
    public string Edition { get; set; }

    [CsvHelper.Configuration.Attributes.Name("character")]
    public string Character { get; set; }

    [CsvHelper.Configuration.Attributes.Name("series")]
    public string Series { get; set; }

    [CsvHelper.Configuration.Attributes.Name("quality")]
    public string Quality { get; set; }

    [CsvHelper.Configuration.Attributes.Name("tag")]
    public string Tag { get; set; }

    [CsvHelper.Configuration.Attributes.Name("wishlists")]
    public string Wishlists { get; set; }
}

public class CasinoCard
{
    public CasinoCard(KarutaCard card)
    {
        Code = card.Code;
        Character = $"{card.Character}{(int.Parse(card.Wishlists) > 999 || int.Parse(card.Number) < 100 ? " :moneybag:" : "")}";
        Series = card.Series;
    }
    
    public string Code { get; set; }
    public string Character { get; set; }
    public string Series { get; set; }

    public override string ToString()
    {
        return $"{Code} - {Series} - {Character}";
    }
}
