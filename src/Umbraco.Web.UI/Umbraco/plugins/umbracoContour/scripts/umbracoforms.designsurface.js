var DesignSurface = function() {
  
  this.addEventListener = function(eventName, id, listener) {
      var success = true;
      if (!this.Event[eventName])
        success = false;
      else
        this.Event[eventName][id] = listener;
      return success;
    };
  
    this.removeEventListener = function(eventName, id) {
      delete this.Event[eventName][id];
    };
  
    this.fireEvent = function(eventName) {
      var ev   = this.Event[eventName],
          args = Array.prototype.slice.call(arguments);
      args.shift();
      for (id in ev) {
        ev[id].apply(this, args);
      }
  };
  
  this.Event = [];
  this.Event['AddPage'] = [];
  this.Event['DeletePage'] = [];
  this.Event['UpdatePage'] = [];
  this.Event['UpdatePageSortOrder'] = [];
  this.Event['AddFieldset'] = [];
  this.Event['DeleteFieldset'] = [];
  this.Event['UpdateFieldset'] = [];
  this.Event['UpdateFieldsetSortOrder'] = [];
  this.Event['AddField'] = [];
  this.Event['DeleteField'] = [];
  this.Event['UpdateField'] = [];
  this.Event['UpdateFieldSortOrder'] = [];
  this.Event['AddPrevalue'] = [];
  this.Event['DeletePrevalue'] = [];
  this.Event['SaveDesign'] = [];
  this.Event['ShowAddFieldDialog'] = [];
  this.Event['ShowUpdateFieldDialog'] = [];
  
  this.AddPage = function(Id, Name)
  {
  	this.fireEvent('AddPage', Id, Name);
  };
  
  this.DeletePage = function(Id)
  {
    	this.fireEvent('DeletePage', Id);
  };
  
  this.UpdatePage = function(Id, Name)
  {
      	this.fireEvent('UpdatePage', Id, Name);
  };
  
  this.UpdatePageSortOrder = function(Sortorder)
  {
        this.fireEvent('UpdatePageSortOrder', Sortorder);
  };
  
  this.AddFieldset = function(PageId, Id, Name)
  {
  	this.fireEvent('AddFieldset', PageId, Id, Name);
  };
  
  this.DeleteFieldset = function(Id)
  {
    	this.fireEvent('DeleteFieldset', Id);
  };
  
  this.UpdateFieldsetSortOrder = function(Id, Sortorder)
  {
      	this.fireEvent('UpdateFieldsetSortOrder', Id, Sortorder);
  };  
  
  this.UpdateFieldset = function(Id, Name)
  {
        	this.fireEvent('UpdateFieldset', Id, Name);
  }; 
  
  this.AddField = function(FieldsetId, Id, Name, Type, Mandatory, Regex)
  {
  	this.fireEvent('AddField', FieldsetId, Id, Name, Type, Mandatory, Regex);
  };
  
  this.DeleteField = function(Id)
  {
    	this.fireEvent('DeleteField', Id);
  };
  
  this.UpdateField = function(Id, Name, Type, Mandatory, Regex)
  {
      	this.fireEvent('UpdateField', Id, Name, Type, Mandatory, Regex);
  };   
  
  this.UpdateFieldSortOrder = function(Id, Sortorder)
  {
        	this.fireEvent('UpdateFieldSortOrder', Id, Sortorder);
  };

  this.SaveDesign = function(Guid, Design) 
  {
      this.fireEvent('SaveDesign', Guid, Design);
  };

  this.ShowAddFieldDialog = function() {
    this.fireEvent('ShowAddFieldDialog');
  };

  this.ShowUpdateFieldDialog = function(Field) {
      this.fireEvent('ShowUpdateFieldDialog',Field);
  };  
}




