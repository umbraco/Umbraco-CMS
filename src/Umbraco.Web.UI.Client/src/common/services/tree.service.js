angular.module('umbraco.services')
.factory('treeService', function ($q, treeResource) {
	//implement this in local storage
	var treeArray = [];
	var currentSection = "content";

	/** ensures there's a view and level property on each tree node */
	function ensureLevelAndView(treeNodes, section, level) {
	//if no level is set, then we make it 1   
	var childLevel = (level ? level : 1);
	for (var i = 0; i < treeNodes.length; i++) {
		treeNodes[i].level = childLevel;
		treeNodes[i].view = section + "/edit/" + treeNodes[i].id;
	}
	}

	return {
		getTree: function (options) {

			if(options === undefined){
				options = {};
			}

			var section = options.section || 'content';
			var cacheKey = options.cachekey || '';
			cacheKey += "_" + section;	

			var deferred = $q.defer();

	//return the cache if it exists
	if (treeArray[cacheKey] !== undefined){
		return treeArray[cacheKey];
	}

	treeResource.loadApplication(options)
	.then(function (data) {
	//this will be called once the tree app data has loaded
	var result = {
		name: section,
		alias: section,
		children: data
	};
	//ensure the view is added to each tree node
	ensureLevelAndView(result.children, section);
	//cache this result
	//TODO: We'll need to un-cache this in many circumstances
	treeArray[cacheKey] = result;
	//return the data result as promised
	deferred.resolve(treeArray[cacheKey]);
	}, function (reason) {
	//bubble up the rejection
	deferred.reject(reason);
	return;
	});

	return deferred.promise;
	},

	getActions: function(treeItem, section){
		return [
		{ name: "Create", cssclass: "plus", alias: "create" },

		{ seperator: true, name: "Delete", cssclass: "remove", alias: "delete" },
		{ name: "Move", cssclass: "move",  alias: "move" },
		{ name: "Copy", cssclass: "copy", alias: "copy" },
		{ name: "Sort", cssclass: "sort", alias: "sort" },

		{ seperator: true, name: "Publish", cssclass: "globe", alias: "publish" },
		{ name: "Rollback", cssclass: "undo", alias: "rollback" },

		{ seperator: true, name: "Permissions", cssclass: "lock", alias: "permissions" },
		{ name: "Audit Trail", cssclass: "time", alias: "audittrail" },
		{ name: "Notifications", cssclass: "envelope", alias: "notifications" },

		{ seperator: true, name: "Hostnames", cssclass: "home", alias: "hostnames" },
		{ name: "Public Access", cssclass: "group", alias: "publicaccess" },

		{ seperator: true, name: "Reload", cssclass: "refresh", alias: "users" }
		];
	},	

	getChildActions: function(options){

		if(options === undefined){
			options = {};
		}
		var section = options.section || 'content';
		var treeItem = options.node;

		return [
		{ name: "Create", cssclass: "plus", alias: "create" },

		{ seperator: true, name: "Delete", cssclass: "remove", alias: "delete" },
		{ name: "Move", cssclass: "move",  alias: "move" },
		{ name: "Copy", cssclass: "copy", alias: "copy" },
		{ name: "Sort", cssclass: "sort", alias: "sort" },

		{ seperator: true, name: "Publish", cssclass: "globe", alias: "publish" },
		{ name: "Rollback", cssclass: "undo", alias: "rollback" },

		{ seperator: true, name: "Permissions", cssclass: "lock", alias: "permissions" },
		{ name: "Audit Trail", cssclass: "time", alias: "audittrail" },
		{ name: "Notifications", cssclass: "envelope", alias: "notifications" },

		{ seperator: true, name: "Hostnames", cssclass: "home", alias: "hostnames" },
		{ name: "Public Access", cssclass: "group", alias: "publicaccess" },

		{ seperator: true, name: "Reload", cssclass: "refresh", alias: "users" }
		];
	},

	getChildren: function (options) {

		if(options === undefined){
			throw "No options object defined for getChildren";
		}
		if (options.node === undefined) {
			throw "No node defined on options object for getChildren";
		}

		var section = options.section || 'content';
		var treeItem = options.node;

	//hack to have create as default content action
	var action;
	if(section === "content"){
		action = "create";
	}

	if (!options.node) {
		throw "No node defined";
	}

	var deferred = $q.defer();
	
	treeResource.loadNodes( {section: section, node:treeItem} )
	.then(function (data) {
	//now that we have the data, we need to add the level property to each item and the view
	ensureLevelAndView(data, section, treeItem.level + 1);
	deferred.resolve(data);
	}, function (reason) {
	//bubble up the rejection
	deferred.reject(reason);
	return;
	});

	return deferred.promise;
	}
	};
});