// Действия игрока: шаг, использование предмета, покупка.
// Каждый метод возвращает сообщение, которое увидит игрок.

public static class Actions
{
    // Шаг на соседнюю клетку: dx и dy — на сколько меняются x и y (например, шаг вправо — dx = 1, dy = 0).
    public static string Move(Game game, int dx, int dy)
    {
        Player player = game.Player;
        int newX = player.X + dx;
        int newY = player.Y + dy;

        // ИСПРАВЛЕНО: проверяем стену по новым координатам (newX, newY)
        if (game.Map.IsWall(newX, newY))
            return "Там стена.";

        Monster monster = game.FindMonsterAt(newX, newY);
        if (monster != null)
            return Combat.PlayerAttacks(player, monster);

        bool isExit = game.Map.IsExit(newX, newY);
        if (isExit && !player.Bag.Has(Item.Key))
            return "Выход заперт. Нужен ключ.";

        player.MoveTo(newX, newY);
        string message = "Вы перешли на клетку (" + newX + ", " + newY + ").";

        FloorItem loot = game.FindLootAt(newX, newY);
        if (loot != null)
            message += " " + PickUp(player, loot);

        if (isExit)
        {
            game.Win();
            message += " Вы открыли дверь ключом и вышли из подземелья!";
        }

        return message;
    }

    // Игрок подбирает предмет с пола. Если в сумке нет места, предмет остаётся лежать.
    static string PickUp(Player player, FloorItem loot)
    {
        if (!player.Bag.Add(loot.Item))
            return "Здесь лежит: " + loot.Item.Name + ", но в сумке нет места.";

        loot.Take();
        return "Вы подобрали: " + loot.Item.Name + ".";
    }

    // Игрок использует предмет из сумки.
    public static string UseItem(Game game, int number)
    {
        Player player = game.Player;
        
        // Если нумерация предметов в игре начинается с 1, а в коллекции с 0, 
        // возможно, понадобится индекс: int index = number - 1;
        // Проверьте, как реализован ваш Inventory.Get(index).
        int index = number - 1; 

        Item item = player.Bag.Get(index);
        if (item == null)
            return "В сумке нет предмета с номером " + number + ".";

        if (item.Kind == Item.Potion)
        {
            player.Bag.RemoveAt(index);
            player.Heal(item.Power);
            return "Вы выпили: " + item.Name + ". Здоровье: " + player.Health + ".";
        }

        if (item.Kind == Item.Weapon)
        {
            Item old = player.Weapon;
            player.Bag.RemoveAt(index);
            player.Equip(item);
            if (old != null)
                player.Bag.Add(old);
            return "Вы взяли в руки: " + item.Name + ". Атака: " + player.Attack + ".";
        }

        return item.Name + ": этот предмет нельзя использовать.";
    }

    // Покупка товара номер number. Покупать можно, только стоя на клетке торговца.
    public static string Buy(Game game, int number)
    {
        Player player = game.Player;
        if (!game.Map.IsShop(player.X, player.Y))
            return "Торговца здесь нет. Он стоит на клетке $.";

        return game.Shop.Buy(player, number);
    }
}