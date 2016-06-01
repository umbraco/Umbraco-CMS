(function () {
    "use strict";

    function PackagesRepoController($scope, $route, $location) {

        var vm = this;

        vm.packageViewState = "packageList";
        vm.pagination = {
            pageNumber: 1,
            totalPages: 10
        };

        vm.selectCategory = selectCategory;
        vm.showPackageDetails = showPackageDetails;
        vm.setPackageViewState = setPackageViewState;
        vm.nextPage = nextPage;
        vm.prevPage = prevPage;
        vm.goToPage = goToPage;

        vm.categories = [
            {
                "id": 1,
                "icon": "icon-male-and-female",
                "name": "All",
                "active": true
            },
            {
                "icon": "icon-male-and-female",
                "name": "Collaboration"
            },
            {
                "id": 2,
                "icon": "icon-molecular-network",
                "name": "Backoffice extensions"
            },
            {
                "id": 3,
                "icon": "icon-brackets",
                "name": "Developer tools"
            },
            {
                "id": 4,
                "icon": "icon-wand",
                "name": "Starter kits"
            },
            {
                "id": 5,
                "icon": "icon-medal",
                "name": "Umbraco Pro"
            },
            {
                "id": 6,
                "icon": "icon-wrench",
                "name": "Website utilities"
            }
        ];

        vm.packages = [
            {
                "id": 1,
                "name": "uSightly",
                "description": "An HTML5 audio player based on jPlayer",
                "karma": "1",
                "downloads": "1672",
                "icon":"https://our.umbraco.org/media/wiki/150283/635768313097111400_usightlylogopng.png?bgcolor=fff&height=154&width=281&format=png"
            },
            {
                "id": 2,
                "name": "Kill IE6",
                "description": "A simple port of the IE6 warning script (http://code.google.com/p/ie6-upgrade-warning/) to use in your Umbraco websites.",
                "karma": "11",
                "downloads": "688",
                "icon":"https://our.umbraco.org/media/wiki/9138/634697622367666000_offroadcode-100x100.png?bgcolor=fff&height=154&width=281&format=png"
            },
            {
                "id": 3,
                "name": "Examine Media Indexer",
                "description": "CogUmbracoExamineMediaIndexer",
                "karma": "3",
                "downloads": "1329",
                "icon":"https://our.umbraco.org/media/wiki/50703/634782902373558000_cogworks.jpg?bgcolor=fff&height=154&width=281&format=png"
            },
            {
                "id": 4,
                "name": "SVG Icon Picker",
                "description": "A picker, for picking icons from an SVG spritesheet.",
                "karma": "5",
                "downloads": "8",
                "icon":"https://our.umbraco.org/media/wiki/154472/635997115126742822_logopng.png?bgcolor=fff&height=154&width=281&format=png"
            },
            {
                "id": 5,
                "name": "Pipeline CRM",
                "description": "Pipeline is a social CRM that lives in Umbraco back-office. It tracks opportunities and helps teams collaborate with timelines and tasks. It stores information about your customers and your interactions with them. It integrates with your website, capturing opportunities from forms and powering personal portals.",
                "karma": "3",
                "downloads": "105",
                "icon":"https://our.umbraco.org/media/wiki/152476/635917291068518788_pipeline-crm-logopng.png?bgcolor=fff&height=154&width=281&format=png"
            },
            {
                "id": 6,
                "name": "uSightly",
                "description": "An HTML5 audio player based on jPlayer",
                "karma": "1",
                "downloads": "1672",
                "icon":"https://our.umbraco.org/media/wiki/150283/635768313097111400_usightlylogopng.png?bgcolor=fff&height=154&width=281&format=png"
            },
            {
                "id": 7,
                "name": "Kill IE6",
                "description": "A simple port of the IE6 warning script (http://code.google.com/p/ie6-upgrade-warning/) to use in your Umbraco websites.",
                "karma": "11",
                "downloads": "688",
                "icon":"https://our.umbraco.org/media/wiki/9138/634697622367666000_offroadcode-100x100.png?bgcolor=fff&height=154&width=281&format=png"
            },
            {
                "id": 8,
                "name": "Examine Media Indexer",
                "description": "CogUmbracoExamineMediaIndexer",
                "karma": "3",
                "downloads": "1329",
                "icon":"https://our.umbraco.org/media/wiki/50703/634782902373558000_cogworks.jpg?bgcolor=fff&height=154&width=281&format=png"
            },
            {
                "id": 9,
                "name": "SVG Icon Picker",
                "description": "A picker, for picking icons from an SVG spritesheet.",
                "karma": "5",
                "downloads": "8",
                "icon":"https://our.umbraco.org/media/wiki/154472/635997115126742822_logopng.png?bgcolor=fff&height=154&width=281&format=png"
            },
            {
                "id": 10,
                "name": "Pipeline CRM",
                "description": "Pipeline is a social CRM that lives in Umbraco back-office. It tracks opportunities and helps teams collaborate with timelines and tasks. It stores information about your customers and your interactions with them. It integrates with your website, capturing opportunities from forms and powering personal portals.",
                "karma": "3",
                "downloads": "105",
                "icon":"https://our.umbraco.org/media/wiki/152476/635917291068518788_pipeline-crm-logopng.png?bgcolor=fff&height=154&width=281&format=png"
            }
        ];

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

        function selectCategory(selectedCategory, categories) {
            for (var i = 0; i < categories.length; i++) {
                var category = categories[i];
                category.active = false;
            }
            selectedCategory.active = true;
        }

        function showPackageDetails(selectedPackage) {
            vm.packageViewState = "packageDetails";
        }

        function setPackageViewState(state) {
            if(state) {
                vm.packageViewState = state;
            }
        }

        function nextPage(pageNumber) {
            console.log(pageNumber);
        }

        function prevPage(pageNumber) {
            console.log(pageNumber);
        }

        function goToPage(pageNumber) {
            console.log(pageNumber);
        }

    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.RepoController", PackagesRepoController);

})();
