using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;
using Play.Common;

namespace Play.Catalog.Service.Controllers;

[ApiController]
[Route("items")]
public class ItemsController : ControllerBase
{
    private readonly IRepository<Item> _itemsRepository;

    public ItemsController(IRepository<Item> itemsRepository)
    {
        _itemsRepository = itemsRepository;
    }


    [HttpGet]
    public async Task<IEnumerable<ItemDto>> GetAsync()
    {
        return (await _itemsRepository.GetAllAsync()).Select(x => x.AsDto());
    }


    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var item = await _itemsRepository.GetAsync(id);

        if (item is null)
            return NotFound();

        return Ok(item.AsDto());
    }

    [HttpPost]
    public async Task<IActionResult> CreateItemAsync(CreateItemDto model)
    {
        var item = new Item()
        {
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            CreatedDate = DateTimeOffset.UtcNow
        };
        await _itemsRepository.CreateAsync(item);

        return CreatedAtAction(nameof(GetByIdAsync), new
        {
            Id = item.Id
        }, item.AsDto());
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateItemAsync(Guid id, UpdateItemDto model)
    {
        var item = await _itemsRepository.GetAsync(id);
        if (item is null)
            return NotFound();

        item.Name = model.Name;
        item.Description = model.Description;
        item.Price = model.Price;

        await _itemsRepository.UpdateAsync(item);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteItemAsync(Guid id)
    {
        var item = _itemsRepository.GetAsync(id);
        if (item is null)
            return NotFound();

        await _itemsRepository.DeleteAsync(id);

        return NoContent();
    }
}