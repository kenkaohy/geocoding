public class GeoPoint
{

	public string zipcode { get; set; }
	public string city { get; set; }
	public string provence { get; set; }
	public string prov_code { get; set; }//gonly for eocoder.ca
	public string country { get; set; }
	public double lat { get; set; } //latitute
	public double lon { get; set; } //longtitue
	public string errmsg { get; set; }

}