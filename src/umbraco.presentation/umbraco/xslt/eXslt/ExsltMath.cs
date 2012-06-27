using System;
using System.Xml;
using System.Xml.XPath;

namespace umbraco.presentation.xslt.Exslt
{
	/// <summary>
	/// This class implements the EXSLT functions in the http://exslt.org/math namespace.
	/// </summary>
	public class ExsltMath {
		/// <summary>
		/// Implements the following function 
		///    number min(node-set)
		/// </summary>
		/// <param name="iterator"></param>
		/// <returns></returns>		
		public static double min(XPathNodeIterator iterator){

			double min, t; 

			if(iterator.Count == 0){
				return Double.NaN; 
			}

			try{ 

				iterator.MoveNext(); 
				min = XmlConvert.ToDouble(iterator.Current.Value);

			
				while(iterator.MoveNext()){
					t = XmlConvert.ToDouble(iterator.Current.Value);
					min = (t < min)? t : min; 
				}
				
			}catch(Exception){
				return Double.NaN; 
			}		

			return min; 
		}

		
		/// <summary>
		/// Implements the following function 
		///    number max(node-set)
		/// </summary>
		/// <param name="iterator"></param>
		/// <returns></returns>		
		public static double max(XPathNodeIterator iterator){

			double max, t; 

			if(iterator.Count == 0){
				return Double.NaN; 
			}

			try{ 

				iterator.MoveNext(); 
				max = XmlConvert.ToDouble(iterator.Current.Value);

			
				while(iterator.MoveNext()){
					t = XmlConvert.ToDouble(iterator.Current.Value);
					max = (t > max)? t : max; 
				}
				
			}catch(Exception){
				return Double.NaN; 
			}		

			return max; 
		}
		

		/// <summary>
		/// Implements the following function 
		///    node-set highest(node-set)
		/// </summary>
		/// <param name="iterator">The input nodeset</param>
		/// <returns>All the nodes that contain the max value in the nodeset</returns>		
		public static XPathNodeIterator highest(XPathNodeIterator iterator){

			ExsltNodeList newList = new ExsltNodeList(); 
			double max, t; 

			if(iterator.Count == 0){
				return ExsltCommon.ExsltNodeListToXPathNodeIterator(newList);  
			}


			try{ 

				iterator.MoveNext(); 
				max = XmlConvert.ToDouble(iterator.Current.Value);
				newList.Add(iterator.Current.Clone()); 

				while (iterator.MoveNext()){
					t = XmlConvert.ToDouble(iterator.Current.Value);
				
					if(t > max){
						max =  t;
						newList.Clear(); 
						newList.Add(iterator.Current.Clone()); 
					}else if( t == max){
						newList.Add(iterator.Current.Clone()); 
					}
				}
				
			}catch(Exception){ //return empty node set
				newList.Clear(); 
				return ExsltCommon.ExsltNodeListToXPathNodeIterator(newList);  
			}

			return ExsltCommon.ExsltNodeListToXPathNodeIterator(newList); 
		}


		/// <summary>
		/// Implements the following function 
		///    node-set lowest(node-set)
		/// </summary>
		/// <param name="iterator">The input nodeset</param>
		/// <returns>All the nodes that contain the min value in the nodeset</returns>		
		public static XPathNodeIterator lowest(XPathNodeIterator iterator){

			ExsltNodeList newList = new ExsltNodeList(); 
			double min, t; 

			if(iterator.Count == 0){
				return ExsltCommon.ExsltNodeListToXPathNodeIterator(newList);  
			}


			try{ 

				iterator.MoveNext(); 
				min = XmlConvert.ToDouble(iterator.Current.Value);
				newList.Add(iterator.Current.Clone()); 

				while (iterator.MoveNext()){
					t = XmlConvert.ToDouble(iterator.Current.Value);
				
					if(t < min){
						min =  t;
						newList.Clear(); 
						newList.Add(iterator.Current.Clone()); 
					}else if( t == min){
						newList.Add(iterator.Current.Clone()); 
					}
				}
				
			}catch(Exception){ //return empty node set
				newList.Clear(); 
				return ExsltCommon.ExsltNodeListToXPathNodeIterator(newList);  
			}

			return ExsltCommon.ExsltNodeListToXPathNodeIterator(newList); 
		}

		/// <summary>
		///  Implements the following function 
		///     number abs(number)
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
		public static double abs(double number){

			return Math.Abs(number); 
		}	

		/// <summary>
		///  Implements the following function 
		///     number sqrt(number)
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
		public static double sqrt(double number){

			return Math.Sqrt(number); 
		}

		/// <summary>
		///  Implements the following function 
		///     number power(number, number)
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public double power(double x, double y){

			return Math.Pow(x, y); 
		}

		/// <summary>
		///  Implements the following function 
		///     number log(number)
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public static double log(double x){

			return Math.Log(x); 
		}

		/// <summary>
		///  Implements the following function 
		///     number constant(string, number)
		/// </summary>
		/// <param name="number"></param>
		/// <returns>The specified constant or NaN</returns>
		/// <remarks>This method only supports the constants 
		/// E and PI. Also the precision parameter is ignored.</remarks>
		public static double constant(string c, double precision){

			switch(c){
				
				case "E":
						return Math.E;

				case "PI":
						return Math.PI; 

				default:
						return Double.NaN;			
			}
		}

		/// <summary>
		///  Implements the following function 
		///     number random()
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public static double random(){

			Random rand = new Random((int) DateTime.Now.Ticks); 
			return rand.NextDouble(); 
		}

		/// <summary>
		///  Implements the following function 
		///     number sin(number)
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public static double sin(double x){

			return Math.Sin(x); 
		}

		/// <summary>
		///  Implements the following function 
		///     number asin(number)
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public static double asin(double x){

			return Math.Asin(x); 
		}


		/// <summary>
		///  Implements the following function 
		///     number cos(number)
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public static double cos(double x){

			return Math.Cos(x); 
		}

		/// <summary>
		///  Implements the following function 
		///     number acos(number)
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public static double acos(double x){

			return Math.Acos(x); 
		}

		/// <summary>
		///  Implements the following function 
		///     number tan(number)
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public static double tan(double x){

			return Math.Tan(x); 
		}

		/// <summary>
		///  Implements the following function 
		///     number atan(number)
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public static double atan(double x){

			return Math.Atan(x); 
		}

		/// <summary>
		///  Implements the following function 
		///     number atan2(number, number)
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public static double atan2(double x, double y){

			return Math.Atan2(x,y); 
		}

		/// <summary>
		///  Implements the following function 
		///     number exp(number)
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public static double exp(double x){

			return Math.Exp(x); 
		}


		/// <summary>
		/// Implements the following function 
		///    number avg(node-set)
		/// </summary>
		/// <param name="iterator"></param>
		/// <returns>The average of all the value of all the nodes in the 
		/// node set</returns>
		/// <remarks>THIS FUNCTION IS NOT PART OF EXSLT!!!</remarks>
		public static double avg(XPathNodeIterator iterator){

			double sum = 0; 
			int count = iterator.Count;

			if(count == 0){
				return Double.NaN; 
			}

			try{ 
				while(iterator.MoveNext()){
					sum += XmlConvert.ToDouble(iterator.Current.Value);
				}
				
			}catch(FormatException){
				return Double.NaN; 
			}			 

			return sum / count; 
		}
	}
}

