using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MindVault.Api.Models
{
    public class NoteLink
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid FromNoteId { get; set; }

        [Required]
        public Guid ToNoteId { get; set; }

        // optional: cached text of the link target title
        public string? ToNoteTitle { get; set; }

        [ForeignKey(nameof(FromNoteId))]
        public Note? FromNote { get; set; }

        [ForeignKey(nameof(ToNoteId))]
        public Note? ToNote { get; set; }
    }
}
