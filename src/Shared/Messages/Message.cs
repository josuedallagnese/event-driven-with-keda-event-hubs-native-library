using System.Linq;
using Bogus;

namespace Shared.Messages
{
    public class Message
    {
        public string Id { get; set; }
        public string Customer { get; set; }
        public bool SimulateError { get; set; }

        public static Message Generate()
        {
            var faker = new Faker<Message>()
                .RuleFor(r => r.Id, p => p.Random.Guid().ToString())
                .RuleFor(r => r.Customer, p => p.Name.FullName())
                .RuleFor(r => r.SimulateError, p => p.Random.Bool());

            return faker.Generate(1).Single();
        }
    }
}
