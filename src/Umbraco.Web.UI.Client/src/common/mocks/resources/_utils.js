angular.module('umbraco.mocks').
    factory('mocksUtils', ['$cookieStore', function($cookieStore) {
        'use strict';
         
        //by default we will perform authorization
        var doAuth = true;

        return {
            
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
                            { alias: "bodyText", label: "Body Text", description: "Here you enter the primary article contents", view: "rte", value: "<p>askjdkasj lasjd</p>" },
                            { alias: "textarea", label: "textarea", view: "textarea", value: "ajsdka sdjkds", config: { rows: 4 } },
                            { alias: "map", label: "Map", view: "googlemaps", value: "37.4419,-122.1419", config: { mapType: "ROADMAP", zoom: 4 } },
                            { alias: "media", label: "Media picker", view: "mediapicker", value: "" },
                            { alias: "content", label: "Content picker", view: "contentpicker", value: "" }
                        ]
                    },
                    {
                        label: "Sample Editor",
                        id: 3,
                        properties: [
                            { alias: "datepicker", label: "Datepicker", view: "datepicker", config: { rows: 7 } },
                            { alias: "tags", label: "Tags", view: "tags", value: "" }
                        ]
                    },
                    {
                        label: "Grid",
                        id: 4,
                        properties: [
                        { alias: "grid", label: "Grid", view: "grid", controller: "umbraco.grid", value: "test", hideLabel: true }
                        ]
                    }, {
                        label: "Generic Properties",
                        id: 0,
                        properties: [
                            {
                                alias: "tes", label: "Stuff", view: "test", controller: "umbraco.embeddedcontent", value: "",

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
                return results == null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
            }
        };
    }]);