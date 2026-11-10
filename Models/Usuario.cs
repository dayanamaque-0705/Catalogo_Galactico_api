namespace Models;

public sealed class Usuario
{
    public int Id { get; set; }
    public required string Correo { get; set; }
    public required string PasswordHash { get; set; }
    public DateTimeOffset CreadoEn { get; set; }
}
