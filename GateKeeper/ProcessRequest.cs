namespace GateKeeper;

internal class ProcessRequest
{
    public required string Action { get; set; } 
    public required string Username { get; set; }
    public required string Password { get; set; }

}