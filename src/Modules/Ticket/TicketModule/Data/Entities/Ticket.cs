using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Common.Domain;

namespace TicketModule.Data.Entities;

internal class Ticket : BaseEntity
{
    public Guid UserId { get; set; }
    [MaxLength(100)]
    public string OwnerFullName { get; set; }
    [MaxLength(11)]
    public string PhoneNumber { get; set; }
    [MaxLength(100)]
    public string Title { get; set; }
    public string Text { get; set; }
    public TicketStatus Status { get; set; }
    public List<TicketMessage> TicketMessages { get; set; }

}

public enum TicketStatus
{
    Pending,
    Answered,
    Closed
}

internal class TicketMessage
{
    public Guid UserId { get; set; }
    public Guid TicketId { get; set; }
    [MaxLength(100)]
    public string UserFullName { get; set; }
    public string Text { get; set; }
    public Ticket Ticket { get; set; }

}