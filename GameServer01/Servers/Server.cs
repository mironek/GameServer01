using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Common;
using GameServer01.Controller;

namespace GameServer01.Servers
{
	internal class Server
	{

		/**
		 *   Klasa ma prywatne pola ipEP, serverSocket, clientList i controller. 
		 *   ipEP - to obiekt klasy IPEndPoint, który przechowuje adres IP i port serwera. 
		 *   serverSocket - to obiekt klasy Socket, który reprezentuje gniazdo sieciowe serwera. 
		 *   clientList - to lista obiektów klasy Client, która przechowuje informacje o połączonych klientach. 
		 *   controller - to obiekt klasy ControllerManager, który zarządza logiką gry i obsługą żądań od klientów.
		 */
		private IPEndPoint ipEP;
		private Socket serverSocket;
		private List<Client> clientList = new List<Client>();
		ControllerManager controller;

		/**
		 * Klasa ma konstruktor, który przyjmuje dwa parametry: __ip i __port.
		 */
		public Server(string __ip, int __port)
		{
			// Konstruktor inicjalizuje pole ipEP za pomocą podanego adresu IP i portu. 
			ipEP = new IPEndPoint(IPAddress.Parse(__ip), __port);

			// Konstruktor tworzy również nowy obiekt controller i przekazuje mu referencję do samego siebie.
			controller = new ControllerManager(this);
		}

		/**
		 *  Klasa ma publiczną metodę Start, która tworzy i uruchamia gniazdo serwera.
		*/
		public void Start()
		{
			// Metoda używa pola serverSocket, aby utworzyć nowy obiekt Socket z parametrami AddressFamily.InterNetwork, SocketType.Stream i ProtocolType.Tcp. Oznacza to, że serwer używa protokołu TCP do komunikacji z klientami w sieci IPv4.
			serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			// Metoda używa metody Bind na obiekcie serverSocket, aby powiązać go z adresem IP i portem przechowywanym w polu ipEP.
			serverSocket.Bind(ipEP);

			// Metoda używa również metody Listen na obiekcie serverSocket, aby ustawić go w tryb nasłuchiwania połączeń przychodzących.
			// Argument 0 oznacza, że serwer akceptuje dowolną liczbę połączeń w kolejce. 
			serverSocket.Listen(0);

			// Metoda używa metody BeginAccept na obiekcie serverSocket, aby rozpocząć asynchroniczne akceptowanie połączeń od klientów. 
			// Metoda przekazuje metodę AcceptCB jako parametr callbacku, który zostanie wywołany po zaakceptowaniu połączenia.
			serverSocket.BeginAccept(AcceptCB, null);
		}

		/**
		 * Klasa ma prywatną metodę AcceptCB, która jest callbackiem wywołanym w Start przy rozpoczęciu asynchronicznego akceptrowania połączenia z klientem i przyjmuje jeden parametr: _ar. 
		 * __ar - to obiekt klasy IAsyncResult, który zawiera informacje o stanie asynchronicznej operacji akceptowania połączenia.  
		 */
		private void AcceptCB(IAsyncResult __ar)
		{
			// Metoda używa metody EndAccept na obiekcie serverSocket, aby zakończyć asynchroniczną operację i uzyskać obiekt Socket reprezentujący gniazdo klienta.
			Socket _clientSocket = serverSocket.EndAccept(__ar);

			// Metoda tworzy nowy obiekt Client za pomocą obiektu Socket i referencji do samego siebie jako parametrów.
			// Obiekt Client jest klasą pomocniczą, która zarządza komunikacją z pojedynczym klientem.
			Client _client = new Client(_clientSocket, this);

			// Metoda dodaje obiekt Client do listy clientList za pomocą metody Add.
			clientList.Add(_client);
		}

		/**
		 * Klasa ma publiczną metodę RemoveClient, która przyjmuje jeden parametr: _client. 
		 * __client - to obiekt klasy Client, który ma być usunięty z listy clientList. 
		 */
		public void RemoveClient(Client __client)
		{
			// Metoda używa słowa kluczowego lock na liście clientList, aby zapewnić bezpieczny dostęp do niej w środowisku wielowątkowym.
			// lock blokuje element w zakresie na czas wykonywania lock'a
			// inne funkcje nie mają dostępu do elementu zablokowanego i czekają z dalyszm wykonywaniem aż lock się zakończy
			lock (clientList)
			{
				// Następnie metoda używa metody Remove na liście clientList, aby usunąć podany obiekt Client z listy.
				clientList.Remove(__client);
			}
		}

		/**
		 * Klasa ma publiczną metodę SendResponse, która przyjmuje trzy parametry: _client, _requestCode i _data. 
		 * __client - to obiekt klasy Client, do którego ma być wysłana odpowiedź. 
		 * __requestCode - to typ wyliczeniowy RequestCode, który określa rodzaj żądania, na które odpowiada serwer. 
		 * __data - to ciąg znaków, który zawiera dane odpowiedzi. 
		 */
		public void SendResponse(Client __client, RequestCode __requestCode, string __data)
		{
			// Metoda wywołuje metodę Send na obiekcie Client, przekazując mu parametry _requestCode i _data.
			// Metoda Send jest metodą pomocniczą klasy Client, która koduje dane odpowiedzi i wysyła je do klienta za pomocą gniazda sieciowego.
			__client.Send(__requestCode, __data);
		}

		/**
		 * Klasa ma publiczną metodę HandleRequest, która przyjmuje cztery parametry: _requestCode, _actionCode, _data i _client. 
		 * __requestCode - to typ wyliczeniowy RequestCode, który określa rodzaj żądania, które otrzymał serwer. 
		 * __actionCode - to typ wyliczeniowy ActionCode, który określa konkretną akcję do wykonania przez serwer. 
		 * __data - to ciąg znaków, który zawiera dodatkowe informacje o żądaniu. 
		 * __client - to obiekt klasy Client, który wysłał żądanie. Metoda wywołuje metodę HandleRequest na obiekcie controller, przekazując mu te same parametry. 
		 */
		public void HandleRequest(RequestCode __requestCode, ActionCode __actionCode, string __data, Client __client)
		{
			// Metoda HandleRequest jest metodą klasy ControllerManager, która znajduje i wywołuje odpowiedni kontroler i metodę do obsługi żądania.
			// Kontroler to klasa, która implementuje logikę gry i zwraca dane odpowiedzi do serwera.
			controller.HandleRequest(__requestCode, __actionCode, __data, __client);
		}


	}
}
