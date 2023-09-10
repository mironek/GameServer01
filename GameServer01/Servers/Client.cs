using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Common;
using MySql.Data.MySqlClient;
using GameServer01.Tool;

namespace GameServer01.Servers
{
	/**
	 * Klasa ma prywatne pola clientSocket, server, msg i mysqlConnection. 
	 * clientSocket - to obiekt klasy Socket, który reprezentuje gniazdo sieciowe klienta. server to obiekt klasy Server, który reprezentuje serwer, z którym się komunikuje. 
	 * msg - to obiekt klasy Message, który przechowuje i przetwarza dane otrzymane od lub wysłane do serwera. 
	 * mysqlConnection - to obiekt klasy MySqlConnection, który reprezentuje połączenie z bazą danych MySQL za pomocą biblioteki MySQL Connector/NET.
	 */
	internal class Client
	{
		private Socket clientSocket;
		private Server server;
		private Message msg = new Message();
		private MySqlConnection mysqlConnection;

		/**
		 * Klasa ma konstruktor, który przyjmuje dwa parametry: clientSocket i server.

		*/
		public Client(Socket __clientSocket, Server __server)
		{
			// Konstruktor inicjalizuje pola clientSocket i server za pomocą podanych argumentów. 		
			clientSocket = __clientSocket;
			server = __server;

			//  Konstruktor wywołuje również statyczną metodę Connect z klasy ConnectionHelper, która nawiązuje połączenie z bazą danych MySQL za pomocą ciągu połączenia przechowywanego w stałej CONNECTIONSTRING.
			// Metoda Connect zwraca obiekt MySqlConnection, który jest przypisywany do pola mysqlConnection.
			mysqlConnection = ConnectionHelper.Connect();
		}

		/**
		 * Klasa ma publiczną metodę Send, która przyjmuje dwa parametry: _requestCode i _data. 
		 * __requestCode - to typ wyliczeniowy RequestCode, który określa rodzaj żądania, które wysyła klient. 
		 * __data - to ciąg znaków, który zawiera dane żądania. Metoda używa statycznej metody PackData z klasy Message, aby zapakować dane żądania do jednego ciągu bajtów. 
		 */
		public void Send(RequestCode __requestCode, string __data)
		{
			// pakujemy dane do jednego ciągu bitów za pomocą funckji PackData
			// Metoda PackData dodaje do danych żądania nagłówek zawierający długość danych i kod żądania. 
			byte[] _bytesToSend = Message.PackData(__requestCode, __data);

			// za pomocą wbudiowanej funkcji wysyłamy do serwera naszą odpowiedz składającasię z ciągu bajtów
			// Metoda używa metody Send na obiekcie clientSocket, aby wysłać ciąg bajtów do serwera za pomocą gniazda sieciowego.
			clientSocket.Send(_bytesToSend);
		}

		/**
		 *  Klasa ma publiczną metodę Start, która rozpoczyna asynchroniczne odbieranie danych od serwera.
		*/
		public void Start() {
			// Metoda używa metody BeginReceive na obiekcie clientSocket, aby rozpocząć asynchroniczną operację odbierania danych.
			// Metoda przekazuje polu msg dane otrzymane od serwera za pomocą właściwości Data, StartIndex i RemainSize.
			// Data to tablica bajtów, która przechowuje dane otrzymane od serwera.
			// StartIndex to indeks tablicy Data, od którego zaczyna się odczytywanie danych.
			// RemainSize to liczba bajtów, które pozostały do odczytania z tablicy Data.
			// Metoda przekazuje również metodę ReciveCB jako parametr callbacku, który zostanie wywołany po zakończeniu asynchronicznej operacji odbierania danych.
			clientSocket.BeginReceive(msg.Data, msg.StartIndex, msg.RemainSize, SocketFlags.None, ReciveCB, null);
		}

		/**
		 * Klasa ma prywatną metodę ReciveCB, która przyjmuje jeden parametr: _ar. 
		 * __ar - to obiekt klasy IAsyncResult, który zawiera informacje o stanie asynchronicznej operacji odbierania danych. 
		 */
		private void ReciveCB(IAsyncResult __ar)
		{
			// Metoda używa metody EndReceive na obiekcie clientSocket, aby zakończyć asynchroniczną operację i uzyskać liczbę bajtów odebranych od serwera.			
			int _byteReceived = clientSocket.EndReceive(__ar);
			
			if (_byteReceived == 0)
			{
				// Jeśli liczba ta jest równa 0, oznacza to, że połączenie zostało zamknięte i metoda wywołuje metodę Close, aby zwolnić zasoby i usunąć klienta z listy serwera.				
				Close();
			}
			else
			{
				// W przeciwnym razie metoda używa metody ReadMessage na obiekcie msg, aby przeczytać i przetworzyć dane odebrane od serwera.
				// Metoda ReadMessage przyjmuje dwa parametry: liczbę bajtów odebranych od serwera i metodę OnProcessMessage jako parametr delegata.
				// Metoda ReadMessage sprawdza nagłówek danych odebranych od serwera i odczytuje długość danych i kod odpowiedzi.
				// Następnie metoda ReadMessage odczytuje dane odpowiedzi z tablicy Data i wywołuje metodę OnProcessMessage z parametrami kod odpowiedzi i dane odpowiedzi.				
				msg.ReadMessage(_byteReceived, OnProcessMessage);

				// Po przetworzeniu danych odebranych od serwera metoda ReciveCB wywołuje ponownie metodę Start, aby kontynuować odbieranie danych od serwera.
				Start();
			}
		}

		/**
		 * Klasa ma publiczną metodę OnProcessMessage, która przyjmuje trzy parametry: _requestCode, _actionCode i _data. 
		 * __requestCode - to typ wyliczeniowy RequestCode, który określa rodzaj żądania, na które odpowiada serwer. 
		 * __actionCode - to typ wyliczeniowy ActionCode, który określa konkretną akcję do wykonania przez klienta. 
		 * __data - to ciąg znaków, który zawiera dane odpowiedzi. 
		 */
		public void OnProcessMessage(RequestCode __requestCode, ActionCode __actionCode, string __data)
		{
			// Metoda wywołuje metodę HandleRequest na obiekcie server, przekazując mu te same parametry oraz referencję do samego siebie.
			// Metoda HandleRequest jest metodą klasy Server, która znajduje i wywołuje odpowiedni kontroler i metodę do obsługi żądania.
			server.HandleRequest(__requestCode, __actionCode, __data, this);
		}

		/**
		 * Klasa ma prywatną metodę Close, która zamyka połączenie z serwerem i bazą danych oraz usuwa klienta z listy serwera.  
		 */
		private void Close()
		{
			// Metoda używa statycznej metody CloseConnection z klasy ConnectionHelper, aby zamknąć połączenie z bazą danych MySQL za pomocą obiektu mysqlConnection.
			ConnectionHelper.CloseConnection(mysqlConnection);

			// Metoda sprawdza, czy obiekt clientSocket nie jest null, a jeśli nie, to używa metody Close na obiekcie clientSocket, aby zamknąć gniazdo sieciowe klienta. 
			if (clientSocket != null)
			{
				clientSocket.Close();
			}

			// Metoda wywołuje również metodę RemoveClient na obiekcie server, przekazując mu referencję do samego siebie.
			// Metoda RemoveClient jest metodą klasy Server, która usuwa klienta z listy clientList za pomocą metody Remove.
			server.RemoveClient(this);
		}
	}
}
