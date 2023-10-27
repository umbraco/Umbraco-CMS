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

  describe('Group type', function () {
    it('should return the group type', function () {
        var groupType = contentTypeHelper.TYPE_GROUP;
        expect(groupType).toBe('Group');
    });
  });

  describe('Tab type', function () {
    it('should return the tab type', function () {
        var tabType = contentTypeHelper.TYPE_TAB;
        expect(tabType).toBe('Tab');
    });
  });

  describe('isAliasUnique', function () {
    const groups = [{ alias: 'alias' }];

    it('should return true when alias is unique', function () {
        var isUnique = contentTypeHelper.isAliasUnique(groups, 'uniqueAlias');
        expect(isUnique).toBe(true);
    });

    it('should return false when alias is not unique', function () {
      var isUnique = contentTypeHelper.isAliasUnique(groups, 'alias');
      expect(isUnique).toBe(false);
    });
  });

  describe('createUniqueAlias', function () {

    it('should generate a unique alias', function () {
        const groups = [{ alias: 'alias' }, { alias: 'otherAlias' }, { alias: 'otherAlias1' }];

        var alias = contentTypeHelper.createUniqueAlias(groups, 'alias');
        expect(alias).toBe('alias1');
        
        alias = contentTypeHelper.createUniqueAlias(groups, 'otherAlias');
        expect(alias).toBe('otherAlias2');
    });

  });

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
        { alias: 'group', parentAlias: null, type: contentTypeHelper.TYPE_GROUP },
        { alias: 'tab/group', parentAlias: 'tab', type: contentTypeHelper.TYPE_GROUP },
        { alias: 'tab', parentAlias: null, type: contentTypeHelper.TYPE_TAB },
        { alias: 'notExistingTab/group', parentAlias: 'notExistingTab', type: contentTypeHelper.TYPE_GROUP }
      ];

      contentTypeHelper.relocateDisorientedGroups(groups);

      expect(groups[0].parentAlias).toBeNull();
      expect(groups[1].parentAlias).toBe('tab');
      expect(groups[2].parentAlias).toBeNull();
      expect(groups[3].parentAlias).toBeNull();
    });

  });

  describe('convertGroupToTab', function () {

    it('should convert group to tab', function () {
      const groups = [
        { type: contentTypeHelper.TYPE_GROUP, alias: 'hero', name: 'Hero' },
        { type: contentTypeHelper.TYPE_GROUP, alias: 'content' },
        { type: contentTypeHelper.TYPE_GROUP, alias: 'footer' }
      ];

      const newTab = groups[0];

      contentTypeHelper.convertGroupToTab(groups, newTab);

      expect(newTab.type).toBe(contentTypeHelper.TYPE_TAB);
      expect(newTab.alias).toBe('hero');
      expect(newTab.parentAlias).toBeNull();
    });

    it('should set sort order to 0 if it is the first tab', function () {
      const groups = [
        { type: contentTypeHelper.TYPE_GROUP, alias: 'hero', name: 'Hero' }
      ];
      
      const newTab = groups[0];
      contentTypeHelper.convertGroupToTab(groups, newTab);

      expect(newTab.sortOrder).toBe(0);
    });

    it('should set sort order to 1 higher than the last tab', function () {
      const groups = [
        { type: contentTypeHelper.TYPE_GROUP, alias: 'settings', name: 'Settings', sortOrder: 100 },
        { type: contentTypeHelper.TYPE_TAB, alias: 'content', name: 'Content', sortOrder: 5 }
      ];
      
      const newTab = groups[0];
      contentTypeHelper.convertGroupToTab(groups, newTab);

      expect(newTab.sortOrder).toBe(6);
    });

  });

});
