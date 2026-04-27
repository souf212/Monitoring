using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace KtcWeb.Application.DTOs
{
    public class AtmTicketDto
    {
        public long TicketId { get; set; }
        public string TicketType { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public DateTime? Created { get; set; }
        public DateTime? Opened { get; set; }
        public DateTime? Closed { get; set; }

        [NotMapped]
        public string Duration { get; set; } = string.Empty;
        public int TicketStatusId { get; set; }

        [NotMapped]
        public string Status { get; set; } = string.Empty;
        public string ErrorId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string ErrorText { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;

        [NotMapped]
        public string LastChangeBy { get; set; } = string.Empty;
        public DateTime? LastChangeDate { get; set; }

        [NotMapped]
        public string LastComment { get; set; } = string.Empty;
        public string SlaSummary { get; set; } = string.Empty;
        public string DispatchedTo { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
    }
}
