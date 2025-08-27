using AutoMapper;
using Common.Application;
using Microsoft.EntityFrameworkCore;
using TicketModule.Core.DTOs.Tickets;
using TicketModule.Data;
using TicketModule.Data.Entities;

namespace TicketModule.Core.Services;

public interface ITicketService
{
    Task<OperationResult<Guid>> CreateTicket(CreateTicketCommand command);
    Task<OperationResult> SendMessageInTicket(SendTicketMessageCommand command);
    Task<OperationResult> CloseTicket(Guid ticketId);

    Task<TicketDto> GetTicket(Guid ticketId);
}

class TicketService : ITicketService
{
    private readonly TicketContext _context;
    private readonly IMapper _mapper;

    public TicketService(TicketContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<OperationResult<Guid>> CreateTicket(CreateTicketCommand command)
    {
        var ticket = _mapper.Map<Ticket>(command);
        await _context.Tickets.AddAsync(ticket);
        await _context.SaveChangesAsync();
        return OperationResult<Guid>.Success(ticket.Id);
    }

    public async Task<OperationResult> SendMessageInTicket(SendTicketMessageCommand command)
    {
        var ticket = await _context.Tickets.FirstOrDefaultAsync(x => x.Id == command.TicketId);
        if (ticket is null)
            return OperationResult.NotFound();

        var message = new TicketMessage()
        {
            Text = command.Text,
            TicketId = command.TicketId,
            UserId = command.UserId,
            UserFullName = command.OwnerFullName
        };
        if (ticket.UserId == command.UserId)
            ticket.Status = TicketStatus.Pending;
        else
            ticket.Status = TicketStatus.Answered;

        await _context.TicketMessages.AddAsync(message);
        _context.TicketMessages.Update(message);
        await _context.SaveChangesAsync();
        return OperationResult.Success();
    }

    public async Task<OperationResult> CloseTicket(Guid ticketId)
    {
        var ticket = await _context.Tickets.FirstOrDefaultAsync(x => x.Id == ticketId);
        if (ticket is null)
            return OperationResult.NotFound();

        ticket.Status = TicketStatus.Closed;
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync();
        return OperationResult.Success();
    }

    public async Task<TicketDto> GetTicket(Guid ticketId)
    {
        var ticket = await _context.Tickets
            .Include(x => x.TicketMessages)
            .FirstOrDefaultAsync(x => x.Id == ticketId);

        return _mapper.Map<TicketDto>(ticket);
    }
}