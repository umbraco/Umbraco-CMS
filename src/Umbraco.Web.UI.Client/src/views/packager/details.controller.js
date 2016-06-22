(function () {
    "use strict";

    function PackageDetailsController($scope, $routeParams) {

        var vm = this;

        vm.page = {};

        vm.package = {
            "name": "Merchello",
            "description": "&lt;p&gt;Merchello is a high performance, designer friendly, open source Umbraco ecommerce package built for the store owner.&lt;/p&gt; &lt;p&gt;&lt;strong&gt;What Merchello does for you&lt;/strong&gt;&lt;/p&gt; &lt;p&gt;In version 1, Merchello supports a large variety of products with options that can be attached to a single warehouse, processes orders, manages taxes and shipping, and sends out email notifications to your customers. The beauty of Merchello is that while it oversees all of your products, orders, and store settings, it allows Umbraco to maintain your content. This seamless integration gives you the flexibility to build your store in any way imagineable on a robust platform capable of handling a wide variety of store sizes.&lt;/p&gt; &lt;p&gt;&lt;strong&gt;Find out more on our website&lt;/strong&gt;&lt;/p&gt; &lt;p&gt;&lt;strong&gt;&lt;a href=&quot;https://merchello.com&quot;&gt;https://merchello.com&lt;/a&gt;&lt;/strong&gt;&lt;/p&gt; &lt;p&gt;&lt;strong&gt;Contribute&lt;/strong&gt;&lt;/p&gt; &lt;p&gt;We would love and need your help. If you want to contribute to Merchello's core, the easiest way to get started is to fork the project on https://github.com/merchello/Merchello and open src/Merchello.sln in Visual Studio. We're excited to see what you do!&lt;/p&gt; &lt;p&gt;&lt;strong&gt;Starter Kit&lt;/strong&gt;&lt;/p&gt; &lt;p&gt;We have built a simple starter kit for Merchello called Bazaar, and you can download it below in the package files tab.&lt;/p&gt;",
            "compatibility": [
                {
                    "version": "7.4.x",
                    "percentage": "100"
                },
                {
                    "version": "7.3.x",
                    "percentage": "86"
                },
                {
                    "version": "7.2.x",
                    "percentage": "93"
                },
                {
                    "version": "7.1.x",
                    "percentage": "100"
                }
            ],
            "information": {
                "owner": "Rusty Swayne",
                "ownerAvatar": "https://our.umbraco.org/media/upload/d476d257-a494-46d9-9a00-56c2f94a55c8/our-profile.jpg?width=200&height=200&mode=crop",
                "ownerKarma": "2673",
                "contributors": [
                    {
                        "name": "Lee"
                    },
                    {
                        "name": "Jason Prothero"
                    }
                ],
                "created": "18/12/2013",
                "currentVersion": "2.0.0",
                "netVersion": "4.5",
                "license": "MIT",
                "downloads": "4198",
                "karma": "53"
            },
            "externalSources": [
                {
                    "name": "Source code",
                    "url": "https://github.com/merchello/Merchello"
                },
                {
                    "name": "Issue tracker",
                    "url": "http://issues.merchello.com/youtrack/oauth?state=%2Fyoutrack%2FrootGo"
                }
            ],
            "images": [
                {
                    "thumbnail": "https://our.umbraco.org/media/wiki/104946/635591947547374885_Product-Listpng.png?bgcolor=fff&height=154&width=281&format=png",
                    "source": "https://our.umbraco.org/media/wiki/104946/635591947547374885_Product-Listpng.png"
                },
                {
                    "thumbnail": "https://our.umbraco.org/media/wiki/104946/635591947547374885_Product-Listpng.png?bgcolor=fff&height=154&width=281&format=png",
                    "source": "https://our.umbraco.org/media/wiki/104946/635591947547374885_Product-Listpng.png"
                },
                {
                    "thumbnail": "https://our.umbraco.org/media/wiki/104946/635591947547374885_Product-Listpng.png?bgcolor=fff&height=154&width=281&format=png",
                    "source": "https://our.umbraco.org/media/wiki/104946/635591947547374885_Product-Listpng.png"
                },
                {
                    "thumbnail": "https://our.umbraco.org/media/wiki/104946/635591947547374885_Product-Listpng.png?bgcolor=fff&height=154&width=281&format=png",
                    "source": "https://our.umbraco.org/media/wiki/104946/635591947547374885_Product-Listpng.png"
                },
                {
                    "thumbnail": "https://our.umbraco.org/media/wiki/104946/635591947547374885_Product-Listpng.png?bgcolor=fff&height=154&width=281&format=png",
                    "source": "https://our.umbraco.org/media/wiki/104946/635591947547374885_Product-Listpng.png"
                },
                {
                    "thumbnail": "https://our.umbraco.org/media/wiki/104946/635591947547374885_Product-Listpng.png?bgcolor=fff&height=154&width=281&format=png",
                    "source": "https://our.umbraco.org/media/wiki/104946/635591947547374885_Product-Listpng.png"
                },
                {
                    "thumbnail": "https://our.umbraco.org/media/wiki/104946/635591947547374885_Product-Listpng.png?bgcolor=fff&height=154&width=281&format=png",
                    "source": "https://our.umbraco.org/media/wiki/104946/635591947547374885_Product-Listpng.png"
                },
                {
                    "thumbnail": "https://our.umbraco.org/media/wiki/104946/635591947547374885_Product-Listpng.png?bgcolor=fff&height=154&width=281&format=png",
                    "source": "https://our.umbraco.org/media/wiki/104946/635591947547374885_Product-Listpng.png"
                }
            ]
        };

    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.DetailsController", PackageDetailsController);

})();
