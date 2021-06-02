(function () {

    describe("create content dialog",
        function () {

            var scope,
                allowedTypes = [
                    { id: 1, alias: "x" },
                    { id: 2, alias: "y", blueprints: { "1": "a", "2": "b" } }
                ],
                location,
                searcher,
                controller,
                rootScope,
                contentTypeResource;

            beforeEach(module("umbraco"));

            function initialize(blueprintConfig) {
                scope = rootScope.$new();
                scope.currentNode = { id: 1234 };
                var dependencies = {
                    $scope: scope,
                    contentTypeResource: contentTypeResource
                };
                if (blueprintConfig) {
                    dependencies.blueprintConfig = blueprintConfig;
                }
                controller("Umbraco.Editors.Content.CreateController",
                    dependencies);

                scope.$digest();

            }

            beforeEach(inject(function ($controller, $rootScope, $q, $location, authMocks) {

                authMocks.register();

                contentTypeResource = {
                    getAllowedTypes: function () {
                        var def = $q.defer();
                        def.resolve(allowedTypes);
                        return def.promise;
                    }
                };
                location = $location;
                controller = $controller;
                rootScope = $rootScope;

                searcher = { search: function () { } };
                spyOn(location, "path").and.returnValue(searcher)
                spyOn(searcher, "search").and.returnValue(searcher);

                initialize();
            }));

            it("shows available child document types for the given node",
                function () {
                    expect(scope.selectContentType).toBe(true);
                    expect(scope.allowedTypes).toBe(allowedTypes);
                });

            it("creates content directly when there are no blueprints",
                function () {
                    scope.createOrSelectBlueprintIfAny(allowedTypes[0]);

                    expect(location.path).toHaveBeenCalledWith("/content/content/edit/1234");
                    expect(searcher.search).toHaveBeenCalledWith('doctype', 'x');
                    expect(searcher.search).toHaveBeenCalledWith('create', 'true');
                });

            it("shows list of blueprints when there are some",
                function () {
                    scope.createOrSelectBlueprintIfAny(allowedTypes[1]);
                    expect(scope.selectContentType).toBe(false);
                    expect(scope.selectBlueprint).toBe(true);
                    expect(scope.docType).toBe(allowedTypes[1]);
                });

            it("creates blueprint when selected",
                function () {
                    scope.createOrSelectBlueprintIfAny(allowedTypes[1]);
                    scope.createFromBlueprint("1");

                    expect(location.path).toHaveBeenCalledWith("/content/content/edit/1234");
                    expect(searcher.search).toHaveBeenCalledWith("doctype", "y");
                    expect(searcher.search).toHaveBeenCalledWith("create", "true");
                    expect(searcher.search).toHaveBeenCalledWith("blueprintId", "1");
                });

            it("skips selection and creates first blueprint when configured to",
                function () {
                    initialize({
                        allowBlank: true,
                        skipSelect: true
                    });

                    scope.createOrSelectBlueprintIfAny(allowedTypes[1]);

                    expect(location.path).toHaveBeenCalledWith("/content/content/edit/1234");
                    expect(searcher.search).toHaveBeenCalledWith("doctype", "y");
                    expect(searcher.search).toHaveBeenCalledWith("create", "true");
                    expect(searcher.search).toHaveBeenCalledWith("blueprintId", "1");
                });

            it("allows blank to be selected",
                function () {
                    expect(scope.allowBlank).toBe(true);
                });

            it("creates blank when selected",
                function () {
                    scope.createBlank(allowedTypes[1]);

                    expect(location.path).toHaveBeenCalledWith("/content/content/edit/1234");
                    expect(searcher.search).toHaveBeenCalledWith('doctype', 'y');
                    expect(searcher.search).toHaveBeenCalledWith('create', 'true');
                });

            it("hides blank when configured to",
                function () {
                    initialize({
                        allowBlank: false,
                        skipSelect: false
                    });

                    expect(scope.allowBlank).toBe(false);
                });

        });

}());
