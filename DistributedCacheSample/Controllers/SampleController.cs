using DistributedCacheSample.Models;
using Microsoft.AspNetCore.Mvc;

namespace DistributedCacheSample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SampleController : ControllerBase
    {
        private readonly ICacheProvider _cacheProvider;
        private readonly Repository _repo;
        public SampleController(ICacheProvider cacheProvider, Repository repo)
        {
            this._cacheProvider = cacheProvider;
            this._repo = repo;
        }

        //Also include "Cache Aside Pattern" implimentation
        [HttpGet("players")]
        public async Task<IActionResult> GetAllPlayersInTeam()
        {
            var allPlayers = await this._cacheProvider.GetAsync<List<Player>>("AllPlayersInTeam");

            if (allPlayers == null)
            {
                allPlayers = this._repo.Players;

                await this._cacheProvider.SetAsync("AllPlayersInTeam", allPlayers, config =>
                {
                    config.AbsoluteExpiration = 2;//dk
                    config.SlidingExpiration = 1;//dk
                });
            }

            return Ok(allPlayers);
        }

        [HttpPost("buy/player")]
        public async Task<IActionResult> AddPlayerToTeam(Player input)
        {
            var allPlayers = this._repo.Players;
            allPlayers.Add(input);

            await this._cacheProvider.RemoveAsync("AllPlayersInTeam");

            await this._cacheProvider.SetAsync("AllPlayersInTeam", allPlayers, config =>
            {
                config.AbsoluteExpiration = 2;
                config.SlidingExpiration = 1;
            });

            return Ok();
        }

        [HttpPut("sell/player/{id}")]
        public async Task<IActionResult> SellPlayerToAnotherTeam([FromRoute]int id)
        {
            var allPlayers = this._repo.Players;
            var player = allPlayers.SingleOrDefault(x => x.Id == id);
            if(player != null)
            {
                allPlayers.Remove(player);

                await this._cacheProvider.RemoveAsync("AllPlayersInTeam");

                await this._cacheProvider.SetAsync("AllPlayersInTeam", allPlayers, config =>
                {
                    config.AbsoluteExpiration = 2;
                    config.SlidingExpiration = 1;
                });
            }

            return Ok();
        }
    }
}
