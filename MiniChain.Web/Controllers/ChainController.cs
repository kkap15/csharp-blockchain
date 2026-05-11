using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniChain.Core.Services;
using MiniChain.Web.Models;
using MiniChain.Web.Services;

namespace MiniChain.Web.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api")]
[ApiController]
public class ChainController(ChainService chainService) : ControllerBase
{
    [HttpGet("blocks")]
    public IActionResult GetBlock()
    {
        return Ok(chainService.Blockchain.Blocks);
    }

    [HttpGet("block/{index:int}")]
    public IActionResult GetBlock(int index)
    {
        if (index < 0 || index >= chainService.Blockchain.Blocks.Count)
        {
            return NotFound();
        }

        return Ok(chainService.Blockchain.Blocks[index]);
    }

    [HttpGet("mempool")]
    public IActionResult GetMempool()
    {
        return Ok(chainService.Mempool.Pending);
    }

    [HttpGet("balance/{publicKey}")]
    public IActionResult GetBalance(string publicKey)
    {
        if (string.IsNullOrWhiteSpace(publicKey)) return BadRequest();

        return Ok(chainService.Blockchain.GetBalance(publicKey));
    }

    [HttpPost("transactions")]
    public IActionResult SubmitTransaction([FromBody] TransactionRequest request)
    {
        var isSuccess = (chainService.Mempool.Submit(
            new Transaction(request.From, request.To, request.Amount, false, request.Signature),
            chainService.Blockchain));

        if (!isSuccess) return BadRequest(new { message = "Transaction failed" });
        
        return Ok(new { message = "Transaction submitted" });
    }

    [HttpPost("mine")]
    public async Task<IActionResult> Mine([FromBody] MineRequest request)
    {
        try
        {
            await chainService.MineBlockAsync(request.MinerAddress);
            return Ok(new { message = $"Block Mined. New Tip: {chainService.Blockchain.Tip}" });
        }
        catch
        {
            return BadRequest(new { message = "Failed to mine block" });
        }
    }
}