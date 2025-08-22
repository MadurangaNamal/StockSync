using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StockSync.ItemService.Entities;
using StockSync.ItemService.Infrastructure;
using StockSync.ItemService.Models;

namespace StockSync.ItemService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IItemServiceRepository _repository;
    private readonly IMapper _mapper;

    public ItemsController(IItemServiceRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemDto>>> GetItems()
    {
        var items = await _repository.GetAllItemsAsync();
        return Ok(_mapper.Map<IEnumerable<ItemDto>>(items));
    }

    [HttpGet("{id}", Name = "GetItem")]
    public async Task<ActionResult<ItemDto>> GetItem(string id)
    {
        var item = await _repository.GetItemByIdAsync(id);

        if (item == null)
            return NotFound($"Item with item id: {id} not found");

        return Ok(_mapper.Map<ItemDto>(item));
    }

    [HttpPost]
    public async Task<ActionResult<ItemDto>> CreateItem(ItemManipulationDto itemDto)
    {
        var item = _mapper.Map<Item>(itemDto);
        var createdItem = await _repository.CreateItemAsync(item);
        var itemToReturn = _mapper.Map<ItemDto>(createdItem);

        return CreatedAtRoute("GetItem", new { id = itemToReturn.Id }, itemToReturn);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ItemDto>> UpdateItem(string id, ItemManipulationDto itemDto)
    {
        if (!await _repository.ItemExistsAsync(id))
            return NotFound($"Item with item id: {id} not found");

        var item = _mapper.Map<Item>(itemDto);
        var updatedItem = await _repository.UpdateItemAsync(id, item);
        return Ok(_mapper.Map<ItemDto>(updatedItem));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteItem(string id)
    {
        if (await _repository.DeleteItemAsync(id))
            return NoContent();

        return NotFound($"Item with item id: {id} not found");
    }
}
