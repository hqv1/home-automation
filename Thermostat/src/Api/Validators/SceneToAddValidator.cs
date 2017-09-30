using FluentValidation;
using Hqv.Thermostat.Api.Models;

namespace Hqv.Thermostat.Api.Validators
{
    public class SceneToAddValidator : AbstractValidator<SceneToAddModel>
    {
        public SceneToAddValidator()
        {
            RuleFor(x => x.HeatHoldTemp).ExclusiveBetween(200, 1000);
            RuleFor(x => x.ColdHoldTemp).ExclusiveBetween(200, 1000);
            RuleFor(x => x.ColdHoldTemp).GreaterThanOrEqualTo(x => x.HeatHoldTemp);
        }
    }
}