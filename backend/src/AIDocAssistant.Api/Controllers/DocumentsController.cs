using Microsoft.AspNetCore.Mvc;

namespace AIDocAssistant.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    // TODO: inject Application layer service (via IDocumentRepository/handler),
    // scope every call by WorkspaceId per AGENTS.md rules.

    [HttpGet]
    public IActionResult List()
    {
        return Ok(new { message = "TODO: return workspace-scoped document list" });
    }
}
