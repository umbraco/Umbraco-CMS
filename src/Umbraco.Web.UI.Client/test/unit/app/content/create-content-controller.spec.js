(function() {

  describe("create content dialog",
    function() {

      var scope,
        allowedTypes = [{ id: 1, alias: "x" }, { id: 2, alias: "y" }],
        location;

      beforeEach(module("umbraco"));

      beforeEach(inject(function ($controller, $rootScope, $q, $location) {
        var contentTypeResource = {
          getAllowedTypes: function() {
            var def = $q.defer();
            def.resolve(allowedTypes);
            return def.promise;
          }
        }

        location = $location;

        scope = $rootScope.$new();
        scope.currentNode = { id: 1234 };

        $controller("Umbraco.Editors.Content.CreateController", {
          $scope: scope,
          contentTypeResource: contentTypeResource
        });

        scope.$digest();

      }));

    it("shows available child document types for the given node", function() {
      expect(scope.allowedTypes).toBe(allowedTypes);
    });

      it("creates content directly when there are no blueprints",
        function() {
          var searcher = {search:function(){}};
          spyOn(location, "path").andReturn(searcher);
          spyOn(searcher, "search");

          scope.createOrSelectBlueprintIfAny(allowedTypes[0]);

          expect(location.path).toHaveBeenCalledWith("/content/content/edit/1234");
          expect(searcher.search).toHaveBeenCalledWith("doctype=x&create=true");
        });

    });

}());

