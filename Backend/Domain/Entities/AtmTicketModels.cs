using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KtcWeb.Domain.Entities
{
    [Table("TroubleTickets")]
    public class TroubleTicket
    {
        [Key]
        [Column("TroubleTicket_id")]
        public int TroubleTicketId { get; set; }

        [Column("client_id")]
        public int ClientId { get; set; }

        [Column("tickettype_id")]
        public int TicketTypeId { get; set; }

        [Column("ticketstatus_id")]
        public byte TicketStatusId { get; set; }

        [Column("creationtime")]
        public DateTime? CreationTime { get; set; }

        [Column("ownedtime")]
        public DateTime? Opened { get; set; }

        [Column("closedtime")]
        public DateTime? Closed { get; set; }

        [Column("owner_id")]
        public int? OwnerId { get; set; }

        [Column("dispatch_id")]
        public int? DispatchId { get; set; }

        [Column("updatetime")]
        public DateTime? LastChangeDate { get; set; }

        [Column("sla_open_duration")]
        public int? SlaOpenDuration { get; set; }

        [Column("comments")]
        public string Comments { get; set; } = string.Empty;
    }

    [Table("TroubleTicketTypes")]
    public class TroubleTicketType
    {
        [Key]
        [Column("tickettype_id")]
        public int TicketTypeId { get; set; }

        [Column("typename")]
        public string Name { get; set; } = string.Empty;

        [Column("errorcodetype_id")]
        public int ErrorCodeId { get; set; }
    }

    [Table("ErrorCodeTypes")]
    public class ErrorCodeType
    {
        [Key]
        [Column("errorcodetype_id")]
        public int ErrorCodeId { get; set; }

        [Column("errorcode")]
        public string ErrorCode { get; set; } = string.Empty;

        [Column("errordescription")]
        public string Code { get; set; } = string.Empty;

        [Column("errortext")]
        public string ErrorText { get; set; } = string.Empty;
    }

    [Table("KTCUsers")]
    public class KtcUser
    {
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("username")]
        public string Name { get; set; } = string.Empty;
    }

    [Table("DispatchList")]
    public class DispatchList
    {
        [Key]
        [Column("dispatch_id")]
        public int DispatchId { get; set; }

        [Column("dispatchname")]
        public string DispatchName { get; set; } = string.Empty;
    }
}
