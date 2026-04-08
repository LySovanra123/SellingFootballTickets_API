using System.Collections;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SellingFootballTickets_API.Data;
using SellingFootballTickets_API.DTO;
using SellingFootballTickets_API.Models;

namespace SellingFootballTickets_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ServiceContext _context;

        public PaymentController(ServiceContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPayments()
        {
            var payments = await _context.payments.ToListAsync();
            return Ok(payments);
        }
        //===============================================
        //              Get Payment on monthly basis
        //===============================================
        [HttpPost("/api/GetMonthlyPayments")]
        public async Task<ActionResult<IEnumerable<Payment>>> GetMonthlyPayments([FromBody] MethodPayment request)
        {
            var payments = await _context.payments
                .Where(p => p.PaymentMethod == request.PaymentMethod)
                .ToListAsync();
            return Ok(payments);
        }
    }
}
