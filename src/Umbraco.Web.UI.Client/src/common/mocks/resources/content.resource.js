angular.module('umbraco.mocks.resources')
.factory('contentResource', function () {

    var contentArray = [];

    var factory = {
        _cachedItems: contentArray,
        getContent: function (id) {


            if (contentArray[id] !== undefined){
                return contentArray[id];
            }

            var content = {
                name: "My content with id: " + id,
                updateDate: new Date(),
                publishDate: new Date(),
                id: id,
                parentId: 1234,
                icon: "icon-file-alt",
                owner: {name: "Administrator", id: 0},
                updater: {name: "Per Ploug Krogslund", id: 1},

                tabs: [
                {
                    label: "Child documents",
                    alias: "tab00",
					id: 0,
					active: true,
                    properties: [
                    { alias: "list", label: "List", view: "umbraco.listview", value: "", hideLabel: true }
                    ]
                },
                {
                    label: "Content",
                    alias: "tab01",
					id: 1,
                    properties: [
                        { alias: "bodyText", label: "Body Text", description:"Here you enter the primary article contents", view: "umbraco.rte", value: "<p>askjdkasj lasjd</p>" },
                        { alias: "textarea", label: "textarea", view: "umbraco.textarea", value: "ajsdka sdjkds", config: { rows: 4 } },
                        { alias: "map", label: "Map", view: "umbraco.googlemaps", value: "37.4419,-122.1419", config: { mapType: "ROADMAP", zoom: 4 } },
                        { alias: "media", label: "Media picker", view: "umbraco.mediapicker", value: "" },
                        { alias: "content", label: "Content picker", view: "umbraco.contentpicker", value: "" }
                    ]
                },
                {
                    label: "Sample Editor",
                    alias: "tab02",
					id: 2,
                    properties: [
                        { alias: "datepicker", label: "Datepicker", view: "umbraco.datepicker", config: { rows: 7 } },
                        { alias: "tags", label: "Tags", view: "umbraco.tags", value: ""}
                    ]
                },
                {
                    label: "Grid",
                    alias: "tab03",
					id: 3,
                    properties: [
                    { alias: "grid", label: "Grid", view: "umbraco.grid", controller: "umbraco.grid", value: "test", hideLabel: true }
                    ]
                },{
                    label: "WIP",
                    alias: "tab04",
                    id: 4,
                    properties: [
                        { alias: "tes", label: "Stuff", view: "umbraco.test", controller: "umbraco.embeddedcontent", value: "", 
                        
                        config: {
                            fields: [
                                        { alias: "embedded", label: "Embbeded", view: "umbraco.textstring", value: ""},
                                        { alias: "embedded2", label: "Embbeded 2", view: "umbraco.contentpicker", value: ""},
                                        { alias: "embedded3", label: "Embbeded 3", view: "umbraco.textarea", value: ""},
                                        { alias: "embedded4", label: "Embbeded 4", view: "umbraco.datepicker", value: ""}
                                    ] 
                                }
                        }
                    ]
                }


                ]
            };

            // return undefined;

            return content;
        },

        //returns an empty content object which can be persistent on the content service
        //requires the parent id and the alias of the content type to base the scaffold on
        getContentScaffold: function(parentId, alias){

            //use temp storage for now...

            var c = this.getContent(parentId);
            c.name = "empty name";

            $.each(c.tabs, function(index, tab){
                $.each(tab.properties,function(index, property){
                    property.value = "";
                });
            });

            return c;
        },

        getChildren: function(parentId, options){

            if(options === undefined){
                options = {
                    take: 10,
                    offset: 0,
                    filter: ''
                };
            }

            var collection = {take: 10, total: 68, pages: 7, currentPage: options.offset, filter: options.filter};
            collection.total = 56 - (options.filter.length);
            collection.pages = Math.round(collection.total / collection.take);
            collection.resultSet = [];

            if(collection.total < options.take){
                collection.take = collection.total;
            }else{
                collection.take = options.take;
            }


            var _id = 0;
            for (var i = 0; i < collection.take; i++) {
                _id = (parentId + i) * options.offset;
                var cnt = this.getContent(_id);

                //here we fake filtering
                if(options.filter !== ''){
                    cnt.name = options.filter + cnt.name;
                }

                collection.resultSet.push(cnt);
            }

            return collection;
        },

        //saves or updates a content object
        saveContent: function (content) {
            contentArray[content.id] = content;
            //alert("Saved: " + JSON.stringify(content));
        },

        publishContent: function (content) {
            contentArray[content.id] = content;
        }

    };

    return factory;
});
