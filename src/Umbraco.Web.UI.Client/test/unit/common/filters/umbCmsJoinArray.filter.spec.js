(function () {

    describe('umbCmsJoinArray filter', function() {
        var $umbCmsJoinArray;

        var testCases = [
            {input:[{param:'a'},{param:'b'},{param:'c'}],  separator:', ', prop:'param' ,   expectedResult: 'a, b, c'},
            {input:[{param:'a'},{param:'b'},{param:'c'}],  separator:' ', prop:'param' ,   expectedResult: 'a b c'},
            {input:[{param:'a'},{param:'b'},{param:'c'}],  separator:'', prop:'param' ,   expectedResult: 'abc'},
            {input:[{param:'a'},{param:'b'},{param:'c'}],  separator:'', prop:'wrong' ,   expectedResult: ''},
            {input:[],  separator:', ', prop:'param' ,   expectedResult: ''},
            {input:[{param:'a'},{param:'b'},{param:'c'}],  separator:', ', prop:null ,   expectedResult: ', , '},
            {input:[{param:'a'},{param:'b'},{param:'c'}],  separator:null, prop:'param' ,   expectedResult: 'abc'},
            {input:'test',  separator:', ', prop:'param', expectedResult: 'test'},
            {input:null,  separator:', ', prop:'param', expectedResult: ''},
            {input:undefined,  separator:', ', prop:'param', expectedResult: ''},
        ];

        beforeEach(module('umbraco'));

        beforeEach(inject(function($filter) {
            $umbCmsJoinArray = $filter('umbCmsJoinArray');
        }));

        testCases.forEach(function(test){
            it('Blackbox tests with expected result=\''+test.expectedResult+'\'', function() {
                expect($umbCmsJoinArray(test.input, test.separator, test.prop)).toBe(test.expectedResult);
            });
        });
    });

}());
