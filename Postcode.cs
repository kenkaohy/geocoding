
using System.Collections.Generic;
public class Postcode{
    public int ID{get;set;}
    public string country_code{get;set;}
    public string postal_code{get;set;}
    public string place_name{get;set;}
    public string state{get;set;}
    public string state_code{get;set;}
    public string county{get;set;}
    public string county_code{get;set;}
    public string latitude{get;set;}
    public string longitude{get;set;}
    public string accuracy{get;set;}

    public List<Postcode> Postcodes {get;set;}
}