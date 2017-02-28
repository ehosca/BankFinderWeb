using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web;
using System.Web.Hosting;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IBankFinder" in both code and config file together.
public interface IBankFinder
{
    Bank[] GetBanksForZipcode(string zipcode, int radius);
}

[ServiceContract]
[AspNetCompatibilityRequirements(
    RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
public class BankFinder : IBankFinder
{
    [OperationContract]
    [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json)]
    public Bank[] GetBanksForZipcode(string zipcode, int radius)
    {
        if (radius >= 25)
            radius = 25;
        var zip = GetAllZipCodes().FirstOrDefault(z => z.Zip.Equals(zipcode));
        if (zip != null)
            return
                GetAllBanks()
                    .Where(b => b.CalculateDistanceFrom(zip.Latitude, zip.Longitude) < (double) radius)
                    .ToArray();
        throw new ArgumentException("Invalid Zipcode !");
    }

    private List<Bank> GetAllBanks()
    {
        if (HttpRuntime.Cache["banks"] == null)
            HttpRuntime.Cache.Insert("banks",
                File.ReadAllLines(Path.Combine(HostingEnvironment.MapPath("~/App_Data/"), "banks.txt"))
                    .Select(l => new
                    {
                        l,
                        i = l.Split(',')
                    }).Select(param0 => new Bank
                    {
                        Name = param0.i[0],
                        Address = param0.i[1],
                        City = param0.i[2],
                        State = param0.i[3],
                        ZipCode = param0.i[4],
                        Latitude = double.Parse(param0.i[5]),
                        Longitude = double.Parse(param0.i[6]),
                        WebAddress = param0.i[7],
                        FullAddress =
                            string.Format("{0}, {1}, {2} {3}", (object) param0.i[1], (object) param0.i[2],
                                (object) param0.i[3], (object) param0.i[4])
                    }).ToList(), null, DateTime.Now.AddHours(6.0), TimeSpan.Zero);
        return HttpRuntime.Cache["banks"] as List<Bank>;
    }

    private List<ZipCode> GetAllZipCodes()
    {
        if (HttpRuntime.Cache["zipcodes"] == null)
            HttpRuntime.Cache.Insert("zipcodes",
                File.ReadAllLines(Path.Combine(HostingEnvironment.MapPath("~/App_Data/"), "zipcodes.txt"))
                    .Select(l => new
                    {
                        l,
                        i = l.Split(',')
                    }).Select(param0 => new ZipCode
                    {
                        Zip = param0.i[0],
                        Latitude = Convert.ToDouble(param0.i[1]),
                        Longitude = Convert.ToDouble(param0.i[2])
                    }).ToList(), null, DateTime.Now.AddHours(6.0), TimeSpan.Zero);
        return HttpRuntime.Cache["zipcodes"] as List<ZipCode>;
    }
}

public class ZipCode
{
    public string Zip { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }
}

[DataContract]
public class Bank
{

    private readonly Func<double, double, double, double, double> _calcDistance =
        (lat1, lon1, lat2, lon2) =>
            7912.0 *
            Math.Asin(Math.Min(1.0,
                Math.Sqrt(Math.Pow(Math.Sin(DiffRadian(lat1, lat2) / 2.0), 2.0) +
                          Math.Cos(ToRadian(lat1)) * Math.Cos(ToRadian(lat2)) *
                          Math.Pow(Math.Sin(DiffRadian(lon1, lon2) / 2.0), 2.0))));

    [DataMember]
    public string Name { get; set; }

    [DataMember]
    public string Address { get; set; }

    [DataMember]
    public string City { get; set; }

    [DataMember]
    public string State { get; set; }

    [DataMember]
    public string ZipCode { get; set; }

    [DataMember]
    public double Latitude { get; set; }

    [DataMember]
    public double Longitude { get; set; }

    [DataMember]
    public string WebAddress { get; set; }

    [DataMember]
    public string FullAddress { get; set; }

    private static double ToRadian(double val)
    {
        return val * (Math.PI / 180.0);
    }

    private static double DiffRadian(double val1, double val2)
    {
        return ToRadian(val2) - ToRadian(val1);
    }

    public double CalculateDistanceFrom(double fromLat, double fromLon)
    {
        return _calcDistance(Latitude, Longitude, fromLat, fromLon);
    }
}