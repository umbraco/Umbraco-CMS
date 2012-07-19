	using System;
	using System.Xml.XPath; 
	using System.Xml;



namespace umbraco.presentation.xslt.Exslt
{
		/// <summary>
		///   This class implements the EXSLT functions in the http://exslt.org/sets namespace.
		/// </summary>		
			public class ExsltSets {
				
				/// <summary>
				/// Implements the following function 
				///    node-set subset(node-set, node-set) 
				/// </summary>
				/// <param name="nodeset1">An input nodeset</param>
				/// <param name="nodeset2">Another input nodeset</param>
				/// <returns>True if all the nodes in the first nodeset are contained 
				/// in the second nodeset</returns>
				/// <remarks>THIS FUNCTION IS NOT PART OF EXSLT!!!</remarks>
				public static bool subset(XPathNodeIterator nodeset1, XPathNodeIterator nodeset2){
				
					ExsltNodeList nodelist1 = new ExsltNodeList(nodeset1, true);
					ExsltNodeList nodelist2 = new ExsltNodeList(nodeset2, true);

					foreach(XPathNavigator nav in nodelist1){
						if(!nodelist2.Contains(nav)){
							return false; 
						}
					}
					

					return true; 
				}

				/// <summary>
				/// Implements the following function 
				///    node-set difference(node-set, node-set) 
				/// </summary>
				/// <param name="nodeset1">An input nodeset</param>
				/// <param name="nodeset2">Another input nodeset</param>
				/// <returns>The those nodes that are in the node set 
				/// passed as the first argument that are not in the node set 
				/// passed as the second argument.</returns>
				public static XPathNodeIterator difference(XPathNodeIterator nodeset1, XPathNodeIterator nodeset2){
							
					ExsltNodeList nodelist1 = new ExsltNodeList(nodeset1, true);
					ExsltNodeList nodelist2 = new ExsltNodeList(nodeset2);
 
					 
					for(int i = 0; i < nodelist1.Count; i++){ 
						
						XPathNavigator nav = nodelist1[i]; 
					
						if(nodelist2.Contains(nav)){
							nodelist1.RemoveAt(i); 
							i--; 
						}
					} 

					return ExsltCommon.ExsltNodeListToXPathNodeIterator(nodelist1); 
				}

				/// <summary>
				/// Implements the following function 
				///    node-set distinct(node-set)
				/// </summary>
				/// <param name="nodeset">The input nodeset</param>
				/// <returns>Returns the nodes in the nodeset whose string value is 
				/// distinct</returns>
				public static XPathNodeIterator distinct(XPathNodeIterator nodeset){
				
					ExsltNodeList nodelist = new ExsltNodeList();

					while(nodeset.MoveNext()){
						if(!nodelist.ContainsValue(nodeset.Current.Value)){
							nodelist.Add(nodeset.Current.Clone()); 
						}					
					}
					 
					return ExsltCommon.ExsltNodeListToXPathNodeIterator(nodelist); 

				}

				/// <summary>
				/// Implements 
				///    boolean hassamenode(node-set, node-set)
				/// </summary>
				/// <param name="nodeset1"></param>
				/// <param name="nodeset2"></param>
				/// <returns>true if both nodeset contain at least one of the same node</returns>
				public static bool hassamenode(XPathNodeIterator nodeset1, XPathNodeIterator nodeset2){
				
					ExsltNodeList nodelist1 = new ExsltNodeList(nodeset1, true);
					ExsltNodeList nodelist2 = new ExsltNodeList(nodeset2, true);

					foreach(XPathNavigator nav in nodelist1){
						if(nodelist2.Contains(nav)){
							return true; 
						}
					}

					return false; 
				}

				/// <summary>
				/// Implements the following function 
				///   node-set intersection(node-set, node-set)
				/// </summary>
				/// <param name="?"></param>
				/// <returns></returns>
				public static XPathNodeIterator intersection(XPathNodeIterator nodeset1, XPathNodeIterator nodeset2){
				
					ExsltNodeList nodelist1 = new ExsltNodeList(nodeset1, true);
					ExsltNodeList nodelist2 = new ExsltNodeList(nodeset2);
 
					 
					for(int i = 0; i < nodelist1.Count; i++){ 
						
						XPathNavigator nav = nodelist1[i]; 
					
						if(!nodelist2.Contains(nav)){
							nodelist1.RemoveAt(i); 
							i--; 
						}
					} 

					return ExsltCommon.ExsltNodeListToXPathNodeIterator(nodelist1); 

				}

				/// <summary>
				/// Implements the following function 
				///		node-set leading(node-set, node-set)
				/// </summary>
				/// <param name="nodeset1"></param>
				/// <param name="nodeset2"></param>
				/// <returns>returns the nodes in the node set passed as the 
				/// first argument that precede, in document order, the first node 
				/// in the node set passed as the second argument</returns>
				public static XPathNodeIterator leading(XPathNodeIterator nodeset1, XPathNodeIterator nodeset2){
				
					XPathNavigator leader = null; 
					
					if(nodeset2.MoveNext()){
						leader = nodeset2.Current; 
					}else{ 
						return nodeset1; 
					}

					ExsltNodeList nodelist1 = new ExsltNodeList();

					while(nodeset1.MoveNext()){
						if(nodeset1.Current.ComparePosition(leader) == XmlNodeOrder.Before){
							nodelist1.Add(nodeset1.Current.Clone()); 
						}					
					}

					return ExsltCommon.ExsltNodeListToXPathNodeIterator(nodelist1);  
			}

				/// <summary>
				/// Implements the following function 
				///		node-set trailing(node-set, node-set)
				/// </summary>
				/// <param name="nodeset1"></param>
				/// <param name="nodeset2"></param>
				/// <returns>returns the nodes in the node set passed as the 
				/// first argument that follow, in document order, the first node 
				/// in the node set passed as the second argument</returns>
				public static XPathNodeIterator trailing(XPathNodeIterator nodeset1, XPathNodeIterator nodeset2){
				
					XPathNavigator leader = null; 
					
					if(nodeset2.MoveNext()){
						leader = nodeset2.Current; 
					}else{ 
						return nodeset1; 
					}

					ExsltNodeList nodelist1 = new ExsltNodeList();

					while(nodeset1.MoveNext()){
						if(nodeset1.Current.ComparePosition(leader) == XmlNodeOrder.After){
							nodelist1.Add(nodeset1.Current.Clone()); 
						}					
					}

					return ExsltCommon.ExsltNodeListToXPathNodeIterator(nodelist1);  
				}
		}

	}
