using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EvoConnect.Server.DTOs;
using EvoConnect.Server.Models;
using EvoConnect.Server.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EvoConnect.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VipPatientsController(IPatientsDA patientsDA) : ControllerBase
    {
        private readonly IPatientsDA _patientsDA = patientsDA;

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<VipPatientDto>>> GetVipPatients(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string status = null,
            [FromQuery] decimal? minRevenue = null,
            [FromQuery] decimal? maxRevenue = null,
            [FromQuery] string sortBy = "priority",
            [FromQuery] string searchQuery = null)
        {
            try
            {
                var result = await _patientsDA.GetVipPatientsPaginated(
                    pageNumber,
                    pageSize,
                    status,
                    minRevenue,
                    maxRevenue,
                    sortBy,
                    searchQuery);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error retrieving VIP patients",
                    error = ex.Message
                });
            }
        }



    }
}