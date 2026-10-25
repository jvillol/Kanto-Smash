using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace KantoSmash
{
    [Serializable]
    public class PokemonData
    {
        public int id;
        public string name;
        public string primaryType;
        public string secondaryType;
        public int speed;   // 1 - 10
        public int attack;  // 1 - 10
        public int weight;  // 1 - 10
        public Color primaryColor;
        public Sprite cachedSprite;

        public string FormattedNumber => $"#{id:D3}";
        public string SpriteUrl => $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{id}.png";

        public string SpecialAbilityName => GetSpecialAbilityName(primaryType);

        public static string GetSpecialAbilityName(string type)
        {
            switch (type)
            {
                case "Eléctrico": return "Rayo Fulgor";
                case "Fuego": return "Llamarada Ígnea";
                case "Agua": return "Hidrobomba Gigante";
                case "Planta": return "Drenadoras Esporas";
                case "Psíquico": return "Onda Telequinética";
                case "Hielo": return "Ventisca Congelante";
                case "Lucha": return "Ultra Golpe Sísmico";
                case "Fantasma": return "Paso Espectral";
                case "Dragón": return "Cometa Dracónico";
                case "Roca": return "Lluvia de Rocas";
                case "Tierra": return "Terremoto Sísmico";
                case "Veneno": return "Bomba Lodo Tóxica";
                case "Volador": return "Vendaval Ciclón";
                case "Bicho": return "Tijera X Cortante";
                default: return "Hiperrayo Destructor";
            }
        }
    }

    public static class KantoPokemonDatabase
    {
        private static List<PokemonData> _allPokemon;

        public static List<PokemonData> GetAllPokemon()
        {
            if (_allPokemon != null) return _allPokemon;

            _allPokemon = new List<PokemonData>
            {
                Create(1, "Bulbasaur", "Planta", "Veneno", 5, 5, 5),
                Create(2, "Ivysaur", "Planta", "Veneno", 6, 6, 6),
                Create(3, "Venusaur", "Planta", "Veneno", 5, 8, 8),
                Create(4, "Charmander", "Fuego", "", 7, 6, 4),
                Create(5, "Charmeleon", "Fuego", "", 7, 7, 5),
                Create(6, "Charizard", "Fuego", "Volador", 8, 9, 7),
                Create(7, "Squirtle", "Agua", "", 5, 5, 5),
                Create(8, "Wartortle", "Agua", "", 6, 6, 6),
                Create(9, "Blastoise", "Agua", "", 5, 8, 9),
                Create(10, "Caterpie", "Bicho", "", 4, 3, 2),
                Create(11, "Metapod", "Bicho", "", 3, 3, 4),
                Create(12, "Butterfree", "Bicho", "Volador", 7, 6, 4),
                Create(13, "Weedle", "Bicho", "Veneno", 5, 4, 2),
                Create(14, "Kakuna", "Bicho", "Veneno", 3, 3, 4),
                Create(15, "Beedrill", "Bicho", "Veneno", 8, 8, 4),
                Create(16, "Pidgey", "Normal", "Volador", 6, 4, 3),
                Create(17, "Pidgeotto", "Normal", "Volador", 7, 6, 5),
                Create(18, "Pidgeot", "Normal", "Volador", 9, 7, 6),
                Create(19, "Rattata", "Normal", "", 8, 5, 3),
                Create(20, "Raticate", "Normal", "", 8, 7, 5),
                Create(21, "Spearow", "Normal", "Volador", 7, 5, 3),
                Create(22, "Fearow", "Normal", "Volador", 8, 8, 6),
                Create(23, "Ekans", "Veneno", "", 6, 5, 4),
                Create(24, "Arbok", "Veneno", "", 7, 7, 6),
                Create(25, "Pikachu", "Eléctrico", "", 9, 7, 4),
                Create(26, "Raichu", "Eléctrico", "", 8, 8, 6),
                Create(27, "Sandshrew", "Tierra", "", 5, 6, 5),
                Create(28, "Sandslash", "Tierra", "", 7, 8, 6),
                Create(29, "Nidoran♀", "Veneno", "", 5, 5, 4),
                Create(30, "Nidorina", "Veneno", "", 6, 6, 5),
                Create(31, "Nidoqueen", "Veneno", "Tierra", 6, 8, 8),
                Create(32, "Nidoran♂", "Veneno", "", 6, 5, 4),
                Create(33, "Nidorino", "Veneno", "", 6, 7, 5),
                Create(34, "Nidoking", "Veneno", "Tierra", 7, 9, 8),
                Create(35, "Clefairy", "Normal", "", 4, 5, 4),
                Create(36, "Clefable", "Normal", "", 5, 7, 6),
                Create(37, "Vulpix", "Fuego", "", 7, 5, 4),
                Create(38, "Ninetales", "Fuego", "", 8, 8, 6),
                Create(39, "Jigglypuff", "Normal", "", 4, 4, 3),
                Create(40, "Wigglytuff", "Normal", "", 5, 7, 5),
                Create(41, "Zubat", "Veneno", "Volador", 7, 4, 3),
                Create(42, "Golbat", "Veneno", "Volador", 8, 7, 6),
                Create(43, "Oddish", "Planta", "Veneno", 4, 5, 3),
                Create(44, "Gloom", "Planta", "Veneno", 4, 6, 5),
                Create(45, "Vileplume", "Planta", "Veneno", 5, 8, 6),
                Create(46, "Paras", "Bicho", "Planta", 3, 6, 4),
                Create(47, "Parasect", "Bicho", "Planta", 4, 8, 6),
                Create(48, "Venonat", "Bicho", "Veneno", 5, 5, 5),
                Create(49, "Venomoth", "Bicho", "Veneno", 8, 7, 5),
                Create(50, "Diglett", "Tierra", "", 9, 5, 2),
                Create(51, "Dugtrio", "Tierra", "", 10, 8, 5),
                Create(52, "Meowth", "Normal", "", 8, 5, 4),
                Create(53, "Persian", "Normal", "", 9, 7, 6),
                Create(54, "Psyduck", "Agua", "", 5, 6, 5),
                Create(55, "Golduck", "Agua", "", 8, 8, 7),
                Create(56, "Mankey", "Lucha", "", 7, 7, 5),
                Create(57, "Primeape", "Lucha", "", 8, 9, 6),
                Create(58, "Growlithe", "Fuego", "", 6, 7, 5),
                Create(59, "Arcanine", "Fuego", "", 8, 9, 8),
                Create(60, "Poliwag", "Agua", "", 8, 5, 4),
                Create(61, "Poliwhirl", "Agua", "", 8, 6, 5),
                Create(62, "Poliwrath", "Agua", "Lucha", 6, 8, 8),
                Create(63, "Abra", "Psíquico", "", 8, 7, 4),
                Create(64, "Kadabra", "Psíquico", "", 9, 8, 5),
                Create(65, "Alakazam", "Psíquico", "", 10, 9, 5),
                Create(66, "Machop", "Lucha", "", 4, 7, 5),
                Create(67, "Machoke", "Lucha", "", 5, 8, 7),
                Create(68, "Machamp", "Lucha", "", 6, 10, 9),
                Create(69, "Bellsprout", "Planta", "Veneno", 4, 6, 3),
                Create(70, "Weepinbell", "Planta", "Veneno", 5, 7, 4),
                Create(71, "Victreebel", "Planta", "Veneno", 6, 8, 6),
                Create(72, "Tentacool", "Agua", "Veneno", 6, 5, 5),
                Create(73, "Tentacruel", "Agua", "Veneno", 8, 8, 7),
                Create(74, "Geodude", "Roca", "Tierra", 3, 7, 7),
                Create(75, "Graveler", "Roca", "Tierra", 4, 8, 9),
                Create(76, "Golem", "Roca", "Tierra", 4, 9, 10),
                Create(77, "Ponyta", "Fuego", "", 8, 7, 6),
                Create(78, "Rapidash", "Fuego", "", 9, 8, 7),
                Create(79, "Slowpoke", "Agua", "Psíquico", 2, 6, 7),
                Create(80, "Slowbro", "Agua", "Psíquico", 3, 8, 8),
                Create(81, "Magnemite", "Eléctrico", "", 5, 6, 5),
                Create(82, "Magneton", "Eléctrico", "", 6, 8, 7),
                Create(83, "Farfetch'd", "Normal", "Volador", 6, 7, 5),
                Create(84, "Doduo", "Normal", "Volador", 7, 7, 5),
                Create(85, "Dodrio", "Normal", "Volador", 9, 8, 7),
                Create(86, "Seel", "Agua", "", 5, 5, 7),
                Create(87, "Dewgong", "Agua", "Hielo", 6, 7, 8),
                Create(88, "Grimer", "Veneno", "", 3, 6, 6),
                Create(89, "Muk", "Veneno", "", 4, 8, 8),
                Create(90, "Shellder", "Agua", "", 4, 6, 5),
                Create(91, "Cloyster", "Agua", "Hielo", 6, 8, 9),
                Create(92, "Gastly", "Fantasma", "Veneno", 7, 7, 2),
                Create(93, "Haunter", "Fantasma", "Veneno", 8, 8, 3),
                Create(94, "Gengar", "Fantasma", "Veneno", 9, 9, 5),
                Create(95, "Onix", "Roca", "Tierra", 6, 6, 10),
                Create(96, "Drowzee", "Psíquico", "", 4, 5, 6),
                Create(97, "Hypno", "Psíquico", "", 6, 7, 7),
                Create(98, "Krabby", "Agua", "", 5, 8, 4),
                Create(99, "Kingler", "Agua", "", 6, 9, 7),
                Create(100, "Voltorb", "Eléctrico", "", 9, 5, 4),
                Create(101, "Electrode", "Eléctrico", "", 10, 7, 6),
                Create(102, "Exeggcute", "Planta", "Psíquico", 4, 5, 4),
                Create(103, "Exeggutor", "Planta", "Psíquico", 5, 9, 9),
                Create(104, "Cubone", "Tierra", "", 4, 6, 5),
                Create(105, "Marowak", "Tierra", "", 5, 8, 6),
                Create(106, "Hitmonlee", "Lucha", "", 8, 9, 6),
                Create(107, "Hitmonchan", "Lucha", "", 7, 9, 6),
                Create(108, "Lickitung", "Normal", "", 3, 6, 7),
                Create(109, "Koffing", "Veneno", "", 4, 6, 4),
                Create(110, "Weezing", "Veneno", "", 5, 8, 6),
                Create(111, "Rhyhorn", "Tierra", "Roca", 3, 7, 9),
                Create(112, "Rhydon", "Tierra", "Roca", 4, 9, 10),
                Create(113, "Chansey", "Normal", "", 5, 4, 7),
                Create(114, "Tangela", "Planta", "", 5, 7, 6),
                Create(115, "Kangaskhan", "Normal", "", 7, 8, 8),
                Create(116, "Horsea", "Agua", "", 6, 5, 3),
                Create(117, "Seadra", "Agua", "", 7, 7, 5),
                Create(118, "Goldeen", "Agua", "", 6, 6, 5),
                Create(119, "Seaking", "Agua", "", 6, 8, 6),
                Create(120, "Staryu", "Agua", "", 8, 6, 5),
                Create(121, "Starmie", "Agua", "Psíquico", 9, 8, 7),
                Create(122, "Mr. Mime", "Psíquico", "", 7, 7, 6),
                Create(123, "Scyther", "Bicho", "Volador", 9, 9, 6),
                Create(124, "Jynx", "Hielo", "Psíquico", 8, 8, 5),
                Create(125, "Electabuzz", "Eléctrico", "", 9, 8, 6),
                Create(126, "Magmar", "Fuego", "", 8, 8, 6),
                Create(127, "Pinsir", "Bicho", "", 7, 9, 7),
                Create(128, "Tauros", "Normal", "", 9, 8, 8),
                Create(129, "Magikarp", "Agua", "", 7, 2, 3),
                Create(130, "Gyarados", "Agua", "Volador", 7, 10, 9),
                Create(131, "Lapras", "Agua", "Hielo", 5, 8, 9),
                Create(132, "Ditto", "Normal", "", 5, 5, 4),
                Create(133, "Eevee", "Normal", "", 6, 5, 4),
                Create(134, "Vaporeon", "Agua", "", 6, 8, 7),
                Create(135, "Jolteon", "Eléctrico", "", 10, 8, 5),
                Create(136, "Flareon", "Fuego", "", 6, 9, 6),
                Create(137, "Porygon", "Normal", "", 5, 7, 6),
                Create(138, "Omanyte", "Roca", "Agua", 4, 6, 5),
                Create(139, "Omastar", "Roca", "Agua", 5, 8, 7),
                Create(140, "Kabuto", "Roca", "Agua", 5, 6, 5),
                Create(141, "Kabutops", "Roca", "Agua", 7, 9, 6),
                Create(142, "Aerodactyl", "Roca", "Volador", 10, 9, 7),
                Create(143, "Snorlax", "Normal", "", 3, 9, 10),
                Create(144, "Articuno", "Hielo", "Volador", 7, 9, 8),
                Create(145, "Zapdos", "Eléctrico", "Volador", 9, 9, 7),
                Create(146, "Moltres", "Fuego", "Volador", 8, 9, 8),
                Create(147, "Dratini", "Dragón", "", 5, 6, 4),
                Create(148, "Dragonair", "Dragón", "", 7, 7, 5),
                Create(149, "Dragonite", "Dragón", "Volador", 7, 10, 9),
                Create(150, "Mewtwo", "Psíquico", "", 10, 10, 8),
                Create(151, "Mew", "Psíquico", "", 9, 9, 5)
            };

            return _allPokemon;
        }

        public static PokemonData GetRandomPokemon()
        {
            var list = GetAllPokemon();
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        private static PokemonData Create(int id, string name, string t1, string t2, int spd, int atk, int wgt)
        {
            Color col = GetColorForType(t1);
            var data = new PokemonData
            {
                id = id,
                name = name,
                primaryType = t1,
                secondaryType = t2,
                speed = spd,
                attack = atk,
                weight = wgt,
                primaryColor = col,
                cachedSprite = CreateFallbackSprite(col)
            };
            return data;
        }

        public static Color GetColorForType(string type)
        {
            switch (type)
            {
                case "Fuego": return new Color(1f, 0.34f, 0.13f);
                case "Agua": return new Color(0.13f, 0.59f, 0.95f);
                case "Planta": return new Color(0.30f, 0.69f, 0.31f);
                case "Eléctrico": return new Color(1f, 0.92f, 0.23f);
                case "Psíquico": return new Color(0.91f, 0.12f, 0.39f);
                case "Hielo": return new Color(0f, 0.74f, 0.83f);
                case "Dragón": return new Color(0.40f, 0.23f, 0.72f);
                case "Fantasma": return new Color(0.49f, 0.34f, 0.76f);
                case "Lucha": return new Color(0.85f, 0.26f, 0.08f);
                case "Roca": return new Color(0.55f, 0.43f, 0.39f);
                case "Tierra": return new Color(0.83f, 0.64f, 0.35f);
                case "Veneno": return new Color(0.67f, 0.28f, 0.74f);
                default: return new Color(0.75f, 0.78f, 0.85f);
            }
        }

        public static Sprite CreateFallbackSprite(Color color)
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size * 0.42f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= radius)
                        tex.SetPixel(x, y, dist > radius - 3f ? Color.white : color);
                    else
                        tex.SetPixel(x, y, Color.clear);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
        }

        public static IEnumerator LoadSpriteCoroutine(PokemonData pokemon, Action<Sprite> onLoaded)
        {
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(pokemon.SpriteUrl))
            {
                yield return uwr.SendWebRequest();
                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
                    tex.filterMode = FilterMode.Point;
                    Sprite spr = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 64f);
                    pokemon.cachedSprite = spr;
                    onLoaded?.Invoke(spr);
                }
                else
                {
                    onLoaded?.Invoke(pokemon.cachedSprite);
                }
            }
        }
    }
}

