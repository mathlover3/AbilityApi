using BoplFixedMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbilityApi
{
    public class InstantTestAbility : MonoUpdatable
    {
        public void Awake()
        {
            Updater.RegisterUpdatable(this);
            InstantAbility ab = GetComponent<InstantAbility>();
            if (ab.funcOnEnter == null)
            {
                ab.funcOnEnter = new();
            }
            var func = ab.funcOnEnter;
            
            func.AddListener(CastAbility);
        }

        public void CastAbility()
        {
            AudioManager.Get().Play("startGame");
        }

        public override void Init()
        {

        }

        public override void UpdateSim(Fix SimDeltaTime)
        {

        }
    }
}
