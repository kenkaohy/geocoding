﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
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
			DelaySec();
			Console.WriteLine("Welcome to geocoding service.");
			Console.WriteLine("Please enter the country name:");
			String queryCountry = Console.ReadLine();
			Console.WriteLine("Please enter the zipcode:");
			String queryZip  = Console.ReadLine().Trim().Replace(" ", string.Empty); //
			
			//Verify zipcode
			if (IsZipCode(queryZip))	
			{
				//Query YQL 1st if no rsult then google if no result then geocoder.ca.
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
					gp.errmsg = "";
					Console.WriteLine("==========   YQL result   =============");
					Console.WriteLine("zipcode: " + gp.zipcode);
					Console.WriteLine("country: " + gp.country);
					Console.WriteLine("provence: " + gp.provence);
					Console.WriteLine("city: " + gp.city);
					Console.WriteLine("latitude: " + gp.lat);
					Console.WriteLine("longitude: " + gp.lon);
					hasGeo = true;
				}
				catch (Exception e)
				{
					gp.errmsg = e.Message.ToString();
					Console.WriteLine(e.Message);
				}//end YQL api

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
						gp.errmsg = "";
						Console.WriteLine("==========   Google result   =============");
						Console.WriteLine("zipcode: " + gp.zipcode);
						Console.WriteLine("country: " + gp.country);
						Console.WriteLine("provence: " + gp.provence);
						Console.WriteLine("city: " + gp.city);
						Console.WriteLine("latitude: " + gp.lat);
						Console.WriteLine("longitude: " + gp.lon);
						hasGeo = true;
					}
					catch (Exception e)
					{
						gp.errmsg = e.Message.ToString();
						Console.WriteLine(e.Message);
					}
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
						gp.errmsg = "";
						Console.WriteLine("==========   geocoder.ca result   =============");
						Console.WriteLine("zipcode: " + gp.zipcode);
						Console.WriteLine("provence code: " + gp.prov_code);
						Console.WriteLine("city: " + gp.city);
						Console.WriteLine("latitude: " + gp.lat);
						Console.WriteLine("longitude: " + gp.lon);
						hasGeo = true;
					}
					catch (Exception e)
					{
						gp.errmsg = e.Message.ToString();
						Console.WriteLine(e.Message);
					}
				}//end geocoder.ca api
			}
			//zip code is Invalid.
			else { 
				gp.errmsg = "nvalid Zip Code";
				Console.WriteLine("Invalid Zip Code");
			}

		}//end main

		/// <summary>
		/// get geographical infomation by json format.
		/// </summary>
		/// <param name="url"></param>
		/// <returns>geo_json</returns>
		public static async Task<string> download_geo_date(string url)
		{
			var geo_json = string.Empty;

			using(var client = new HttpClient())
			{
				try
				{
					geo_json =  await client.GetStringAsync(url);
					//Console.WriteLine(geo_json);
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
			string _usZipRegEx = @"(^\d{5}(-\d{4})?$)|(^[ABCEGHJKLMNPRSTVXY]{1}\d{1}[A-Z]{1} *\d{1}[A-Z]{1}\d{1}$)";
			string _caZipRegEx =  @"^[ABCEGHJKLMNPRSTVXY]{1}\d{1}[A-Z]{1} *\d{1}[A-Z]{1}\d{1}$";
			bool validZipCode = true;
			//if ((!Regex.Match(zipCode, _usZipRegEx).Success) && (!Regex.Match(zipCode, _caZipRegEx).Success))
			if (!Regex.Match(zipCode.ToUpper(), _caZipRegEx).Success)	
			{
				validZipCode = false;
			}
			return validZipCode;
		}
		
		/// <summary>
		/// Delay timmer to prevent web API denny
		/// </summary>
		/// <returns></returns>
		public static async Task DelaySec()
		{
			await Task.Run(async () => 
			{
				Console.WriteLine("Start Delay");
				await Task.Delay(1000);
				Console.WriteLine("End Delay");
			});
		}

		/// <summary>
		/// SQLite database insert example.
		/// </summary>
		/// <param name="GeoList"></param>
		public static void updateSQLite(List<GeoPoint> GeoList)
		{
            using (var db = new GeoContext())
            {
				foreach (var gp in GeoList)
				{
					var Postcode = new Postcode() 
					{ 
						country_code = gp.country,
						postal_code = gp.zipcode,
						place_name = gp.zipcode,
						state = gp.zipcode,
						state_code = gp.zipcode,
						county = gp.zipcode,
						county_code = gp.country,
						latitude = gp.lat.ToString(),
						longitude = gp.lon.ToString(),
						//accuracy = gp.zipcode,				
					};
					db.Postcodes.Add(Postcode);
				}
							
                //db.Postcodes.Add(new Postcode { Url = "http://blogs.msdn.com/adonet" });
                var count = db.SaveChanges();
                Console.WriteLine("{0} records saved to database", count);
                Console.WriteLine("All blogs in database:");
                // foreach (var pc in db.Blogs)
                // {
                //     Console.WriteLine(" - {0}", blog.Url);
                // }
            }
		}

	}//end mainclass
}//end namespace