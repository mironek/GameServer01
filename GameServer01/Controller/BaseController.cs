using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace GameServer01.Controller
{
	abstract class BaseController
	{
		private protected RequestCode requestCode = RequestCode.None;

		/**
		 * GET / SET
		 */

		public RequestCode RequestCode { get { return requestCode; } }
		public ActionCode ActionCode { get { return ActionCode; } }
	}
}
