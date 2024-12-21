namespace Personal.Domain.Entities;

public class ReadPage
{
    public DateTime Date { set; get; } = DateTime.Today;
    public int Pages { set; get; } = 1;
    public string? Note { set; get; }
    public string? ExtNote { set; get; }

}
