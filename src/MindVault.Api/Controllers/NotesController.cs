
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindVault.Api.Data;
using MindVault.Api.Models;
using System.Security.Claims;


namespace MindVault.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public NotesController(ApplicationDbContext db) => _db = db;

        private string CurrentUserId => User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        public async Task<IActionResult> GetNotes()
        {
            var notes = await _db.Notes.Where(n => n.UserId == CurrentUserId).ToListAsync();
            return Ok(notes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNote(Guid id)
        {
            var note = await _db.Notes.FirstOrDefaultAsync(n => n.Id == id && n.UserId == CurrentUserId);
            if (note == null) return NotFound();
            return Ok(note);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Note note)
        {
            note.UserId = CurrentUserId;
            note.CreatedAt = DateTime.UtcNow;
            note.UpdatedAt = DateTime.UtcNow;
            _db.Notes.Add(note);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetNote), new { id = note.Id }, note);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Note dto)
        {
            var note = await _db.Notes.FirstOrDefaultAsync(n => n.Id == id && n.UserId == CurrentUserId);
            if (note == null) return NotFound();
            note.Title = dto.Title;
            note.Body = dto.Body;
            note.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var note = await _db.Notes.FirstOrDefaultAsync(n => n.Id == id && n.UserId == CurrentUserId);
            if (note == null) return NotFound();
            _db.Notes.Remove(note);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{id}/links")]
        public async Task<IActionResult> GetLinks(Guid id)
        {
            var outgoing = await _db.NoteLinks.Where(nl => nl.FromNoteId == id).ToListAsync();
            var incoming = await _db.NoteLinks.Where(nl => nl.ToNoteId == id).ToListAsync();
            return Ok(new { outgoing, incoming });
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return BadRequest();
            var ids = _searchService.Search(q);
            var notes = await _db.Notes.Where(n => ids.Contains(n.Id)).ToListAsync();
            return Ok(notes);
        }

    }
}
