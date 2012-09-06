using System;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;

namespace umbraco.presentation.xslt.Exslt
{
	/// <summary>
	/// This class implements the EXSLT functions in the http://exslt.org/dates-and-times namespace.
	/// </summary>
	public class ExsltDatesAndTimes
	{
		
		/// <summary>
		/// Implements the following function
		///   string date:date-time()
		/// </summary>
		/// <returns>The current time</returns>
		public static string datetime(){		
			return DateTime.Now.ToString("s"); 
		}

		/// <summary>
		/// Implements the following function
		///   string date:date-time()
		/// </summary>
		/// <returns>The current date and time or the empty string if the 
		/// date is invalid </returns>
		public static string datetime(string d){		
			try{
				return DateTime.Parse(d).ToString("s"); 				 
			}catch(FormatException){
				return ""; 
			}
		}

		/// <summary>
		/// Implements the following function
		///   string date:date()
		/// </summary>
		/// <returns>The current date</returns>
		public static string date(){
		  string date = DateTime.Now.ToString("s"); 
		  string[] dateNtime = date.Split(new Char[]{'T'}); 
		  return dateNtime[0]; 
		}

		/// <summary>
		/// Implements the following function
		///   string date:date(string)
		/// </summary>
		/// <returns>The date part of the specified date or the empty string if the 
		/// date is invalid</returns>
		public static string date(string d){
			try{
				string[] dateNtime = DateTime.Parse(d).ToString("s").Split(new Char[]{'T'}); 
				return dateNtime[0]; 
			}catch(FormatException){
			  return ""; 
			}
		}

		/// <summary>
		/// Implements the following function
		///   string date:time()
		/// </summary>
		/// <returns>The current time</returns>
		public static string time(){
			string date = DateTime.Now.ToString("s"); 
			string[] dateNtime = date.Split(new Char[]{'T'}); 
			return dateNtime[1]; 
		}

		/// <summary>
		/// Implements the following function
		///   string date:time(string)
		/// </summary>
		/// <returns>The time part of the specified date or the empty string if the 
		/// date is invalid</returns>
		public static string time(string d){
			try{
				string[] dateNtime = DateTime.Parse(d).ToString("s").Split(new Char[]{'T'}); 
				return dateNtime[1]; 
			}catch(FormatException){
				return ""; 
			}
		}
		

		/// <summary>
		/// Implements the following function
		///   number date:year()
		/// </summary>
		/// <returns>The current year</returns>
		public static double year(){
			return DateTime.Now.Year;
		}

		/// <summary>
		/// Implements the following function
		///   number date:year(string)
		/// </summary>
		/// <returns>The year part of the specified date or the empty string if the 
		/// date is invalid</returns>
		/// <remarks>Does not support dates in the format of the xs:yearMonth or 
		/// xs:gYear types</remarks>
		public static double year(string d){
			try{
				DateTime date = DateTime.Parse(d); 
				return date.Year; 
			}catch(FormatException){
				return System.Double.NaN; 
			}
		}

		/// <summary>
		/// Helper method for calculating whether a year is a leap year. Algorithm 
		/// obtained from http://mindprod.com/jglossleapyear.html
		/// </summary>
		private static bool IsLeapYear ( int year) { 
		
			return CultureInfo.CurrentCulture.Calendar.IsLeapYear(year); 
		}


		/// <summary>
		/// Implements the following function
		///   boolean date:leap-year()
		/// </summary>
		/// <returns>True if the current year is a leap year</returns>
		public static bool leapyear(){
			return IsLeapYear(DateTime.Now.Year);
		}

		/// <summary>
		/// Implements the following function
		///   boolean date:leap-year(string)
		/// </summary>
		/// <returns>True if the specified year is a leap year</returns>
		/// <remarks>Does not support dates in the format of the xs:yearMonth or 
		/// xs:gYear types</remarks>
		public static bool leapyear(string d){
			try{
				DateTime date = DateTime.Parse(d); 
				return IsLeapYear(date.Year); 
			}catch(FormatException){
				return false; 
			}
		}

		/// <summary>
		/// Implements the following function
		///   number date:month-in-year()
		/// </summary>
		/// <returns>The current month</returns>
		public static double monthinyear(){
			return DateTime.Now.Month;
		}

		/// <summary>
		/// Implements the following function
		///   number date:month-in-year(string)
		/// </summary>
		/// <returns>The month part of the specified date or the empty string if the 
		/// date is invalid</returns>
		/// <remarks>Does not support dates in the format of the xs:yearMonth or 
		/// xs:gYear types</remarks>
		public static double monthinyear(string d){
			try{
				DateTime date = DateTime.Parse(d); 
				return date.Month; 
			}catch(FormatException){
				return System.Double.NaN; 
			}
		}

		/// <summary>
		/// Helper method uses local culture information. 
		/// </summary>
		/// <param name="d"></param>
		/// <returns></returns>
		private static double weekinyear(DateTime d){
		Calendar calendar = CultureInfo.CurrentCulture.Calendar; 
		return calendar.GetWeekOfYear(d,CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule, 
										CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek); 
		}

		/// <summary>
		/// Implements the following function
		///   number date:week-in-year()
		/// </summary>
		/// <returns>The current week. This method uses the Calendar.GetWeekOfYear() method 
		/// with the CalendarWeekRule and FirstDayOfWeek of the current culture.
		/// THE RESULTS OF CALLING THIS FUNCTION VARIES ACROSS CULTURES</returns>
		public static double weekinyear(){
			return weekinyear(DateTime.Now);
		}

		/// <summary>
		/// Implements the following function
		///   number date:week-in-year(string)
		/// </summary>
		/// <returns>The week part of the specified date or the empty string if the 
		/// date is invalid</returns>
		/// <remarks>Does not support dates in the format of the xs:yearMonth or 
		/// xs:gYear types. This method uses the Calendar.GetWeekOfYear() method 
		/// with the CalendarWeekRule and FirstDayOfWeek of the current culture.
		/// THE RESULTS OF CALLING THIS FUNCTION VARIES ACROSS CULTURES</remarks>
		public static double weekinyear(string d){
			try{
				DateTime date = DateTime.Parse(d); 
				return weekinyear(date); 
			}catch(FormatException){
				return System.Double.NaN; 
			}
		}


		/// <summary>
		/// Implements the following function
		///   number date:day-in-year()
		/// </summary>
		/// <returns>The current day. </returns>
		public static double dayinyear(){
			return DateTime.Now.DayOfYear;
		}

		/// <summary>
		/// Implements the following function
		///   number date:day-in-year(string)
		/// </summary>
		/// <returns>The day part of the specified date or the empty string if the 
		/// date is invalid</returns>
		public static double dayinyear(string d){
			try{
				DateTime date = DateTime.Parse(d); 
				return date.DayOfYear; 
			}catch(FormatException){
				return System.Double.NaN; 
			}
		}

		/// <summary>
		/// Implements the following function
		///   number date:day-in-week()
		/// </summary>
		/// <returns>The current day in the week. 1=Sunday, 2=Monday,...,7=Saturday</returns>
		public static double dayinweek(){
			return ((int) DateTime.Now.DayOfWeek) + 1;
		}

		/// <summary>
		/// Implements the following function
		///   number date:day-in-week(string)
		/// </summary>
		/// <returns>The day in the week of the specified date or the empty string if the 
		/// date is invalid. <returns>The current day in the week. 1=Sunday, 2=Monday,...,7=Saturday
		/// </returns>
		public static double dayinweek(string d){
			try{
				DateTime date = DateTime.Parse(d); 
				return ((int)date.DayOfWeek) + 1; 
			}catch(FormatException){
				return System.Double.NaN; 
			}
		}


		/// <summary>
		/// Implements the following function
		///   number date:day-in-month()
		/// </summary>
		/// <returns>The current day. </returns>
		public static double dayinmonth(){
			return DateTime.Now.Day;
		}

		/// <summary>
		/// Implements the following function
		///   number date:day-in-month(string)
		/// </summary>
		/// <returns>The day part of the specified date or the empty string if the 
		/// date is invalid</returns>
		/// <remarks>Does not support dates in the format of the xs:MonthDay or 
		/// xs:gDay types</remarks>
		public static double dayinmonth(string d){
			try{
				DateTime date = DateTime.Parse(d); 
				return date.Day; 
			}catch(FormatException){
				return System.Double.NaN; 
			}
		}

		/// <summary>
		/// Helper method.
		/// </summary>
		/// <param name="day"></param>
		/// <returns></returns>
		private static double dayofweekinmonth(int day){
		
		int toReturn = 0;
 
			do{ 
				toReturn++; 
				day-= 7; 
			}while(day > 0);

		return toReturn; 
		}

		/// <summary>
		/// Implements the following function
		///   number date:day-of-week-in-month()
		/// </summary>
		/// <returns>The current day of week in the month as a number. For instance 
		/// the third Tuesday of the month returns 3</returns>
		public static double dayofweekinmonth(){
			return dayofweekinmonth(DateTime.Now.Day);
		}

		/// <summary>
		/// Implements the following function
		///   number date:day-of-week-in-month(string)
		/// </summary>
		/// <returns>The day part of the specified date or the empty string if the 
		/// date is invalid</returns>

		public static double dayofweekinmonth(string d){
			try{
				DateTime date = DateTime.Parse(d); 
				return dayofweekinmonth(date.Day); 
			}catch(FormatException){
				return System.Double.NaN; 
			}
		}
	
		/// <summary>
		/// Implements the following function
		///   number date:hour-in-day()
		/// </summary>
		/// <returns>The current hour of the day as a number.</returns>
		public static double hourinday(){
			return DateTime.Now.Hour;
		}

		/// <summary>
		/// Implements the following function
		///   number date:hour-in-day(string)
		/// </summary>
		/// <returns>The current hour of the specified time or the empty string if the 
		/// date is invalid</returns>

		public static double hourinday(string d){
			try{
				DateTime date = DateTime.Parse(d); 
				return date.Hour; 
			}catch(FormatException){
				return System.Double.NaN; 
			}
		}

		/// <summary>
		/// Implements the following function
		///   number date:minute-in-hour()
		/// </summary>
		/// <returns>The minute of the current hour as a number. </returns>
		public static double minuteinhour(){
			return DateTime.Now.Minute;
		}

		/// <summary>
		/// Implements the following function
		///   number date:minute-in-hour(string)
		/// </summary>
		/// <returns>The minute of the hour of the specified time or the empty string if the 
		/// date is invalid</returns>

		public static double minuteinhour(string d){
			try{
				DateTime date = DateTime.Parse(d); 
				return date.Minute; 
			}catch(FormatException){
				return System.Double.NaN; 
			}
		}


		/// <summary>
		/// Implements the following function
		///   number date:second-in-minute()
		/// </summary>
		/// <returns>The seconds of the current minute as a number. </returns>
		public static double secondinminute(){
			return DateTime.Now.Second;
		}

		/// <summary>
		/// Implements the following function
		///   number date:second-in-minute(string)
		/// </summary>
		/// <returns>The seconds of the minute of the specified time or the empty string if the 
		/// date is invalid</returns>

		public static double secondinminute(string d){
			try{
				DateTime date = DateTime.Parse(d); 
				return date.Second; 
			}catch(FormatException){
				return System.Double.NaN; 
			}
		}


		/// <summary>
		/// Implements the following function
		///   string date:day-name()
		/// </summary>
		/// <returns>The name of the current day</returns>
		public static string dayname(){
			return DateTime.Now.DayOfWeek.ToString();
		}

		/// <summary>
		/// Implements the following function
		///   string date:day-name(string)
		/// </summary>
		/// <returns>The name of the day of the specified date or the empty string if the 
		/// date is invalid</returns>
		public static string dayname(string d){
			try{
				DateTime date = DateTime.Parse(d); 
				return date.DayOfWeek.ToString();
			}catch(FormatException){
				return ""; 
			}
		}

		/// <summary>
		/// Implements the following function
		///   string date:day-abbreviation()
		/// </summary>
		/// <returns>The abbreviated name of the current day</returns>
		public static string dayabbreviation(){
			return DateTime.Now.DayOfWeek.ToString().Substring(0,3);
		}

		/// <summary>
		/// Implements the following function
		///   string date:day-abbreviation(string)
		/// </summary>
		/// <returns>The abbreviated name of the day of the specified date or the empty string if the 
		/// date is invalid</returns>
		public static string dayabbreviation(string d){
			try{
				DateTime date = DateTime.Parse(d); 
				return date.DayOfWeek.ToString().Substring(0,3);
			}catch(FormatException){
				return ""; 
			}
		}


		/// <summary>
		/// Implements the following function
		///   string date:month-name()
		/// </summary>
		/// <returns>The name of the current month</returns>
		public static string monthname(){
			string month = DateTime.Now.ToString("MMMM");
			string[] splitmonth = month.Split(new Char[]{' '});
			return splitmonth[0];
		}

		/// <summary>
		/// Implements the following function
		///   string date:month-name(string)
		/// </summary>
		/// <returns>The name of the month of the specified date or the empty string if the 
		/// date is invalid</returns>
		/// <remarks>Does not support dates in the format of the xs:yearMonth or 
		/// xs:gYear types</remarks>
		public static string monthname(string d){
			try{
				DateTime date = DateTime.Parse(d); 
				string month = date.ToString("MMMM");
				string[] splitmonth = month.Split(new Char[]{' '});
				return splitmonth[0];
			}catch(FormatException){
				return ""; 
			}
		}

		/// <summary>
		/// Implements the following function
		///   string date:month-abbreviation()
		/// </summary>
		/// <returns>The abbreviated name of the current month</returns>
		public static string monthabbreviation(){
			string month = DateTime.Now.ToString("MMM");
			string[] splitmonth = month.Split(new Char[]{' '});
			return splitmonth[0].Substring(0,3); 
		}

		/// <summary>
		/// Implements the following function
		///   string date:month-abbreviation(string)
		/// </summary>
		/// <returns>The abbreviated name of the month of the specified date or the empty string if the 
		/// date is invalid</returns>
		/// <remarks>Does not support dates in the format of the xs:yearMonth or 
		/// xs:gYear types</remarks>
		public static string monthabbreviation(string d){
			try{
				DateTime date = DateTime.Parse(d); 
				string month = date.ToString("MMM");
				string[] splitmonth = month.Split(new Char[]{' '});
				return splitmonth[0].Substring(0, 3);;
			}catch(FormatException){
				return ""; 
			}
		}

		/// <summary>
		/// Implements the following function
		///   string date:format-date(string, string)
		/// </summary>
		/// <param name="date">The date to format</param>
		/// <param name="format">One of the format strings understood by the 
		/// DateTime.ToString(string) method.</param>
		/// <returns>The formated date</returns>
		public static string formatdate(string d, string format){
			try{
				DateTime date = DateTime.Parse(d); 
				return date.ToString(format);
			}catch(FormatException){
				return ""; 
			}
		}


		/// <summary>
		/// Implements the following function
		///   string date:parse-date(string, string)
		/// </summary>
		/// <param name="date">The date to parse</param>
		/// <param name="format">One of the format strings understood by the 
		/// DateTime.ToString(string) method.</param>
		/// <returns>The parsed date</returns>
		public static string parsedate(string d, string format){
			try{
				DateTime date = DateTime.ParseExact(d, format, CultureInfo.CurrentCulture); 
				return XmlConvert.ToString(date, XmlDateTimeSerializationMode.Unspecified);
			}catch(FormatException){
				return ""; 
			}
		}
	
		/// <summary>
		/// Implements the following function 
		///    string:date:difference(string, string)
		/// </summary>
		/// <param name="start">The start date</param>
		/// <param name="end">The end date</param>
		/// <returns>A positive difference if start is before end otherwise a negative
		/// difference. The difference is in the format [-][d.]hh:mm:ss[.ff]</returns>
		public static string difference(string start, string end){
		
			try{
				DateTime startdate = DateTime.Parse(start); 
				DateTime enddate   = DateTime.Parse(end); 
				return XmlConvert.ToString(enddate.Subtract(startdate));
			}catch(FormatException){
				return ""; 
			}
		}

		/// <summary>
		/// Implements the following function
		///    string:date:add(string, string)
		/// </summary>
		/// <param name="datetime">A date/time</param>
		/// <param name="duration">the duration to add</param>
		/// <returns>The new time</returns>
		public static string add(string datetime, string duration){
			
			try{
				DateTime date = DateTime.Parse(datetime); 
				TimeSpan timespan = System.Xml.XmlConvert.ToTimeSpan(duration); 
				return XmlConvert.ToString(date.Add(timespan), XmlDateTimeSerializationMode.Unspecified);
			}catch(FormatException){
				return ""; 
			}

		}


		/// <summary>
		/// Implements the following function
		///    string:date:add-duration(string, string)
		/// </summary>
		/// <param name="datetime">A date/time</param>
		/// <param name="duration">the duration to add</param>
		/// <returns>The new time</returns>
		public static string addduration(string duration1, string duration2){
			
			try{
				TimeSpan timespan1 = XmlConvert.ToTimeSpan(duration1);
				TimeSpan timespan2 = XmlConvert.ToTimeSpan(duration2); 
				return XmlConvert.ToString(timespan1.Add(timespan2));
			}catch(FormatException){
				return ""; 
			}

		}

		/// <summary>
		/// Implements the following function
		///		number date:seconds()
		/// </summary>
		/// <returns>The amount of seconds since the epoch (1970-01-01T00:00:00Z)</returns>
		public static double  seconds(){
		 
			try{
				
			DateTime epoch  = new DateTime(1970, 1, 1, 0,0,0,0, CultureInfo.CurrentCulture.Calendar);
			TimeSpan duration = DateTime.Now.Subtract(epoch); 
			return duration.TotalSeconds; 

			}catch(Exception){
				return System.Double.NaN; 
			} 
		}


		/// <summary>
		/// Implements the following function
		///		number date:seconds(string)
		/// </summary>
		/// <returns>The amount of seconds between the specified date and the 
		/// epoch (1970-01-01T00:00:00Z)</returns>
		public static  double  seconds(string datetime){
		 
			try{
				
				DateTime epoch  = new DateTime(1970, 1, 1, 0,0,0,0, CultureInfo.CurrentCulture.Calendar);
				DateTime date   = DateTime.Parse(datetime);; 
				return date.Subtract(epoch).TotalSeconds; 

			}catch(FormatException){ ; } //might be a duration

			try{
				TimeSpan duration = XmlConvert.ToTimeSpan(datetime); 
				return duration.TotalSeconds;
			}catch(FormatException){
				return System.Double.NaN;
			}
		}

		/// <summary>
		/// Implements the following function 
		///		string date:sum(node-set)
		/// </summary>
		/// <param name="iterator">The nodeset</param>
		/// <returns>The sum of the values within the node set treated as </returns>
		public static string sum(XPathNodeIterator iterator){
			
			TimeSpan sum = new TimeSpan(0,0,0,0); 
 
			if(iterator.Count == 0){
				return ""; 
			}

			try{ 
				while(iterator.MoveNext()){
					sum = XmlConvert.ToTimeSpan(iterator.Current.Value).Add(sum);
				}
				
			}catch(FormatException){
				return ""; 
			}

			return XmlConvert.ToString(sum) ; //XmlConvert.ToString(sum);
			}



		/// <summary>
		/// Implements the following function 
		///    string date:duration(number)
		/// </summary>
		/// <param name="seconds"></param>
		/// <returns></returns>
		public static string duration(double seconds){
		
			return XmlConvert.ToString(new TimeSpan(0,0,(int)seconds)); 
		}


		/// <summary>
		/// Implements the following function 
		///    string date:avg(node-set)
		/// </summary>
		/// <param name="iterator"></param>
		/// <returns></returns>
		/// <remarks>THIS FUNCTION IS NOT PART OF EXSLT!!!</remarks>
		public static string avg(XPathNodeIterator iterator){

			TimeSpan sum = new TimeSpan(0,0,0,0); 
			int count = iterator.Count;

			if(count == 0){
				return ""; 
			}

			try{ 
				while(iterator.MoveNext()){
					sum = XmlConvert.ToTimeSpan(iterator.Current.Value).Add(sum);
				}
				
			}catch(FormatException){
				return ""; 
			}			 

			return duration(sum.TotalSeconds / count); 
		}

		/// <summary>
		/// Implements the following function 
		///    string date:min(node-set)
		/// </summary>
		/// <param name="iterator"></param>
		/// <returns></returns>
		/// <remarks>THIS FUNCTION IS NOT PART OF EXSLT!!!</remarks>
		public static string min(XPathNodeIterator iterator){

			TimeSpan min, t; 

			if(iterator.Count == 0){
				return ""; 
			}

			try{ 

			iterator.MoveNext(); 
			min = XmlConvert.ToTimeSpan(iterator.Current.Value);
			
				while(iterator.MoveNext()){
					t = XmlConvert.ToTimeSpan(iterator.Current.Value);
					min = (t < min)? t : min; 
				}
				
			}catch(FormatException){
				return ""; 
			}		

			return XmlConvert.ToString(min); 
		}

		
		/// <summary>
		/// Implements the following function 
		///    string date:max(node-set)
		/// </summary>
		/// <param name="iterator"></param>
		/// <returns></returns>
		/// <remarks>THIS FUNCTION IS NOT PART OF EXSLT!!!</remarks>
		public static string max(XPathNodeIterator iterator){

			TimeSpan max, t; 

			if(iterator.Count == 0){
				return ""; 
			}
	
			try{ 

			iterator.MoveNext(); 
			max = XmlConvert.ToTimeSpan(iterator.Current.Value);

			
				while(iterator.MoveNext()){
					t = XmlConvert.ToTimeSpan(iterator.Current.Value);
					max = (t > max)? t : max; 
				}
				
			}catch(FormatException){
				return ""; 
			}		

			return XmlConvert.ToString(max); 
		}
	}
}
