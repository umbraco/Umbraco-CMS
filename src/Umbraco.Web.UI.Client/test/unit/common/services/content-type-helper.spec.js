describe('contentTypeHelper tests', function () {
  var contentTypeHelper, contentTypeResource, dataTypeResource, filter, injector, q;

  beforeEach(module('umbraco.services'));
  beforeEach(module('umbraco.mocks'));
  beforeEach(module('ngRoute'));

  beforeEach(inject(function ($injector, localizationMocks) {
      localizationMocks.register();

      contentTypeHelper = $injector.get('contentTypeHelper');
      contentTypeResource = $injector.get('contentTypeResource');
      dataTypeResource = $injector.get('dataTypeResource');
      filter = $injector.get('$filter');
      injector = $injector.get('$injector');
      q = $injector.get('$q');
  }));

  describe('generateLocalAlias', function () {

      it('should generate an alias when given a name', function () {
          var alias = contentTypeHelper.generateLocalAlias('my cool name');
          expect(alias).toBe('myCoolName');
      });

      it('should generate a guid if not given a name', function () {
        var regex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-5][0-9a-f]{3}-[089ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
        var alias = contentTypeHelper.generateLocalAlias();
        const match = regex.test(alias)
        expect(match).toBeTrue();
      });

  });

  describe('getLocalAlias', function () {

    it('should get group alias', function () {
        var alias = contentTypeHelper.getLocalAlias('groupAlias');
        expect(alias).toBe('groupAlias');
    });

    it('should get group alias inside a tab', function () {
      var alias = contentTypeHelper.getLocalAlias('tabAlias/groupAlias');
      expect(alias).toBe('groupAlias');
    });
    
  });

  describe('updateLocalAlias', function () {

    it('it should update local alias', function () {
      const group = { alias: 'group' };
      const newAlias = contentTypeHelper.updateLocalAlias(group.alias, 'newGroupAlias');
      expect(newAlias).toBe('newGroupAlias');
    });

    it('it should update alias on a group when inside a tab', function () {
      const group = { alias: 'tab/group' };
      const newAlias = contentTypeHelper.updateLocalAlias(group.alias, 'newGroupAlias');
      expect(newAlias).toBe('tab/newGroupAlias');
    });
    
  });

  describe('getParentAlias', function () {

    it('should return null if no parent', function () {
        var parentAlias = contentTypeHelper.getParentAlias('groupAlias');
        expect(parentAlias).toBeNull();
    });

    it('should return parent alias', function () {
      var parentAlias = contentTypeHelper.getParentAlias('tabAlias/groupAlias');
      expect(parentAlias).toBe('tabAlias');
    });
    
  });

  describe('updateParentAlias', function () {

    it('it should update parent alias', function () {
      const group = { alias: 'tab/group' };
      const newAlias = contentTypeHelper.updateParentAlias(group.alias, 'newTabAlias');
      expect(newAlias).toBe('newTabAlias/group');
    });

    it('it should add parent alias to alias if it doesnt exist', function () {
      const group = { alias: 'group' };
      const newAlias = contentTypeHelper.updateParentAlias(group.alias, 'newTabAlias');
      expect(newAlias).toBe('newTabAlias/group');
    });
    
  });

  describe('defineParentAliasOnGroups', function () {

    it('it should set parent alias on a group based on the alias', function () {
      var groups = [
        { alias: 'group' },
        { alias: 'tab/group' }
      ];
  
      contentTypeHelper.defineParentAliasOnGroups(groups);
  
      expect(groups[0].parentAlias).toBeNull();
      expect(groups[1].parentAlias).toBe('tab');
    });

  });

  describe('relocateDisorientedGroups', function () {

    it('should remove parentAlias from groups where the tab doesnt exist', function () {

      const groups = [
        { alias: 'group', parentAlias: null, type: 0 },
        { alias: 'tab/group', parentAlias: 'tab', type: 0 },
        { alias: 'tab', parentAlias: null, type: 1 },
        { alias: 'notExistingTab/group', parentAlias: 'notExistingTab', type: 0 }
      ];

      contentTypeHelper.relocateDisorientedGroups(groups);

      expect(groups[0].parentAlias).toBeNull();
      expect(groups[1].parentAlias).toBe('tab');
      expect(groups[2].parentAlias).toBeNull();
      expect(groups[3].parentAlias).toBeNull();
    });

  });

});
