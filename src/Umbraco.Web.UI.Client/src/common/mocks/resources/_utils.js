angular.module('umbraco.mocks').
    factory('mocksUtils', ['$cookieStore', function($cookieStore) {
        'use strict';
         
        //by default we will perform authorization
        var doAuth = true;

        return {
            
            getMockDataType: function(id, selectedId) {
                var dataType = {
                    id: id,
                    name: "Simple editor " + id,
                    selectedEditor: selectedId,
                    availableEditors: [
                        { name: "Simple editor 1", editorId: String.CreateGuid() },
                        { name: "Simple editor 2", editorId: String.CreateGuid() },
                        { name: "Simple editor " + id, editorId: selectedId },
                        { name: "Simple editor 4", editorId: String.CreateGuid() },
                        { name: "Simple editor 5", editorId: String.CreateGuid() },
                        { name: "Simple editor 6", editorId: String.CreateGuid() }
                    ],
                    preValues: [
                          {
                              label: "Custom pre value 1 for editor " + selectedId,
                              description: "Enter a value for this pre-value",
                              key: "myPreVal",
                              view: "requiredfield",
                              validation: [
                                  {
                                      type: "Required"
                                  }
                              ]
                          },
                            {
                                label: "Custom pre value 2 for editor " + selectedId,
                                description: "Enter a value for this pre-value",
                                key: "myPreVal",
                                view: "requiredfield",
                                validation: [
                                    {
                                        type: "Required"
                                    }
                                ]
                            }
                    ]

                };
                return dataType;
            },

            /** Creats a mock content object */
            getMockContent: function(id) {
                var node = {
                    name: "My content with id: " + id,
                    updateDate: new Date().toIsoDateTimeString(),
                    publishDate: new Date().toIsoDateTimeString(),
                    createDate: new Date().toIsoDateTimeString(),
                    id: id,
                    parentId: 1234,
                    icon: "icon-file-alt",
                    owner: { name: "Administrator", id: 0 },
                    updater: { name: "Per Ploug Krogslund", id: 1 },

                    tabs: [
                    {
                        label: "Child documents",
                        id: 1, 
                        active: true,
                        properties: [                            
                            { alias: "list", label: "List", view: "listview", value: "", hideLabel: true }
                        ]
                    },
                    {
                        label: "Content",
                        id: 2,
                        properties: [
                            { alias: "valTest", label: "Validation test", view: "validationtest", value: "asdfasdf" },
                            { alias: "bodyText", label: "Body Text", description: "Here you enter the primary article contents", view: "rte", value: "<p>askjdkasj lasjd</p>" },
                            { alias: "textarea", label: "textarea", view: "textarea", value: "ajsdka sdjkds", config: { rows: 4 } },
                            { alias: "map", label: "Map", view: "googlemaps", value: "37.4419,-122.1419", config: { mapType: "ROADMAP", zoom: 4 } },
                            { alias: "media", label: "Media picker", view: "mediapicker", value: "" },
                            { alias: "content", label: "Content picker", view: "contentpicker", value: "1234,23242,23232,23231" }
                        ]
                    },
                    {
                        label: "Sample Editor",
                        id: 3,
                        properties: [
                            { alias: "datepicker", label: "Datepicker", view: "datepicker", config: { pickTime: false, format: "yyyy-MM-dd" } },
                            { alias: "tags", label: "Tags", view: "tags", value: "" }
                        ]
                    },
                    {
                        label: "Grid",
                        id: 4,
                        properties: [
                        { alias: "grid", label: "Grid", view: "grid", value: "test", hideLabel: true }
                        ]
                    }, {
                        label: "Generic Properties",
                        id: 0,
                        properties: [
                            {
                                label: 'Id',
                                value: 1234,
                                view: "readonlyvalue",
                                alias: "_umb_id"
                            },
                            {
                                label: 'Created by',
                                description: 'Original author',
                                value: "Administrator",
                                view: "readonlyvalue",
                                alias: "_umb_createdby"
                            },
                            {
                                label: 'Created',
                                description: 'Date/time this document was created',
                                value: new Date().toIsoDateTimeString(),
                                view: "readonlyvalue",
                                alias: "_umb_createdate"
                            },
                            {
                                label: 'Updated',
                                description: 'Date/time this document was created',
                                value: new Date().toIsoDateTimeString(),
                                view: "readonlyvalue",
                                alias: "_umb_updatedate"
                            },                            
                            {
                                label: 'Document Type',
                                value: "Home page",
                                view: "readonlyvalue",
                                alias: "_umb_doctype" 
                            },
                            {
                                label: 'Publish at',
                                description: 'Date/time to publish this document',
                                value: new Date().toIsoDateTimeString(),
                                view: "datepicker",
                                alias: "_umb_releasedate"
                            },
                            {
                                label: 'Unpublish at',
                                description: 'Date/time to un-publish this document',
                                value: new Date().toIsoDateTimeString(),
                                view: "datepicker",
                                alias: "_umb_expiredate"
                            },
                            {
                                label: 'Template', 
                                value: "{id: 1234, alias: 'myTemplate', name: 'My Template'}",
                                view: "templatepicker",
                                alias: "_umb_template" 
                            },
                            {
                                label: 'Link to document',
                                value: ["/testing" + id, "http://localhost/testing" + id, "http://mydomain.com/testing" + id].join(),
                                view: "urllist",
                                alias: "_umb_template"
                            },
                            {
                                alias: "test", label: "Stuff", view: "test", value: "",
                                config: {
                                    fields: [
                                                { alias: "embedded", label: "Embbeded", view: "textstring", value: "" },
                                                { alias: "embedded2", label: "Embbeded 2", view: "contentpicker", value: "" },
                                                { alias: "embedded3", label: "Embbeded 3", view: "textarea", value: "" },
                                                { alias: "embedded4", label: "Embbeded 4", view: "datepicker", value: "" }
                                    ]
                                }
                            }
                        ]
                    }
                    ]
                };

                return node;
            },

            getMockEntity : function(id){
                return {name: "hello", id: id, icon: "icon-file"};
            },

            /** generally used for unit tests, calling this will disable the auth check and always return true */
            disableAuth: function() {
                doAuth = false;
            },

            /** generally used for unit tests, calling this will enabled the auth check */
            enabledAuth: function() {
                doAuth = true;
            }, 

            /** Checks for our mock auth cookie, if it's not there, returns false */
            checkAuth: function () {
                if (doAuth) {
                    var mockAuthCookie = $cookieStore.get("mockAuthCookie");
                    if (!mockAuthCookie) {
                        return false;
                    }
                    return true;
                }
                else {
                    return true;
                }
            },
            
            /** Creates/sets the auth cookie with a value indicating the user is now authenticated */
            setAuth: function() {
                //set the cookie for loging
                $cookieStore.put("mockAuthCookie", "Logged in!");
            },
            
            /** removes the auth cookie  */
            clearAuth: function() {
                $cookieStore.remove("mockAuthCookie");
            },

            urlRegex: function(url) {
                url = url.replace(/[\-\[\]\/\{\}\(\)\*\+\?\.\\\^\$\|]/g, "\\$&");
                return new RegExp("^" + url);
            },

            getParameterByName: function(url, name) {
                name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");

                var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
                    results = regex.exec(url);

                return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
            },

            getParametersByName: function(url, name) {
                name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");

                var regex = new RegExp(name + "=([^&#]*)", "mg"), results = [];
                var match;

                while ( ( match = regex.exec(url) ) !== null )
                {
                    results.push(decodeURIComponent(match[1].replace(/\+/g, " ")));
                }

                return results;
            }
        };
    }]);
