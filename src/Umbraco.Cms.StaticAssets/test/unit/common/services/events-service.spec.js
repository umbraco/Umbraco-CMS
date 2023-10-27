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
            
            eventsService.on("testEvent", function (e, args) {                
                args.val = "changed";
            });

            eventsService.emit("testEvent", eventArgs);
            
            expect(eventArgs.val).toBe("changed");
            
            $rootScope.$digest();
        });
        
        it('will handle 2 non async process', function () {
            var eventArgs = {
                val: ""
            };

            eventsService.on("testEvent", function (e, args) {
                args.val = "changed";
            });
            
            eventsService.on("testEvent", function (e, args) {
                args.val = "changed1";
            });

            eventsService.emit("testEvent", eventArgs);

            expect(eventArgs.val).toBe("changed1");

            $rootScope.$digest();
        });

        it('will handle one async process', function () {
            var eventArgs = {
                val: ""
            };

            eventsService.on("testEvent", function (e, msg) {
                $timeout(function () {                    
                    msg.val = "changed";
                    //NOTE: We could resolve anything here
                    msg.resolve(msg);
                }, 1000);
            });

            
            //this won't be changed yet
            expect(eventArgs.val).toBe("");

            /*
            promises[0].then(function (args) {
                expect(args.val).toBe("changed");
                expect(eventArgs.val).toBe("changed");
            });*/

            $rootScope.$digest();
        });
        
        //NOTE: This logic has been merged into the eventsService
        it('POC will wait to execute after all handlers', function () {

            //assign multiple listeners

            $rootScope.$on("testEvent", function(e, args) {
                $timeout(function () {
                    args.val = "changed1";
                    args.resolve(args);
                }, 1000);
            });
            
            $rootScope.$on("testEvent", function (e, args) {
                $timeout(function () {
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
                    expect(args.val).toBe("changed" + index);
                });
            });
            
            $rootScope.$digest();
            $timeout.flush();

        });

    });
    
});