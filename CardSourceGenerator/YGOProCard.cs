using System;
using System.Text.Json;

namespace CardSourceGenerator
{
    public sealed class YGOProCard : IYGOCard
    {
        public string Name { get; set; } = string.Empty;
        public int AttackPoints { get; set; } = -1;
        public int DefensePoints { get; set; } = -1;
        public string MonsterType { get; set; } = string.Empty;
        public string MonsterAttribute { get; set; } = string.Empty;

        public YGOProCard(JsonElement element)
        {
            {
                if (!element.TryGetProperty("name", out var el))
                {
                    throw new Exception("name not found");
                }

                Name = el.GetString() ?? throw new Exception("name not a string");
            }

            {
                if (element.TryGetProperty("atk", out var el))
                {
                    AttackPoints = el.GetInt32();
                }
            }

            {
                if (element.TryGetProperty("def", out var el))
                {
                    DefensePoints = el.GetInt32();
                }

            }

            {
                if (element.TryGetProperty("race", out var el))
                {
                    MonsterType = el.GetString() ?? throw new Exception("type not a string");
                }
            }

            {
                if (element.TryGetProperty("attribute", out var el))
                {
                    MonsterAttribute = el.GetString() ?? throw new Exception("attribute not a string");
                }
            }
        }
    }
}
