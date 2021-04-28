angular.module('umbraco.mocks').
    factory('mocksUtils', ['$cookies', 'udiService', function ($cookies, udiService) {
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
                              key: "myPreVal1",
                              view: "requiredfield"                              
                          },
                            {
                                label: "Custom pre value 2 for editor " + selectedId,
                                description: "Enter a value for this pre-value",
                                key: "myPreVal2",
                                view: "requiredfield"                                
                            }
                    ]

                };
                return dataType;
            },

            /** Creats a mock content object */
            getMockContent: function (id, key, udi) {
                key = key || String.CreateGuid();
                var udi = udi || udiService.build("content", key);
                var node = {
                    name: "My content with id: " + id,
                    updateDate: new Date().toIsoDateTimeString(),
                    publishDate: new Date().toIsoDateTimeString(),
                    createDate: new Date().toIsoDateTimeString(),
                    id: id,
                    key: key,
                    udi: udi,
                    parentId: 1234,
                    icon: "icon-umb-content",
                    owner: { name: "Administrator", id: 0 },
                    updater: { name: "Per Ploug Krogslund", id: 1 },
                    path: "-1,1234,2455",
                    allowedActions: ["U", "H", "A"],
                    tabs: [
                    {
                        label: "Grid",
                        id: 1, 
                        active: true,
                        properties: [                            
                            { alias: "grid", 
                            label: "Grid", 
                            view: "grid", 
                            value: "", 
                            hideLabel: true, 
                            config: {
                                items:{

                                    styles:[
                                        {
                                            label: "Set a background image",
                                            description: "Set a row background",
                                            key: "background-image",
                                            view: "imagepicker",
                                            modifier: "url({0})"
                                        }
                                    ],
                                    config:[
                                        {
                                            label: "Class",
                                            description: "Set a css class",
                                            key: "class",
                                            view: "textstring"
                                        }
                                    ],
                                    columns: 12,
                                    templates:[
                                        {
                                            name: "1 column layout",
                                            sections: [
                                                {
                                                    grid: 12
                                                }
                                            ]
                                        },
                                        {
                                            name: "2 column layout",
                                            sections: [
                                                {
                                                    grid: 4
                                                },
                                                {
                                                    grid: 8
                                                }
                                            ]
                                        }
                                    ],


                                    layouts:[
                                        {
                                            name: "Headline",
                                            areas: [
                                                {
                                                    grid: 12,
                                                    editors: ["headline"]
                                                }
                                            ]
                                        },
                                        {
                                            name: "Article",
                                            areas: [
                                                {
                                                    grid: 12
                                                },
                                                {
                                                    grid: 4
                                                },
                                                {
                                                    grid: 8
                                                },
                                                {
                                                    grid: 12
                                                }
                                            ]
                                        }
                                    ]
                                    }

                                }
                            }
                        ]
                    },
                    {
                        label: "Content",
                        id: 2,
                        properties: [
                            { alias: "valTest", label: "Validation test", view: "validationtest", value: "asdfasdf" },
                            { alias: "bodyText", label: "Body Text", description: "Here you enter the primary article contents", view: "rte", value: "<p>askjdkasj lasjd</p>", config: {} },
                            { alias: "textarea", label: "textarea", view: "textarea", value: "ajsdka sdjkds", config: { rows: 4 } },
                            { alias: "media", label: "Media picker", view: "mediapicker", value: "1234,23242,23232,23231", config: {multiPicker: 1} }
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
                        label: "This",
                        id: 4,
                        properties: [
                            { alias: "valTest4", label: "Validation test", view: "validationtest", value: "asdfasdf" },
                            { alias: "bodyText4", label: "Body Text", description: "Here you enter the primary article contents", view: "rte", value: "<p>askjdkasj lasjd</p>", config: {} },
                            { alias: "textarea4", label: "textarea", view: "textarea", value: "ajsdka sdjkds", config: { rows: 4 } },
                            { alias: "content4", label: "Content picker", view: "contentpicker", value: "1234,23242,23232,23231" }
                        ]
                    },
                    {
                        label: "Is",
                        id: 5,
                        properties: [
                            { alias: "valTest5", label: "Validation test", view: "validationtest", value: "asdfasdf" },
                            { alias: "bodyText5", label: "Body Text", description: "Here you enter the primary article contents", view: "rte", value: "<p>askjdkasj lasjd</p>", config: {} },
                            { alias: "textarea5", label: "textarea", view: "textarea", value: "ajsdka sdjkds", config: { rows: 4 } },
                            { alias: "content5", label: "Content picker", view: "contentpicker", value: "1234,23242,23232,23231" }
                        ]
                    },
                    {
                        label: "Overflown",
                        id: 6,
                        properties: [
                            { alias: "valTest6", label: "Validation test", view: "validationtest", value: "asdfasdf" },
                            { alias: "bodyText6", label: "Body Text", description: "Here you enter the primary article contents", view: "rte", value: "<p>askjdkasj lasjd</p>", config: {} },
                            { alias: "textarea6", label: "textarea", view: "textarea", value: "ajsdka sdjkds", config: { rows: 4 } },
                            { alias: "content6", label: "Content picker", view: "contentpicker", value: "1234,23242,23232,23231" }
                        ]
                    },
                    {
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
                                value: "myTemplate",
                                view: "dropdown",
                                alias: "_umb_template",
                                config: {
                                    items: {
                                        "" : "-- Choose template --",
                                        "myTemplate" : "My Templates",
                                        "home" : "Home Page",
                                        "news" : "News Page"
                                    }
                                }
                            },
                            {
                                label: 'Link to document',
                                value: ["/testing" + id, "http://localhost/testing" + id, "http://mydomain.com/testing" + id].join(),
                                view: "urllist",
                                alias: "_umb_urllist"
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


            /** Creats a mock variant content object */
            getMockVariantContent: function(id, key, udi) {
                key = key || String.CreateGuid();
                var udi = udi || udiService.build("content", key);
                var node = {
                    name: "My content with id: " + id,
                    updateDate: new Date().toIsoDateTimeString(),
                    publishDate: new Date().toIsoDateTimeString(),
                    createDate: new Date().toIsoDateTimeString(),
                    id: id,
                    key: key,
                    udi: udi,
                    parentId: 1234,
                    icon: "icon-umb-content",
                    owner: { name: "Administrator", id: 0 },
                    updater: { name: "Per Ploug Krogslund", id: 1 },
                    path: "-1,1234,2455",
                    allowedActions: ["U", "H", "A"],
                    contentTypeAlias: "testAlias", 
                    contentTypeKey: "7C5B74D1-E2F9-45A3-AE4B-FC7A829BF8AB", 
                    apps: [],
                    variants: [
                        {
                            name: "",
                            language: null,
                            segment: null,
                            state: "NotCreated",
                            updateDate: "0001-01-01 00:00:00",
                            createDate: "0001-01-01 00:00:00",
                            publishDate: null,
                            releaseDate: null,
                            expireDate: null,
                            notifications: [],
                            tabs: [
                                {
                                    label: "Content",
                                    id: 2,
                                    properties: [
                                        { alias: "testproperty", label: "Test property", view: "textbox", value: "asdfghjk" },
                                        { alias: "valTest", label: "Validation test", view: "validationtest", value: "asdfasdf" },
                                        { alias: "bodyText", label: "Body Text", description: "Here you enter the primary article contents", view: "rte", value: "<p>askjdkasj lasjd</p>", config: {} },
                                        { alias: "textarea", label: "textarea", view: "textarea", value: "ajsdka sdjkds", config: { rows: 4 } },
                                        { alias: "media", label: "Media picker", view: "mediapicker", value: "1234,23242,23232,23231", config: {multiPicker: 1} }
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
                                    label: "This",
                                    id: 4,
                                    properties: [
                                        { alias: "valTest4", label: "Validation test", view: "validationtest", value: "asdfasdf" },
                                        { alias: "bodyText4", label: "Body Text", description: "Here you enter the primary article contents", view: "rte", value: "<p>askjdkasj lasjd</p>", config: {} },
                                        { alias: "textarea4", label: "textarea", view: "textarea", value: "ajsdka sdjkds", config: { rows: 4 } },
                                        { alias: "content4", label: "Content picker", view: "contentpicker", value: "1234,23242,23232,23231" }
                                    ]
                                },
                                {
                                    label: "Is",
                                    id: 5,
                                    properties: [
                                        { alias: "valTest5", label: "Validation test", view: "validationtest", value: "asdfasdf" },
                                        { alias: "bodyText5", label: "Body Text", description: "Here you enter the primary article contents", view: "rte", value: "<p>askjdkasj lasjd</p>", config: {} },
                                        { alias: "textarea5", label: "textarea", view: "textarea", value: "ajsdka sdjkds", config: { rows: 4 } },
                                        { alias: "content5", label: "Content picker", view: "contentpicker", value: "1234,23242,23232,23231" }
                                    ]
                                },
                                {
                                    label: "Overflown",
                                    id: 6,
                                    properties: [
                                        { alias: "valTest6", label: "Validation test", view: "validationtest", value: "asdfasdf" },
                                        { alias: "bodyText6", label: "Body Text", description: "Here you enter the primary article contents", view: "rte", value: "<p>askjdkasj lasjd</p>", config: {} },
                                        { alias: "textarea6", label: "textarea", view: "textarea", value: "ajsdka sdjkds", config: { rows: 4 } },
                                        { alias: "content6", label: "Content picker", view: "contentpicker", value: "1234,23242,23232,23231" }
                                    ]
                                },
                                {
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
                                            value: "myTemplate",
                                            view: "dropdown",
                                            alias: "_umb_template",
                                            config: {
                                                items: {
                                                    "" : "-- Choose template --",
                                                    "myTemplate" : "My Templates",
                                                    "home" : "Home Page",
                                                    "news" : "News Page"
                                                }
                                            }
                                        },
                                        {
                                            label: 'Link to document',
                                            value: ["/testing" + id, "http://localhost/testing" + id, "http://mydomain.com/testing" + id].join(),
                                            view: "urllist",
                                            alias: "_umb_urllist"
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
                    var mockAuthCookie = $cookies.get("mockAuthCookie");
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
                $cookies.put("mockAuthCookie", "Logged in!");
            },
            
            /** removes the auth cookie  */
            clearAuth: function() {
                $cookies.remove("mockAuthCookie");
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
            },

            getObjectPropertyFromJsonString: function(data, name) {
                var obj = JSON.parse(data);
                return obj[name];
            }
        };
    }]);
