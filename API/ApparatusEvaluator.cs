using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace FacilityMeltdown.API {
    public abstract class ApparatusEvaluator {
        public abstract int Evaluate(LungProp apparatus);
    }
}
