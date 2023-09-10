using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer01.Tool
{
	/**
	 * Ten kod to klasa pomocnicza do nawiązywania i zamykania połączenia z bazą danych MySQL za pomocą biblioteki MySQL 
	 */
	class ConnectionHelper
	{
		/**
		 * Klasa ma stałą CONNECTIONSTRING, która przechowuje ciąg połączenia z bazą danych.
		 * Ciąg ten zawiera informacje o źródle danych (adres IP i port serwera MySQL), nazwie bazy danych, nazwie użytkownika i haśle.
		*/
		private const string CONNECTIONSTRING = "datasource=127.0.0.1;port=3306;database=game03;user=root;pwd=1234";

		/**
		 * Klasa ma statyczną metodę Connect, która tworzy i otwiera obiekt MySqlConnection za pomocą ciągu połączenia. 		 * 
		 */
		public static MySqlConnection Connect()
		{
			// Obiekt ten reprezentuje fizyczne połączenie z bazą danych i umożliwia wykonywanie zapytań SQL. Metoda zwraca obiekt MySqlConnection do dalszego użycia.
			MySqlConnection _conn = new MySqlConnection(CONNECTIONSTRING);
			_conn.Open();
			return _conn;
		}

		/**
		 *  Klasa ma również statyczną metodę CloseConnection, która przyjmuje obiekt MySqlConnection jako parametr i zamyka połączenie z bazą danych. 
		 */
		public static void CloseConnection(MySqlConnection _conn)
		{
			//  Metoda sprawdza, czy obiekt MySqlConnection nie jest null, a jeśli tak, to wypisuje komunikat o błędzie.
			if ( _conn != null )
			{
				_conn.Close();
			}
			else
			{
				Console.WriteLine("MySqlConnection is null!");
			}
		}
	}
}
