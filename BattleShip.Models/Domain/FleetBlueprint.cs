namespace BattleShip.Models.Domain;

public static class FleetBlueprint
{
    public static readonly IReadOnlyList<(string Name, int Size)> Standard =
    [
        ("Porte-avions", 5),
        ("Croiseur", 4),
        ("Contre-torpilleur", 3),
        ("Sous-marin", 3),
        ("Torpilleur", 2)
    ];
}
