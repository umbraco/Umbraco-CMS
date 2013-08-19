describe('keyboard service tests', function () {
    var keyboardService, $window;

      var createKeyEvent = function (mainKey, alt, ctrl, shift, meta) {
        var keyEvent = jQuery.Event("keypress");
        keyEvent.keyCode = mainKey.charCodeAt(0);
        keyEvent.altKey = alt;
        keyEvent.ctrlKey = ctrl;
        keyEvent.shiftKey = shift;
        keyEvent.metaKey = meta;
        return keyEvent;
      };
      
  
    beforeEach(module('umbraco.services'));
    beforeEach(inject(function ($injector) {
        keyboardService = $injector.get('keyboardService');
        $window = $injector.get("$window");
    }));

    describe('Detecting key combinations', function () {

        it('Detects ctrl+s', function () {
            
            var ctrls = false;
            var el = $("<span></span>");
            var ev = createKeyEvent("s", false, true, false);

            el.keypress(function(ev) {
              console.log(ev);  
              console.log("Handler for .keypress() called.");
            });


            console.log("loaded");

            keyboardService.bind("ctrl+s", function(){
                ctrls = true;
                console.log("triggered");
            }, el);

            //initially it should be false
            expect(ctrls).toBe(false);
            
            //trigger the ctrls+s event
            el.trigger(ev);

            //it should now be true
//            expect(ctrls).toBe(true);

          //  expect(iconHelper.isFileBasedIcon(legacyBased)).toBe(false);
          //  expect(iconHelper.isFileBasedIcon(belleBased)).toBe(false);
        });
        
        /*
        it('detects a legacy icon', function () {
            var fileBased = "this-is-file-based.jpg";
            var legacyBased = ".legacy-class";
            var belleBased = "normal-class";

            expect(iconHelper.isLegacyIcon(fileBased)).toBe(false);
            expect(iconHelper.isLegacyIcon(legacyBased)).toBe(true);
            expect(iconHelper.isLegacyIcon(belleBased)).toBe(false);
        });
        
        it('converts from legacy icon', function () {
            var legacyBased = ".sprTreeSettingDomain";

            expect(iconHelper.convertFromLegacyIcon(legacyBased)).toBe("icon-home");
            
        });*/

    });
});