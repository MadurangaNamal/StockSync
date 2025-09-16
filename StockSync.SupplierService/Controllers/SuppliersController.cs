using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockSync.Shared.Models;
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
    private readonly ICacheService _cacheService;

    public SuppliersController(ISupplierServiceRepository repository, IMapper mapper, ICacheService cacheService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierDto>>> GetAllSuppliers()
    {
        var allSuppliers = await _repository.GetSuppliersAsync() ?? [];
        var supplierDtos = new List<SupplierDto>();

        foreach (var supplier in allSuppliers)
        {
            var itemDtos = (supplier.Items ?? new List<string>())
                .Select(id => _cacheService.GetItemDto(id))
                .Where(item => item != null)
                .ToList();

            var dto = new SupplierDto(
                supplier.SupplierId,
                supplier.Name,
                supplier.ContactEmail,
                supplier.ContactPhone,
                supplier.Address,
                supplier.City,
                supplier.State,
                supplier.ZipCode,
                supplier.Country);

            dto.Items = itemDtos!;
            supplierDtos.Add(dto);
        }

        return Ok(supplierDtos);
    }

    [HttpGet("{id}", Name = "GetSupplierById")]
    public async Task<ActionResult<SupplierDto>> GetSupplierById(string id)
    {
        if (!int.TryParse(id, out int supplierId))
        {
            return BadRequest($"Invalid supplier id format: {id}");
        }

        var supplier = await _repository.GetSupplierAsync(supplierId);

        if (supplier == null)
            return NotFound($"Supplier with id: {id} not found");

        var supplierDto = _mapper.Map<SupplierDto>(supplier);

        if (supplier.Items?.Any() == true)
        {
            var itemDtos = supplier.Items
                .Select(itemId => _cacheService.GetItemDto(itemId))
                .Where(item => item != null)
                .ToList();

            supplierDto.Items = itemDtos!;
        }

        return Ok(supplierDto);
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
