using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public interface IAbility
{
    void Execute(Character target);
}

public abstract class Character
{
    private int _currentHealth;
    private int _currentMana;
    private int _bleedTurns;
    
    // damage bleed per turn dari ghoul
    private const int BleedDamagePerTurn = 10;

    public string Name { get; protected set; }
    public int MaxHealth { get; protected set; }
    public int MaxMana { get; protected set; }
    public int Damage { get; protected set; }
    public int Level { get; protected set; }

    public int CurrentHealth
    {
        get { return _currentHealth; }
        protected set { _currentHealth = value; }
    }

    public int CurrentMana
    {
        get { return _currentMana; }
        protected set { _currentMana = value; }
    }

    public bool IsAlive => CurrentHealth > 0;
    public bool IsBleeding => _bleedTurns > 0;

    public Character(string name, int maxHealth, int damage, int maxMana)
    {
        Name = name;
        MaxHealth = maxHealth;
        _currentHealth = maxHealth;
        Damage = damage;
        MaxMana = maxMana;
        _currentMana = maxMana;
        Level = 1;
    }

    public virtual void TakeDamage(int damage)
    {
        if (!IsAlive) return;

        _currentHealth -= damage;

        if (_currentHealth < 0)
        {
            _currentHealth = 0;
        }

        Console.WriteLine($"\n{Name} takes {damage} damage! Remaining Health: {_currentHealth}/{MaxHealth}");

        if (!IsAlive)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n*** {Name} has been defeated! ***");
            Console.ResetColor();
        }
    }

    public void Heal(int amount)
    {
        if (!IsAlive) return;

        _currentHealth += amount;

        if (_currentHealth > MaxHealth)
        {
            _currentHealth = MaxHealth;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"{Name} heals for {amount} HP.");
        Console.ResetColor();
    }

    public bool ConsumeMana(int amount)
    {
        if (_currentMana >= amount)
        {
            _currentMana -= amount;
            return true;
        }
        return false;
    }

    public void RegenerateMana(int amount)
    {
        if (!IsAlive) return;

        _currentMana += amount;
        if (_currentMana > MaxMana)
        {
            _currentMana = MaxMana;
        }
    }

    public void ApplyBleed(int turns)
    {
        _bleedTurns = turns;
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine($"{Name} is bleeding! Takes damage for next {turns} turns.");
        Console.ResetColor();
    }

    public void ProcessStatusEffects()
    {
        if (_bleedTurns > 0 && IsAlive)
        {
            _bleedTurns--;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"\n[BLEED] {Name} loses blood! (-{BleedDamagePerTurn} HP)");
            Console.ResetColor();
            TakeDamage(BleedDamagePerTurn);
        }
    }

    public void RemoveStatusEffects()
    {
        _bleedTurns = 0;
    }

    public virtual void Attack(Character target)
    {
        Console.WriteLine($"{Name} attacks {target.Name}!");
        target.TakeDamage(Damage);
    }

    public abstract void UseSpecialAbility(Character target);
}

public class Paladin : Character, IAbility
{
    // pengali damage skill
    private const int HolySmiteDamageMultiplier = 2;
    // jumlah heal saat pakai skill
    private const int SelfHealAmount = 10;
    // tambahan hp saat naik level
    private const int LevelUpHpBonus = 20;
    // tambahan damage saat naik level
    private const int LevelUpDmgBonus = 3;
    // tambahan mana saat naik level
    private const int LevelUpManaBonus = 5;
    // persentase tambahan damage forge weapon
    private const double ForgeDamagePercent = 0.25;
    // mana dasar player
    private const int PlayerBaseMana = 20; 

    private int _originalDamage;

    public Paladin(string name) : base(name, 150, 15, PlayerBaseMana) 
    {
        _originalDamage = Damage;
    }

    public override void UseSpecialAbility(Character target)
    {
        Execute(target);
    }

    public void Execute(Character target)
    {
        int holyDamage = Damage * HolySmiteDamageMultiplier;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"{Name} casts Holy Smite on {target.Name}!");
        Console.ResetColor();
        
        target.TakeDamage(holyDamage);
        Heal(SelfHealAmount);
    }

    public void ForgeWeapon()
    {
        int bonus = (int)(Damage * ForgeDamagePercent);
        Damage += bonus;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"{Name} sharpens their blade! Damage increased by {bonus} for this match.");
        Console.ResetColor();
    }

    public void ResetBattleStats()
    {
        Damage = _originalDamage;
        RemoveStatusEffects();
    }

    public void GainLevel()
    {
        Level++;
        MaxHealth += LevelUpHpBonus;
        _originalDamage += LevelUpDmgBonus;
        MaxMana += LevelUpManaBonus;
        
        CurrentHealth = MaxHealth; 
        CurrentMana = MaxMana;
        Damage = _originalDamage;

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n*** LEVEL UP! {Name} has reached Level {Level}! ***");
        Console.WriteLine($"Max HP +{LevelUpHpBonus}, Max Mana +{LevelUpManaBonus}, Base Dmg +{LevelUpDmgBonus}.");
        Console.ResetColor();
    }
}

public class Goblin : Character
{
    // bonus damage saat crit
    private const int CriticalHitBonus = 10;

    public Goblin(string name) : base(name, 60, 10, 10) { }

    public override void UseSpecialAbility(Character target)
    {
        int totalDamage = Damage + CriticalHitBonus;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"{Name} performs a Sneak Attack on {target.Name}!");
        Console.ResetColor();
        
        target.TakeDamage(totalDamage);
    }
}

public class Orc : Character
{
    // pengali damage berserk
    private const int BerserkDamageMultiplier = 3;
    // damage balik ke diri sendiri
    private const int RecoilDamage = 5;

    public Orc(string name) : base(name, 100, 12, 12) { }

    public override void UseSpecialAbility(Character target)
    {
        int massiveDamage = Damage * BerserkDamageMultiplier;
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine($"{Name} goes Berserk and smashes {target.Name}!");
        Console.ResetColor();

        target.TakeDamage(massiveDamage);
        
        Console.WriteLine($"{Name} takes recoil damage from the exertion.");
        TakeDamage(RecoilDamage);
    }
}

public class Ghoul : Character
{
    // durasi efek bleed
    private const int BleedDuration = 3;

    public Ghoul(string name) : base(name, 140, 15, 15) { }

    public override void UseSpecialAbility(Character target)
    {
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine($"{Name} uses Feral Sweep on {target.Name}!");
        Console.ResetColor();

        target.TakeDamage(Damage);
        target.ApplyBleed(BleedDuration);
    }
}

public class Dragon : Character
{
    // pengali damage naga
    private const int WrathMultiplier = 3;

    public Dragon(string name) : base(name, 200, 20, 20) { }

    public override void UseSpecialAbility(Character target)
    {
        int wrathDamage = Damage * WrathMultiplier;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{Name} unleashes Dragon's Wrath upon {target.Name}!");
        Console.ResetColor();

        target.TakeDamage(wrathDamage);
    }
}

public class GameManager
{
    // jumlah healing dari potion
    private const int PlayerHealPotionAmount = 25;
    // peluang musuh pakai skill
    private const int AiSpecialAttackChance = 3; 
    // biaya mana skill
    private const int AbilityManaCost = 10;
    // jumlah regen mana per turn
    private const int ManaRegenPerTurn = 5;
    
    // batas hp tinggi untuk warna hijau
    private const int HighHealthThreshold = 70;
    // batas hp sedang untuk warna kuning
    private const int MidHealthThreshold = 36;

    // delay aksi combat
    private const int ActionDelay = 1000;
    // delay tampilan musuh
    private const int EnemyDisplayDelay = 2000;

    // peluang spawn naga
    private const int DragonSpawnChance = 5;
    // peluang spawn ghoul
    private const int GhoulSpawnChance = 25;
    // peluang spawn orc
    private const int OrcSpawnChance = 55;

    private readonly Random _rng = new Random();

    private Paladin _player; 
    private List<Character> _enemies;
    private int _matchCount;

    public void StartGame()
    {
        Console.Clear();
        Console.WriteLine("==============================================");
        Console.WriteLine("           C# CONSOLE RPG COMBAT SIM          ");
        Console.WriteLine("           MADE BY: 2702279243 Jason          ");
        Console.WriteLine("==============================================");
        
        Console.Write("\nInput your username: ");
        Console.Write("\n>> ");
        string playerName = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = "Hero";
        }

        _player = new Paladin(playerName);
        _matchCount = 1;

        while (_player.IsAlive)
        {
            InitializeEnemies();
            
            Console.Clear();
            Console.WriteLine($"\n====== MATCH {_matchCount} START ======");
            Console.WriteLine($"A group of enemies approaches {_player.Name}!");
            Pause();
            Console.Clear();

            CombatLoop();

            if (!_player.IsAlive) break;

            Console.Clear();
            Console.WriteLine("==============================================");
            Console.WriteLine("                MATCH COMPLETE                ");
            Console.WriteLine("==============================================");
            
            _player.GainLevel();
            _player.ResetBattleStats(); 
            Pause();

            bool continueGame = PostMatchMenu();
            if (!continueGame) break;

            _matchCount++;
        }

        EndGame();
    }

    private void InitializeEnemies()
    {
        _enemies = new List<Character>();

        if (_matchCount == 1)
        {
            _enemies.Add(new Goblin("Goblin Scavenger"));
        }
        else
        {
            int enemyCount = _rng.Next(1, 4); 
            
            for (int i = 0; i < enemyCount; i++)
            {
                int roll = _rng.Next(1, 101);

                if (roll <= DragonSpawnChance)
                {
                    _enemies.Add(new Dragon($"Dragon the Might {i + 1}"));
                }
                else if (roll <= GhoulSpawnChance)
                {
                    _enemies.Add(new Ghoul($"Ghoul the Undead {i + 1}"));
                }
                else if (roll <= OrcSpawnChance)
                {
                    _enemies.Add(new Orc($"Orc the Sharp {i + 1}"));
                }
                else
                {
                    _enemies.Add(new Goblin($"Goblin the Sly {i + 1}"));
                }
            }
        }
    }

    private void CombatLoop()
    {
        while (_player.IsAlive && _enemies.Any(e => e.IsAlive))
        {
            DisplayStatus();
            
            _player.ProcessStatusEffects();
            
            foreach (var enemy in _enemies.Where(e => e.IsAlive).ToList())
            {
                enemy.ProcessStatusEffects();
                if (!_player.IsAlive) break;
            }

            if (!_player.IsAlive) break;

            bool actionTaken = PlayerTurn();
            
            if (actionTaken)
            {
                Pause();
                Console.Clear();
                CleanUpEnemies();

                if (!_enemies.Any(e => e.IsAlive)) break; 

                EnemyTurn();
                Pause();
                Console.Clear();
            }
        }
    }

    private bool PostMatchMenu()
    {
        Console.Clear();
        Console.WriteLine("==============================================");
        Console.WriteLine("              PREPARE FOR BATTLE              ");
        Console.WriteLine("==============================================");
        Console.WriteLine("1: Find a new match!");
        Console.WriteLine("2: Exit Game");
        Console.WriteLine("==============================================");
        Console.Write(">> ");
        
        string choice = Console.ReadLine();
        return choice == "1";
    }

    private void DisplayStatus()
    {
        Console.WriteLine("\n----- MATCH STATUS ----------------------------");
        Console.Write($"(Level {_player.Level}) ");
        PrintHealthEntry(_player.Name, _player, false);

        Console.WriteLine("----- ENEMIES ---------------------------------");
        for (int i = 0; i < _enemies.Count; i++)
        {
            var enemy = _enemies[i];
            if (enemy.IsAlive)
            {
                PrintHealthEntry(enemy.Name, enemy, true, i + 1);
            }
        }
        Console.WriteLine("-----------------------------------------------");
    }

    private void PrintHealthEntry(string name, Character unit, bool isEnemy, int index = 0)
    {
        double percentage = (double)unit.CurrentHealth / unit.MaxHealth * 100;

        if (percentage >= HighHealthThreshold)
        {
            Console.ForegroundColor = ConsoleColor.Green;
        }
        else if (percentage >= MidHealthThreshold)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }

        string prefix = isEnemy ? $"[{index}] " : "";
        Console.Write($"{prefix}{name}: {GetHealthBar(unit)} ({unit.CurrentHealth}/{unit.MaxHealth}) ");
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[MP: {unit.CurrentMana}/{unit.MaxMana}]");
        Console.ResetColor();
    }

    private string GetHealthBar(Character unit)
    {
        int barLength = 20;
        double ratio = (double)unit.CurrentHealth / unit.MaxHealth;
        int filled = (int)Math.Round(ratio * barLength);
        return $"[{new string('#', filled)}{new string('-', barLength - filled)}]";
    }
    
    private void ApplyRegen(Character unit)
    {
        int oldMana = unit.CurrentMana;
        unit.RegenerateMana(ManaRegenPerTurn);
        int diff = unit.CurrentMana - oldMana;
        
        if (diff > 0)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"   + {unit.Name} regenerates {diff} Mana. (Total: {unit.CurrentMana}/{unit.MaxMana})");
            Console.ResetColor();
        }
    }

    private bool PlayerTurn()
    {
        Console.WriteLine("\n----- YOUR TURN -----");
        Console.WriteLine("Choose action:");
        Console.WriteLine($"1: Standard Attack [Deals {_player.Damage} dmg]");
        Console.WriteLine($"2: Holy Smite (Special) [Cost: {AbilityManaCost} MP | Deals {_player.Damage * 2} dmg]");
        Console.WriteLine($"3: Forge Weapon [Adds 25% extra dmg]");
        Console.WriteLine($"4: Drink Potion [Heals {PlayerHealPotionAmount} HP]");
        Console.Write(">> ");

        string choice = Console.ReadLine();
        Console.Clear();

        if (choice == "1")
        {
            Character target = SelectTarget();
            if (target == null) return false;

            Console.Clear();
            Thread.Sleep(ActionDelay);
            _player.Attack(target);
            
            ApplyRegen(_player);
            return true;
        }
        else if (choice == "2")
        {
            if (_player.CurrentMana < AbilityManaCost)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nNot enough Mana! Need {AbilityManaCost}, have {_player.CurrentMana}.");
                Console.ResetColor();
                Pause();
                Console.Clear();
                return false;
            }

            Character target = SelectTarget();
            if (target == null) return false;

            Console.Clear();
            Thread.Sleep(ActionDelay);
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"! [{_player.Name} uses {AbilityManaCost} Mana]");
            Console.ResetColor();

            _player.ConsumeMana(AbilityManaCost);
            _player.UseSpecialAbility(target);
            
            return true;
        }
        else if (choice == "3")
        {
            Thread.Sleep(ActionDelay);
            _player.ForgeWeapon();
            
            ApplyRegen(_player);
            return true;
        }
        else if (choice == "4")
        {
            Thread.Sleep(ActionDelay);
            _player.Heal(PlayerHealPotionAmount);
            
            ApplyRegen(_player);
            return true;
        }
        else
        {
            Console.WriteLine("Invalid choice.");
            Pause();
            Console.Clear();
            return false;
        }
    }

    private Character SelectTarget()
    {
        var livingEnemies = _enemies.Where(e => e.IsAlive).ToList();

        Console.WriteLine("Select Your Target:");
        Console.WriteLine("---------------------------------");
        for (int i = 0; i < livingEnemies.Count; i++)
        {
            PrintHealthEntry(livingEnemies[i].Name, livingEnemies[i], true, i + 1);
        }
        Console.WriteLine("---------------------------------");
        Console.Write(">> ");

        if (int.TryParse(Console.ReadLine(), out int targetIndex) && targetIndex >= 1 && targetIndex <= livingEnemies.Count)
        {
            return livingEnemies[targetIndex - 1];
        }
        else
        {
            Console.WriteLine("Invalid target selection.");
            Pause();
            Console.Clear();
            return null;
        }
    }

    private void EnemyTurn()
    {
        Console.WriteLine("\n--- ENEMY TURN ---");
        Thread.Sleep(EnemyDisplayDelay);

        foreach (var enemy in _enemies.Where(e => e.IsAlive).ToList())
        {
            if (!_player.IsAlive) return;

            bool usedSpecial = false;
            
            if (_rng.Next(0, AiSpecialAttackChance) == 0)
            {
                if (enemy.ConsumeMana(AbilityManaCost))
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"! {enemy.Name} consumes {AbilityManaCost} Mana!");
                    Console.ResetColor();

                    enemy.UseSpecialAbility(_player);
                    usedSpecial = true;
                }
            }
            
            if (!usedSpecial)
            {
                enemy.Attack(_player);
                ApplyRegen(enemy);
            }

            Thread.Sleep(ActionDelay);
            Console.WriteLine(); 
        }
    }

    private void CleanUpEnemies()
    {
        _enemies.RemoveAll(e => !e.IsAlive);
    }

    private void Pause()
    {
        Console.Write("\nPress enter to continue...");
        Console.ReadKey(true);
    }

    private void EndGame()
    {
        Console.WriteLine("\n==============================================");
        if (_player.IsAlive)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("           YOU HAVE RETIRED FROM COMBAT       ");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("                 DEFEAT! GAME OVER.                 ");
        }
        Console.ResetColor();
        Console.WriteLine("==============================================");
        Pause();
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        GameManager game = new GameManager();
        game.StartGame();
    }
}