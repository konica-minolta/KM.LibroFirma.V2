using MFiles.VAF.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.LibroFirma.V2
{
   public static class MFAlias
   {
      /*----------------------------------------*
       * Properties                             *
       *----------------------------------------*/
      [MFPropertyDef]
      public static MFIdentifier SignDate = "PD.DataUltimaFirma";
        /*----------------------------------------*
        * WorkFlows                              *
        *----------------------------------------*/
        [MFWorkflow]
        public static MFIdentifier WF_WorkFlowFirma = "WF.Workflow Firma"; // ho dovuto metterlo come alias perchè non li serializza nei custom directive

        /*----------------------------------------*
         * States of workflows                    *
         *----------------------------------------*/
        [MFState]
        public static MFIdentifier SignStateSigned = "WFS.SignWorkflow.Signed";// ho dovuto metterlo come alias perchè non li serializza nei custom directive
    }
}
