using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockSync.Shared;
using StockSync.SupplierService.Entities;
using StockSync.SupplierService.Infrastructure;
using StockSync.SupplierService.Models;

namespace StockSync.SupplierService.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierServiceRepository _repository;
    private readonly IMapper _mapper;

    public SuppliersController(ISupplierServiceRepository repository, IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierDto>>> GetAllSuppliers()
    {
        var suppliers = await _repository.GetSuppliersAsync() ?? [];
        return Ok(_mapper.Map<IEnumerable<SupplierDto>>(suppliers));
    }

    [HttpGet("{id}", Name = "GetSupplierById")]
    public async Task<ActionResult<SupplierDto>> GetSupplierById(string id)
    {
        var supplier = await _repository.GetSupplierAsync(int.Parse(id));

        if (supplier == null)
            return NotFound($"Supplier with id: {id} not found");

        return Ok(_mapper.Map<SupplierDto>(supplier));
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpPost]
    public async Task<ActionResult<SupplierDto>> AddNewSupplier(SupplierManipulationDto supplierCreationDto)
    {
        var supplier = _mapper.Map<Supplier>(supplierCreationDto);
        await _repository.AddSupplierAsync(supplier);

        var supplierToReturn = _mapper.Map<SupplierDto>(supplier);
        return CreatedAtRoute("GetSupplierById", new { id = supplierToReturn.SupplierId }, supplierToReturn);
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpPut("{id}")]
    public async Task<ActionResult<SupplierDto>> UpdateSupplier(string id, SupplierManipulationDto supplierUpdateDto)
    {
        if (!await _repository.SupplierExistsAsync(int.Parse(id)))
            return NotFound($"Supplier with id: {id} not found");

        var supplier = await _repository.GetSupplierAsync(int.Parse(id));
        _mapper.Map(supplierUpdateDto, supplier);

        await _repository.UpdateSupplier(supplier!);
        var supplierToReturn = _mapper.Map<SupplierDto>(supplier);
        return Ok(supplierToReturn);
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteSupplier(string id)
    {
        var supplier = await _repository.GetSupplierAsync(int.Parse(id));
        if (supplier == null)
            return NotFound($"Supplier with id: {id} not found");

        await _repository.DeleteSupplier(supplier);
        return NoContent();
    }
}
