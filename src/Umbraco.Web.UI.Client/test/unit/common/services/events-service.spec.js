describe('angular event tests', function () {
    var $rootScope, eventsService, $timeout, $q;
    
    beforeEach(module('umbraco.services'));

    beforeEach(inject(function ($injector) {
        $rootScope = $injector.get('$rootScope');
        eventsService = $injector.get('eventsService');
        $timeout = $injector.get('$timeout');
        $q = $injector.get('$q');
    }));

    describe('event handling', function () {

        it('will handle one non async process', function () {
            var eventArgs = {
                val: ""
            };
            
            eventsService.subscribe("testEvent", function (args) {
                args.val = "changed";
            });

            eventsService.publish("testEvent", eventArgs).then(function (args) {
                expect(eventArgs.val).toBe("changed");
                expect(args.val).toBe("changed");
            });
            
            $rootScope.$digest();
        });
        
        //NOTE: This will fail because our eventsService doesn't actually wait for async events to finish.

        //it('will handle one async process', function () {
        //    var eventArgs = {
        //        val: ""
        //    };

        //    eventsService.subscribe("testEvent", function (args) {
        //        $timeout(function() {
        //            args.val = "changed";
        //        }, 1000);
        //    });

        //    eventsService.publish("testEvent", eventArgs).then(function (args) {
        //        expect(eventArgs.val).toBe("changed");
        //        expect(args.val).toBe("changed");
        //    });

        //    $rootScope.$digest();
        //    $timeout.flush();
        //});
        
        it('will wait to execute after all handlers', function () {

            //assign multiple listeners

            $rootScope.$on("testEvent", function(e, args) {
                console.log("handler #1");
                $timeout(function () {
                    console.log("timeout #1");
                    args.val = "changed1";
                    args.resolve(args);
                }, 1000);
            });
            
            $rootScope.$on("testEvent", function (e, args) {
                console.log("handler #2");
                $timeout(function () {
                    console.log("timeout #2");
                    args.val = "changed2";
                    args.resolve(args);
                }, 1000);
            });

            //setup a promise for each listener
            var promises = [];
            for (var i = 0; i < $rootScope.$$listeners["testEvent"].length; i++) {
                promises.push($q.defer());
            }
            var eventArgs = {
                val: "",
                reject: function(args) {
                    promises.pop().reject(args);
                },
                resolve: function(args) {
                    promises.pop().resolve(args);
                }                
            };

            $rootScope.$broadcast("testEvent", eventArgs);

            //We can still do stuff here after the event execution...

            //This is where we'd wait for each one to finish.
            var index = 0;
            _.each(promises, function(p) {
                p.promise.then(function (args) {
                    index++;
                    console.log("YOU ARE HERE: " + args.val);
                    expect(args.val).toBe("changed" + index);
                });
            });
            
            $rootScope.$digest();
            $timeout.flush();

        });

    });
    
});