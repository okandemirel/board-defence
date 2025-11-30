using UnityEngine;
using Strada.Core.DI;
using Strada.Core.Modules;
using BoardDefence.UI.Controllers;

namespace BoardDefence.UI
{
    [CreateAssetMenu(fileName = "BoardUIModuleConfig", menuName = "BoardDefence/Board UI Module Config")]
    public class BoardUIModuleConfig : ModuleConfig
    {
        protected override void Configure(IModuleBuilder builder)
        {
            builder
                .RegisterController<UIController>();
        }

        public override void Initialize(IServiceLocator services)
        {
            var container = services.Get<IContainer>();
            var uiController = services.Get<UIController>();
            InjectionProcessor.Inject(uiController, container);
            uiController?.Initialize();
        }
    }
}
