using System;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Maps
{
	public sealed class MapSpan
	{
		const double EarthRadiusKm = 6371;
		const double EarthCircumferenceKm = EarthRadiusKm * 2 * Math.PI;
		const double MinimumRangeDegrees = 0.001 / EarthCircumferenceKm * 360; // 1 meter

		public MapSpan(Location center, double latitudeDegrees, double longitudeDegrees)
		{
			Center = center;
			LatitudeDegrees = Math.Min(Math.Max(latitudeDegrees, MinimumRangeDegrees), 90.0);
			LongitudeDegrees = Math.Min(Math.Max(longitudeDegrees, MinimumRangeDegrees), 180.0);
		}

		public MapSpan ClampLatitude(double north, double south)
		{
			north = Math.Min(Math.Max(north, 0), 90);
			south = Math.Max(Math.Min(south, 0), -90);
			double lat = Math.Max(Math.Min(Center.Latitude, north), south);
			double maxDLat = Math.Min(north - lat, -south + lat) * 2;
			return new MapSpan(new Location(lat, Center.Longitude), Math.Min(LatitudeDegrees, maxDLat), LongitudeDegrees);
		}

		public Location Center { get; }

		public double LatitudeDegrees { get; }

		public double LongitudeDegrees { get; }

		public Distance Radius
		{
			get
			{
				double latKm = LatitudeDegreesToKm(LatitudeDegrees);
				double longKm = LongitudeDegreesToKm(Center, LongitudeDegrees);
				return new Distance(1000 * Math.Min(latKm, longKm) / 2);
			}
		}

		public MapSpan WithZoom(double zoomFactor)
		{
			double maxDLat = Math.Min(90 - Center.Latitude, 90 + Center.Latitude) * 2;
			return new MapSpan(Center, Math.Min(LatitudeDegrees / zoomFactor, maxDLat), LongitudeDegrees / zoomFactor);
		}

		static double DistanceToLatitudeDegrees(Distance distance)
		{
			return distance.Kilometers / EarthCircumferenceKm * 360;
		}

		static double DistanceToLongitudeDegrees(Location location, Distance distance)
		{
			double latCircumference = LatitudeCircumferenceKm(location);
			return distance.Kilometers / latCircumference * 360;
		}

		bool Equals(MapSpan other)
		{
			return Center.Equals(other.Center) && LongitudeDegrees.Equals(other.LongitudeDegrees) && LatitudeDegrees.Equals(other.LatitudeDegrees);
		}

		static double LatitudeCircumferenceKm(Location location)
		{
			return EarthCircumferenceKm * Math.Cos(location.Latitude * Math.PI / 180.0);
		}

		static double LatitudeDegreesToKm(double latitudeDegrees)
		{
			return EarthCircumferenceKm * latitudeDegrees / 360;
		}

		static double LongitudeDegreesToKm(Location location, double longitudeDegrees)
		{
			double latCircumference = LatitudeCircumferenceKm(location);
			return latCircumference * longitudeDegrees / 360;
		}
	}
}