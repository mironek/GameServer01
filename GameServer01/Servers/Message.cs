using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace GameServer01.Servers
{
	internal class Message
	{
		private byte[] dataBuffer = new byte[1024];
		private int startIndex = 0;

		/** 
		 * Ta funkcja służy do odczytywania wiadomości z bufora danych, który jest tablicą bajtów. 
		 * Funkcja przyjmuje jako parametr ilość danych, które zostały odebrane i dodane do bufora. Oraz funkcje callback która będzie wezwana jak zbierzemy informacje które klient będzie potrzebował do wykonania działania 
		 */
		public void ReadMessage(int __dataAmount, Action<RequestCode, ActionCode, string>OnProcessDataCallback)
		{
			// Zwiększa indeks startowy o ilość odebranych danych. Indeks startowy wskazuje na początek wiadomości w buforze.
			startIndex += __dataAmount;

			// Sprawdza, czy indeks startowy jest mniejszy niż 4.
			// Jeśli tak, to oznacza, że nie ma wystarczającej ilości danych do odczytania długości wiadomości, która jest zapisana na pierwszych czterech bajtach bufora.
			// W takim przypadku funkcja kończy działanie i zwraca.
			if (startIndex < 4) return;

			// Odczytuje długość wiadomości z pierwszych czterech bajtów bufora za pomocą metody BitConverter.ToInt32.
			// Ta metoda konwertuje tablicę bajtów na liczbę całkowitą (liczba całkowita ma minimum 4 bajty).
			int _byteCount = BitConverter.ToInt32(dataBuffer, 0);

			// Sprawdza, czy indeks startowy pomniejszony o 4 jest większy lub równy długości wiadomości.
			// Jeśli tak, to oznacza, że w buforze jest wystarczająca ilość danych do odczytania całej wiadomości.
			if (startIndex - 4 >= _byteCount) 
			{
				// Request code to liczba całkowita zamieszczona w wiadomości tuż po długości - czyli po 4 bajcie - i mająca kolejne 4 bajty (ToInt32() znaczy ze wyciągnie tylko 4 bajty)
				RequestCode _requestCode = (RequestCode) BitConverter.ToInt32(dataBuffer, 4);
				// tak samo jak wyżej tylko wyciągnie 4 bajty po 8 bajcie (tam gdzie kończy się informacja requestCode)
				ActionCode _actionCode = (ActionCode)BitConverter.ToInt32(dataBuffer, 8);


				// Odczytuje wiadomość z bufora za pomocą metody Encoding.UTF8.GetString.
				// Ta metoda konwertuje tablicę bajtów na ciąg znaków w kodowaniu UTF-8.
				// Metoda przyjmuje jako parametry tablicę bajtów, indeks początkowy i ilość bajtów do odczytania.
				// W tym przypadku indeks początkowy to 12, a ilość bajtów to długość wiadomości .
				// string _s = Encoding.UTF8.GetString(dataBuffer, 4, _byteCount);

				// jakoż że 0-3 to długość 4-7 to request code 8-11 to actionCode daltego wszystko pozatym będzie dodatkową informacją, może to być jakiś ciąg znaków lub np hasło login, nr pokoju czy kierunek - zależnie co będzie przyjmował i tłumaczył kontroler
				string _s = Encoding.UTF8.GetString(dataBuffer, 12, _byteCount - 4);

				// posiadając powyższe informacje możemy przesłać je do funkcji callback któa została podana przy wzywaniu metody ReadMessage 
				OnProcessDataCallback(_requestCode, _actionCode, _s);

				// Przesuwa pozostałe dane w buforze na początek tablicy za pomocą metody Array.Copy.
				// Ta metoda kopiuje fragment tablicy do innej tablicy. Metoda przyjmuje jako parametry źródłową tablicę, indeks źródłowy, docelową tablicę, indeks docelowy i ilość elementów do skopiowania.
				// W tym przypadku źródłową i docelową tablicą jest ta sama tablica danych, indeks źródłowy to suma długości wiadomości (z poprzedniej wiadomości) i 4, indeks docelowy to 0, a ilość elementów to różnica między indeksem startowym (z poprzedniej wiadomości) a sumą długości wiadomości i 4.
				Array.Copy(dataBuffer, _byteCount + 4, dataBuffer, 0, startIndex - 4 - _byteCount);

				// Ustawia indeks startowy (dla nowej wiadomści) na wartość długości wiadomości plus 4. To oznacza, że następna wiadomość zaczyna się od tego indeksu w buforze.
				startIndex = _byteCount + 4;
			}
			else
			{
				// Jeśli indeks startowy pomniejszony o 4 jest mniejszy niż długość wiadomości, to oznacza, że w buforze nie ma wystarczającej ilości danych do odczytania całej wiadomości.
				// W takim przypadku funkcja kończy działanie i zwraca.
				return;
			}
		}

		/**
		 * PackData to metoda która pakuje wszystkie informacje w jeden ciąg znaków który będzie przekazywany do serwera jako mapa bitów i wysyłany do klienta aby ten mógł go odczytać według wzoru.
		 * Wzór odczytu jest opisany w Metodzie ReadMessage
		 */
		public static byte[] PackData(RequestCode __requestCode, string __data)
		{
			byte[] _requestCodeBytes = BitConverter.GetBytes((int)__requestCode);
			byte[] _dataBytes = Encoding.UTF8.GetBytes(__data);

			int _newDataAmount = _requestCodeBytes.Length + _dataBytes.Length;
			byte[] _newDataAmountBytes = BitConverter.GetBytes(_newDataAmount);
			return _newDataAmountBytes.Concat(_requestCodeBytes).ToArray().Concat(_dataBytes).ToArray();
		}


		/**
		 * 
		 * GET / SET 
		 */
		public byte[] Data
		{
			get { return dataBuffer; }
		}
		public int StartIndex
		{
			get { return startIndex; }
		}

		public int RemainSize
		{
			get { return dataBuffer.Length - startIndex; }
		}

		
	}
}
