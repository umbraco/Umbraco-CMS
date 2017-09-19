describe('date helper tests', function () {
    var dateHelper;

    beforeEach(module('umbraco.services'));

    beforeEach(inject(function ($injector) {
        dateHelper = $injector.get('dateHelper');
    }));

    describe('converting to local moments', function () {

        it('converts from a positive offset', function () {
            var offsetMin = 600; //+10
            var strDate = "2016-01-01 10:00:00";

            var result = dateHelper.convertToLocalMomentTime(strDate, offsetMin);

            //expect(result.format("YYYY-MM-DD HH:mm:ss Z")).toBe("2016-01-01 01:00:00 +01:00");
        });

        it('converts from a negataive offset', function () {
            var offsetMin = -420; //-7
            var strDate = "2016-01-01 10:00:00";

            var result = dateHelper.convertToLocalMomentTime(strDate, offsetMin);

            //expect(result.format("YYYY-MM-DD HH:mm:ss Z")).toBe("2016-01-01 18:00:00 +01:00");
        });

    });

    describe('converting to server strings', function () {

        it('converts to a positive offset', function () {
            var offsetMin = 600; //+10
            var localDate = moment("2016-01-01 10:00:00");

            var result = dateHelper.convertToServerStringTime(localDate, offsetMin, "YYYY-MM-DD HH:mm:ss Z");

            //expect(result).toBe("2016-01-01 19:00:00 +10:00");
        });

        it('converts from a negataive offset', function () {
            var offsetMin = -420; //-7
            var localDate = moment("2016-01-01 10:00:00");

            var result = dateHelper.convertToServerStringTime(localDate, offsetMin, "YYYY-MM-DD HH:mm:ss Z");

            //expect(result).toBe("2016-01-01 02:00:00 -07:00");
        });

    });
});