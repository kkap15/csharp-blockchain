using System.ComponentModel.DataAnnotations;

namespace MiniChain.Web.Models;

public record TransactionRequest
{ 
    public required string From { get; set; }
    public required string To { get; set; }
    public required decimal Amount { get; set; }
    public required string Signature { get; set; }
}