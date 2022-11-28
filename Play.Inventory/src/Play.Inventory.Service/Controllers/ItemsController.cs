using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Controllers;

[ApiController]
[Route("items")]
public class ItemsController : ControllerBase
{
    private readonly IRepository<InventoryItem> _itemsRepository;
    private readonly CatalogClient _catalogClient;

    public ItemsController(IRepository<InventoryItem> itemsRepository, CatalogClient catalogClient)
    {
        _itemsRepository = itemsRepository;
        _catalogClient = catalogClient;
    }


    [HttpGet]
    public async Task<IActionResult> GetAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            return BadRequest();


        var catalogItems = await _catalogClient.GetCatalogItemsAsync();


        var inventoryItemsEntites = await _itemsRepository.GetAllAsync(x => x.UserId == userId);

        var inventoryItemDtos = inventoryItemsEntites.Select(inventoryItem =>
        {
            var catalogItem = catalogItems?.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId)
                    ?? throw new ArgumentException(nameof(catalogItems));
            return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
        });

        return Ok(inventoryItemDtos);
    }


    [HttpPost]
    public async Task<IActionResult> PostAsync(GrantItemsDto grantItem)
    {
        var inventoryItem = await _itemsRepository.GetAsync(item =>
        item.UserId == grantItem.UserId
        && item.CatalogItemId == grantItem.CatalogItemId);

        if (inventoryItem is null)
        {
            inventoryItem = new InventoryItem
            {
                CatalogItemId = grantItem.CatalogItemId,
                UserId = grantItem.UserId,
                Quantity = grantItem.Quantity,
                AquiredDate = DateTimeOffset.UtcNow
            };
            await _itemsRepository.CreateAsync(inventoryItem);
        }
        else
        {
            inventoryItem.Quantity += grantItem.Quantity;
            await _itemsRepository.UpdateAsync(inventoryItem);
        }


        return Ok();
    }
}