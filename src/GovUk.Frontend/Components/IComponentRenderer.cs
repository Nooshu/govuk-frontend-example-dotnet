using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public interface IComponentRenderer
{
    string Name { get; }

    string Render(ParamBag parameters);
}
