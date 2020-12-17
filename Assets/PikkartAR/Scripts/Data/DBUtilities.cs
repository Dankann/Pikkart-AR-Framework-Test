using SQLite4Unity3d;
using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

namespace PikkartAR
{
	/// <summary>
	/// Low level interaction with local database.
	/// Handles database creation, connection and queries.
	/// </summary>
	public class DBUtilities  {
		
		private SQLiteConnection _connection;

        #region INIT
        /// <summary>
        /// Initializes a new instance of the <see cref="PikkartAR.DBUtilities"/> class.
        /// </summary>
        /// <param name="DatabaseName">Database name.</param>
        public DBUtilities(string DatabaseName = Constants.DB_NAME)
        {
            _connection = new SQLiteConnection(Application.persistentDataPath + "/" + DatabaseName, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);

			_connection.CreateTable<Marker>();
            _connection.CreateTable<MarkerDatabase>();
		}
		
		/// <summary>
		/// Gets the database connection.
		/// </summary>
		/// <returns>The connection.</returns>
		public SQLiteConnection GetDB()
		{
			return _connection;
		}
		#endregion

		#region UPDATE_LAST_ACCESS_DATE
		/// <summary>
		/// Updates the last access date of a Marker.
		/// </summary>
		/// <param name="obj">Marker to update.</param>
		public void UpdateLastAccessDate (Marker obj)
		{
			obj.lastAccessDate = DateTime.Now.ToUniversalTime();
			_connection.Update (obj);
		}


        /// <summary>
        /// Updates the last access date of a MarkerDatabase.
        /// </summary>
        /// <param name="obj">MarkerDatabase to update.</param>
        public void UpdateLastAccessDate(MarkerDatabase obj)
        {
            obj.lastAccessDate = DateTime.Now.ToUniversalTime();
            _connection.Update(obj);

        }
		#endregion

		#region METODI GENERICI
		/// <summary>
		/// Generic function for INSERT.
		/// </summary>
		/// <param name="obj">Object.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void Insert<T> (T obj){
			_connection.Insert (obj);
		}
		
		/// <summary>
		/// Inserts or replace.
		/// </summary>
		/// <param name="obj">Object.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void InsertOrReplace<T> (T obj){
			_connection.InsertOrReplace (obj);
		}
		
		/// <summary>
		/// Delete the specified id.
		/// </summary>
		/// <param name="id">Identifier.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void Delete<T> (string id){
			_connection.Delete<T> (id);
		}
		
		/// <summary>
		/// Delete the specified obj.
		/// </summary>
		/// <param name="obj">Object.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void Delete<T> (T obj)
		{
			_connection.Delete<T> (obj);
		}
		
		/// <summary>
		/// Bulk INSERT.
		/// </summary>
		/// <param name="objs">Objects.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void InsertAll<T> (IEnumerable<T> objs)
		{
			_connection.InsertAll (objs);
		}
		#endregion

		#region MARKERS

		public List<Marker> GetMarkers() {
			string query = "SELECT * FROM Markers";
			return _connection.Query<Marker>(query, new string[]{});
		}

		public void DeleteObsoleteMarkers() {
			string query = "DELETE FROM Markers WHERE publishedTo < '" + DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd hh:mm:ss") + "' and publishedTo is not null";
			_connection.Query<Marker>(query, new string[]{});
		}

        #endregion
        
        public List<Marker> MarkerQuery(string query)
        {
            return _connection.Query<Marker>(query, new string[] { });
        }
        public List<MarkerDatabase> MarkerDBQuery(string query)
        {
            return _connection.Query<MarkerDatabase>(query, new string[] { });
        }
    }
}