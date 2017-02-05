using System;
using System.Text;
using System.Net.Http;
//using System.Net;
//using System.Web;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace geocoding
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			GeoPoint gp = new GeoPoint();
			Boolean hasGeo = false;
			dynamic resultData;
			Console.Clear();
			Console.WriteLine("Welcome to geocoding service.");
			Console.WriteLine("Please enter the country name:");
			String queryCountry = Console.ReadLine();
			Console.WriteLine("Please enter the zipcode:");
			String queryZip  = Console.ReadLine().Trim().Replace(" ", string.Empty); //
			//Console.WriteLine("Zipcode parsing: " + queryZip);

			if (IsZipCode(queryZip))	//verify zipcode
			{
				
				//1. YQL API query, parsing json result.
				try
				{
					//Build YQL query string. 
					//	e.g. https://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20geo.places%20where%20text%3D%22K1P%205R7%20canada%22&format=json&diagnostics=true&callback=
					StringBuilder urlYQL = new StringBuilder();
					urlYQL.Append("https://query.yahooapis.com/v1/public/yql?");
					//urlYQL.Append("q=" + System.Web.HttpUtility.UrlEncode("select * from geo.places where text = \'" + queryZip + ", " + queryCountry + "\'"));
					urlYQL.Append("q=" + System.Net.WebUtility.UrlEncode("select * from geo.places where text = \'" + queryZip + ", " + queryCountry + "\'"));
					urlYQL.Append("&diagnostics=false&format=json");

					resultData = JObject.Parse(download_geo_date(urlYQL.ToString()).Result);
					gp.zipcode = resultData.query.results.place.name;
					gp.country = resultData.query.results.place.country.content;
					gp.provence = resultData.query.results.place.admin1.content;
					gp.city = resultData.query.results.place.admin2.content;
					gp.lat = resultData.query.results.place.centroid.latitude;
					gp.lon = resultData.query.results.place.centroid.longitude;
					//Console.WriteLine("==========   YQL result   =============");
					//Console.WriteLine("zipcode: " + gp.zipcode);
					//Console.WriteLine("country: " + gp.country);
					//Console.WriteLine("provence: " + gp.provence);
					//Console.WriteLine("city: " + gp.city);
					//Console.WriteLine("latitude: " + gp.lat);
					//Console.WriteLine("longitude: " + gp.lon);
					hasGeo = true;
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);

				}
				finally { }//end YQL api

				//2. Google API query, parsing json result.
				if (!hasGeo)
				{
					//Build google maps query string
					//	e.g. http://maps.googleapis.com/maps/api/geocode/json?address=K1P&sensor=true&components=country:canada
					StringBuilder urlGoogle = new StringBuilder();
					urlGoogle.Append("https://maps.googleapis.com/maps/api/geocode/json?");
					//urlGoogle.Append("address=" + System.Web.HttpUtility.UrlEncode(queryZip)
					urlGoogle.Append("address=" + System.Net.WebUtility.UrlEncode(queryZip)

												 + "&components=country:"
												 //+ System.Web.HttpUtility.UrlEncode(queryCountry));
												 + System.Net.WebUtility.UrlEncode(queryCountry));
					urlGoogle.Append("&sensor=true");
					try
					{
						resultData = JObject.Parse(download_geo_date(urlGoogle.ToString()).Result);						
						gp.zipcode = resultData.results[0].address_components[0].short_name;
						gp.country = resultData.results[0].address_components[5].long_name;
						gp.provence = resultData.results[0].address_components[4].long_name;
						gp.city = resultData.results[0].address_components[2].long_name;
						gp.lat = resultData.results[0].geometry.location.lat;
						gp.lon = resultData.results[0].geometry.location.lng;
						//Console.WriteLine("==========   Google result   =============");
						//Console.WriteLine("zipcode: " + gp.zipcode);
						//Console.WriteLine("country: " + gp.country);
						//Console.WriteLine("provence: " + gp.provence);
						//Console.WriteLine("city: " + gp.city);
						//Console.WriteLine("latitude: " + gp.lat);
						//Console.WriteLine("longitude: " + gp.lon);
						hasGeo = true;
					}
					catch (Exception e)
					{
						Console.WriteLine(e.Message);
					}
					finally { }
				}//end google api

				//3. geocoder.ca API query, parsing json result.
				if (!hasGeo)
				{
					//Build geocoder.ca query string. 
					//	e.g. http://geocoder.ca/K1P5R7?json=1
					StringBuilder urlGeocoder = new StringBuilder();
					urlGeocoder.Append("https://geocoder.ca/");
					//urlGoogle.Append(System.Web.HttpUtility.UrlEncode(queryZip + queryCountry));
					urlGeocoder.Append(System.Net.WebUtility.UrlEncode(queryZip + queryCountry));
					urlGeocoder.Append("?json=1");

					try
					{
						resultData = JObject.Parse(download_geo_date(urlGeocoder.ToString()).Result);
						gp.zipcode = resultData.postal;
						gp.prov_code = resultData.standard.prov;
						gp.city = resultData.standard.city;
						gp.lat = resultData.latt;
						gp.lon = resultData.longt;
						//Console.WriteLine("==========   geocoder.ca result   =============");
						//Console.WriteLine("zipcode: " + gp.zipcode);
						//Console.WriteLine("provence code: " + gp.prov_code);
						//Console.WriteLine("city: " + gp.city);
						//Console.WriteLine("latitude: " + gp.lat);
						//Console.WriteLine("longitude: " + gp.lon);
						hasGeo = true;
					}
					catch (Exception e)
					{
						Console.WriteLine(e.Message);
					}
					finally { }
				}//end geocoder.ca api
			}
			else { 
				Console.WriteLine("Invalid Zip Code");
			}

		}//end main

		/// <summary>
		/// get geo infomation by json format. Using HettpClient()
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static async Task<string> download_geo_date(string url)
		{
			var geo_json = string.Empty;

			using(var client = new HttpClient())
			{
				try
				{
					geo_json =  await client.GetStringAsync(url);
					Console.WriteLine(geo_json);
				}catch(HttpRequestException e)
				{
					Console.WriteLine($"Request exception: {e.Message}");
				}
			}
			return geo_json;
		}


		/// <summary>
		/// Validation zip code 
		/// </summary>
		/// <returns><c>true</c>, if usor canadian zip code was ised, <c>false</c> otherwise.</returns>
		/// <param name="zipCode">Zip code.</param>
		public static bool IsZipCode(string zipCode)
		{
			//string _usZipRegEx = @"^\d{5}(?:[-\s]\d{4})?$";
			string _caZipRegEx = @"^([ABCEGHJKLMNPRSTVXY]\d[ABCEGHJKLMNPRSTVWXYZ])\ {0,1}(\d[ABCEGHJKLMNPRSTVWXYZ]\d)$";
			bool validZipCode = true;
			//if ((!Regex.Match(zipCode, _usZipRegEx).Success) && (!Regex.Match(zipCode, _caZipRegEx).Success))
			if (!Regex.Match(zipCode.ToUpper(), _caZipRegEx).Success)	
			{
				validZipCode = false;
			}
			return validZipCode;
		}
	}//end mainclass

}//end namespace


		// /// <summary>
		// /// Downloads the serialized json string. Using WebClient()
		// /// </summary>
		// /// <returns>The serialized json data.</returns>
		// /// <param name="url">URL.</param>
		// public static string download_serialized_json_data(string url)
		// {
		// 	var json_data = string.Empty;// attempt to download JSON data as a string

		// 	using (var w = new WebClient())
		// 	{
		// 		try
		// 		{
		// 			// HTTP GET
		// 			w.UseDefaultCredentials = true;
		// 			json_data = w.DownloadString(url);

		// 		}
		// 		catch (WebException ex)
		// 		{
		// 			// Http Error
		// 			if (ex.Status == WebExceptionStatus.ProtocolError)
		// 			{
		// 				HttpWebResponse wrsp = (HttpWebResponse)ex.Response;
		// 				var statusCode = (int)wrsp.StatusCode;
		// 				var msg = wrsp.StatusDescription;
		// 				throw new HttpException(statusCode, msg);
		// 			}
		// 			else
		// 			{
		// 				throw new HttpException(500, ex.Message);
		// 			}

		// 		}
		// 		return json_data;
		// 	}
		// }