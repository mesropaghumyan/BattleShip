using BattleShip.API.State;
using BattleShip.Models.Domain;

namespace BattleShip.Tests.Unit;

public class InMemoryGameStoreTests
{
    [Fact]
    public void Add_WithAlreadyRegisteredGameId_ThrowsInvalidOperationException()
    {
        var store = new InMemoryGameStore();
        var gameId = Guid.NewGuid();
        store.Add(gameId, new Game(new Grid(), new Grid()));

        Assert.Throws<InvalidOperationException>(() => store.Add(gameId, new Game(new Grid(), new Grid())));
    }
}
