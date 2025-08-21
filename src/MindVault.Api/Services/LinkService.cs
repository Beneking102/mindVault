using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using MindVault.Api.Data;
using MindVault.Api.Models;

namespace MindVault.Api.Services
{
    public class LinkService
    {
        private readonly ApplicationDbContext _db;
        private static readonly Regex LinkRegex = new(@"\[\[([^\]]+)\]\]", RegexOptions.Compiled);

        public LinkService(ApplicationDbContext db) => _db = db;

        // Call after note saved/updated
        public async Task UpdateLinksForNoteAsync(Note note)
        {
            // find all [[...]] matches
            var matches = LinkRegex.Matches(note.Body ?? string.Empty);
            var titles = matches.Select(m => m.Groups[1].Value.Trim()).Where(s => !string.IsNullOrEmpty(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            // Remove existing links outgoing from this note
            var existing = _db.NoteLinks.Where(nl => nl.FromNoteId == note.Id);
            _db.NoteLinks.RemoveRange(existing);

            // For each found title, try to find target note by title (simple exact match)
            foreach (var title in titles)
            {
                var target = await _db.Notes.FirstOrDefaultAsync(n => n.UserId == note.UserId && n.Title == title);
                if (target != null)
                {
                    _db.NoteLinks.Add(new NoteLink
                    {
                        FromNoteId = note.Id,
                        ToNoteId = target.Id,
                        ToNoteTitle = target.Title
                    });
                }
            }

            await _db.SaveChangesAsync();
        }
    }
}
