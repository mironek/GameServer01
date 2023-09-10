using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using System.Reflection; // Refleksja to mechanizm języka C#, który pozwala na badanie i modyfikowanie kodu w trakcie jego wykonywania. 
using GameServer01.Servers;

namespace GameServer01.Controller
{
	class ControllerManager
	{
		private Dictionary<RequestCode, BaseController> controllerDict = new Dictionary<RequestCode, BaseController> ();
		private Server server;
		
		public ControllerManager(Server __server)
		{
			server = __server;
			Init();
		}

		private void Init()
		{
			DefaultController _defaultController = new DefaultController ();
			controllerDict.Add(_defaultController.RequestCode, new DefaultController());
		}

		/**
		 * Ta funkcja obsługuje żądania od klienta. 
		 * __requestCode - jest typem wyliczeniowym, który określa rodzaj żądania, np. User, Room, Game itp. 
		 * __actionCode - jest również typem wyliczeniowym, który określa konkretną akcję do wykonania, np. Login, CreateRoom, StartGame itp. 
		 * __data - jest ciągiem znaków, który zawiera dodatkowe informacje o żądaniu, np. nazwę użytkownika, hasło, nazwę pokoju itp.
		 * __client - wybrany klient który otrzyma wiadomość
		 */
		public void HandleRequest(RequestCode __requestCode, ActionCode __actionCode, string __data, Client __client)
		{
			BaseController _controller;

			//  Funkcja próbuje znaleźć kontroler odpowiadający danemu _requestCode w słowniku controllerDict, który przechowuje pary (requestCode, controller).
			//  Jeśli nie znajdzie odpowiedniego kontrolera, wypisuje komunikat o błędzie i kończy działanie.
			bool _isGet = controllerDict.TryGetValue (__requestCode, out _controller);
			if (!_isGet) // jeśłu nie udało sięznaleźć kontrollera odpowiadającego danemu requestowi
			{
				Console.WriteLine("Can't get controller for: " + __requestCode);
			}

			// Funkcja próbuje znaleźć metodę odpowiadającą danemu _actionCode w klasie kontrolera za pomocą refleksji.
			// Refleksja to mechanizm języka C#, który pozwala na badanie i modyfikowanie kodu w trakcie jego wykonywania.

			//  Funkcja używa metody Enum.GetName, aby uzyskać nazwę metody z typu wyliczeniowego _actionCode.
			string _methodName = Enum.GetName(typeof (ActionCode), __actionCode);
			// Następnie używa metody GetType na obiekcie kontrolera, aby uzyskać jego typ.
			// Wreszcie używa metody GetMethod na typie kontrolera, aby uzyskać informacje o metodzie o danej nazwie.
			MethodInfo _methodInfo = _controller.GetType().GetMethod(_methodName);

			if (_methodInfo == null)
			{
				//  Jeśli nie znajdzie odpowiedniej metody, wypisuje komunikat o błędzie i kończy działanie.
				Console.WriteLine("Theres no corresponding method for: " + _methodName);
				return;
			}

			//  Funkcja tworzy tablicę parametrów z __data.
			object[] _parameters = new object[] { __data };

			//  Następnie wywołuje metodę na obiekcie kontrolera za pomocą metody Invoke na obiekcie methodInfo, przekazując mu tablicę parametrów.
			//  Metoda Invoke zwraca obiekt, który może być wynikiem działania metody lub null, jeśli metoda nie zwraca niczego.
			object _obj = _methodInfo.Invoke(_controller, _parameters);

			// Jeśli obiekt zwrócony przez metodę Invoke jest null, funkcja kończy działanie.
			if (_obj == null) return;

			// W przeciwnym razie funkcja może wykonać dalsze operacje na tym obiekcie lub przesłać go do klienta.

			// wyślij odpowiedz na __requestCode którą jest _obj jako string który potem jest przeformatowany do bajtów
			server.SendResponse(__client, __requestCode, _obj as string);

		} 
	}
}
