using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockSync.ItemService.Entities;
using StockSync.ItemService.Infrastructure;
using StockSync.ItemService.Models;
using StockSync.Shared.Models;

namespace StockSync.ItemService.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IItemServiceRepository _repository;
    private readonly IMapper _mapper;

    public ItemsController(IItemServiceRepository repository, IMapper mapper)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ItemDto>>> GetItems(
        [FromQuery] PaginationParams paginationParams,
        [FromQuery] string? itemIds = null)
    {
        var pagedItems = await _repository.GetItemsAsync(itemIds, paginationParams);
        var itemsResponse = _mapper.Map<IEnumerable<ItemDto>>(pagedItems.Items);

        return Ok(PagedResult<ItemDto>.Create(itemsResponse, pagedItems.PageNumber, pagedItems.PageSize, pagedItems.TotalCount));
    }

    [HttpGet("{id}", Name = "GetItem")]
    public async Task<ActionResult<ItemDto>> GetItem(string id)
    {
        var item = await _repository.GetItemByIdAsync(id);

        if (item == null)
            return NotFound($"Item with item id: {id} not found");

        var itemResponse = _mapper.Map<ItemDto>(item);

        return Ok(itemResponse);
    }

    [HttpPost]
    public async Task<IActionResult> CreateItem(ItemManipulationDto itemDto)
    {
        if (itemDto == null)
            return BadRequest($"Invalid item data");

        var item = _mapper.Map<Item>(itemDto);
        var createdItem = await _repository.CreateItemAsync(item);
        var itemToReturn = _mapper.Map<ItemDto>(createdItem);

        return CreatedAtRoute("GetItem", new { id = itemToReturn.Id }, itemToReturn);
    }

    [Authorize(Policy = "RequireAdminOrUser")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ItemDto>> UpdateItem(string id, ItemManipulationDto itemDto)
    {
        if (!await _repository.ItemExistsAsync(id))
            return NotFound($"Item with item id: {id} not found");

        if (itemDto == null)
            return BadRequest($"Invalid item data");

        var item = _mapper.Map<Item>(itemDto);
        var updatedItem = await _repository.UpdateItemAsync(id, item);
        var itemResponse = _mapper.Map<ItemDto>(updatedItem);

        return Ok(itemResponse);
    }

    [Authorize(Policy = "RequireAdministratorRole")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItem(string id)
    {
        if (await _repository.DeleteItemAsync(id))
            return NoContent();

        return NotFound($"Item with item id: {id} not found");
    }
}
