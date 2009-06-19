using System;
using System.Collections;
using System.Xml.XPath;

namespace umbraco.presentation.xslt.Exslt
{
	/// <summary>
	/// A list that holds XPathNavigator objects
	/// </summary>
	internal class ExsltNodeList
	{

		#region Private Fields and Properties

		/// <summary>
		/// The inner arraylist used by this class. 
		/// </summary>
		internal ArrayList innerList = new ArrayList();  

		#endregion

		
		#region Public Fields and Properties
		
		/// <summary>
		/// Gets or sets the element at the specified index
		/// </summary>
		public XPathNavigator this[int index] {
			get {return (XPathNavigator) this.innerList[index];}
			set { this.innerList[index] = value; }
		}

		/// <summary>
		/// Gets the number of items in the list
		/// </summary>
		public  int Count {get { return this.innerList.Count;}}

		#endregion
	

		#region Constructors
		
		public ExsltNodeList(){}


		/// <summary>
		/// Initializes the ExsltNodeList with the specified XPathNodeIterator. All nodes 
		/// in the iterator are placed in the list. 
		/// </summary>
		/// <param name="iterator">The iterator to load the nodelist from</param>
		public ExsltNodeList(XPathNodeIterator iterator): this(iterator, false){;}


		
		/// <summary>
		/// Initializes the ExsltNodeList with the specified XPathNodeIterator. All nodes 
		/// in the iterator are placed in the list. 
		/// </summary>
		/// <param name="iterator">The iterator to load the nodelist from</param>
		/// <param name="removeDuplicates">A flag that indicates whether duplicate nodes 
		/// should be loaded into the nodelist or only node with unique identity should 
		/// be added</param>
		public ExsltNodeList(XPathNodeIterator iterator, bool removeDuplicates){
		
			XPathNodeIterator it = iterator.Clone(); 

			while(it.MoveNext()){
				
				if(removeDuplicates){
					if(this.Contains(it.Current)){
						continue; 
					}
				}
				
				this.Add(it.Current.Clone()); 
			}

		}

		#endregion


		#region Public Methods 


		/// <summary>
		/// Returns an enumerator for the entire list.
		/// </summary>
		/// <returns>An enumerator for the entire list</returns>
		 public  IEnumerator GetEnumerator(){
			return this.innerList.GetEnumerator(); 
		 }

		/// <summary>
		/// Adds an item to the list
		/// </summary>
		/// <param name="value">The item to add</param>
		/// <returns>The position into which the new element was inserted</returns>		
		public int Add( XPathNavigator nav){					

			return this.innerList.Add(nav); 
		}


		/// <summary>
		/// Removes all items from the list. 
		/// </summary>
		public void Clear(){
			this.innerList.Clear(); 
		}

		/// <summary>
		/// Determines whether the list contains a navigator positioned at the same 
		/// location as the specified XPathNavigator. This 
		/// method relies on the IsSamePositon() method of the XPathNavightor. 
		/// </summary>
		/// <param name="value">The object to locate in the list.</param>
		/// <returns>true if the object is found in the list; otherwise, false.</returns>
		public bool Contains(XPathNavigator value){
		
			foreach(XPathNavigator nav in this.innerList){
				if(nav.IsSamePosition(value)){
					return true;
				}
			}
			return false; 
		}

		/// <summary>
		/// Determines whether the list contains a navigator whose Value property matches
		/// the target value
		/// </summary>
		/// <param name="value">The value to locate in the list.</param>
		/// <returns>true if the value is found in the list; otherwise, false.</returns>
		public bool ContainsValue(string  value){
		
			foreach(XPathNavigator nav in this.innerList){
				if(nav.Value.Equals(value)){
					return true;
				}
			}
			return false; 
		}

		/// <summary>
		/// Determines the index of a specific item in the list.
		/// </summary>
		/// <param name="value">The object to locate in the list</param>
		/// <returns>The index of value if found in the list; otherwise, -1.</returns>
		public int IndexOf( object value  ){
		
			return this.innerList.IndexOf(value); 
		}

		/// <summary>
		/// Inserts an item to the list at the specified position.
		/// </summary>
		/// <param name="index">The zero-based index at which value should be inserted. </param>
		/// <param name="value">The object to insert into the list</param>		
		public void Insert(int index,XPathNavigator nav ){				

			this.innerList.Insert(index, nav); 
		}


		/// <summary>
		/// Removes the first occurrence of a specific object from the list.
		/// </summary>
		/// <param name="value">The object to remove from the list.</param>
		public void Remove(XPathNavigator nav){
		
			for(int i = 0; i < this.Count; i++){
				if(nav.IsSamePosition((XPathNavigator) this.innerList[i])){
					this.innerList.RemoveAt(i); 
					return; 
				} 
			}
		}

		/// <summary>
		/// Removes the list item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		public void RemoveAt(int index){
		
			this.innerList.RemoveAt(index); 
		}


		#endregion

	}
}
