describe('appState tests', function () {
    var appState, $rootScope, editorState;
    
    beforeEach(module('umbraco.services'));

    beforeEach(inject(function ($injector) {
        appState = $injector.get('appState');
        $rootScope = $injector.get('$rootScope');
        editorState = $injector.get('editorState');
    }));

    describe('Editor state', function () {
        it('Can set', function () {
            editorState.set({ some: "object" });
            expect(editorState.current).not.toBeNull();
            expect(editorState.current.some).toBe("object");
        });
        it('Can reset', function () {
            editorState.reset();
            expect(editorState.current).toBeNull();
        });
        it('Throws when setting current', function () {
            function setCurrent() {
                editorState.current = { some: "object" };
            }            
            expect(setCurrent).toThrow();
        });
    });

    describe('Global state', function () {
        it('Can get/set state', function () {
            appState.setGlobalState("showNavigation", true);
            expect(appState.getGlobalState("showNavigation")).toBe(true);
        });        
        it('Throws when invalid key', function () {
            function setInvalidKey() {
                appState.setGlobalState("blah", true);
            }
            function getInvalidKey() {
                appState.getGlobalState("blah");
            }            
            expect(setInvalidKey).toThrow();
            expect(getInvalidKey).toThrow();
        });
    });
    
    describe('Section state', function () {
        it('Can get/set state', function () {
            appState.setSectionState("currentSection", true);
            expect(appState.getSectionState("currentSection")).toBe(true);
        });
        it('Throws when invalid key', function () {
            function setInvalidKey() {
                appState.getSectionState("blah", true);
            }
            function getInvalidKey() {
                appState.setSectionState("blah");
            }
            expect(setInvalidKey).toThrow();
            expect(getInvalidKey).toThrow();
        });
    });
    
    describe('Tree state', function () {
        it('Can get/set state', function () {
            appState.setTreeState("selectedNode", true);
            expect(appState.getTreeState("selectedNode")).toBe(true);
        });
        it('Throws when invalid key', function () {
            function setInvalidKey() {
                appState.getTreeState("blah", true);
            }
            function getInvalidKey() {
                appState.setTreeState("blah");
            }
            expect(setInvalidKey).toThrow();
            expect(getInvalidKey).toThrow();
        });
    });
    
    describe('Menu state', function () {
        it('Can get/set state', function () {
            appState.setMenuState("showMenu", true);
            expect(appState.getMenuState("showMenu")).toBe(true);
        });
        it('Throws when invalid key', function () {
            function setInvalidKey() {
                appState.getMenuState("blah", true);
            }
            function getInvalidKey() {
                appState.setMenuState("blah");
            }
            expect(setInvalidKey).toThrow();
            expect(getInvalidKey).toThrow();
        });
    });
});