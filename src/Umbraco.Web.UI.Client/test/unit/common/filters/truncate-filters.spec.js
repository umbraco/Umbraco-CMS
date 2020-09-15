(function () {
    
    describe('truncate filter', function() {
        var $truncate;

        var testCases = [
            {input:'test',                          noOfChars:5,    appendDots:true,    expectedResult: 'test'},
            {input:'test ',                          noOfChars:4,    appendDots:true,    expectedResult: 'test…'},
            {input:'test a long text with space',   noOfChars:5,    appendDots:true,    expectedResult: 'test …'},
            {input:'scenarios is a long word',      noOfChars:5,    appendDots:true,    expectedResult: 'scena…'},
            {input:'scenarios is a long word',      noOfChars:10,   appendDots:true,    expectedResult: 'scenarios …'},
            {input:'test',                          noOfChars:5,    appendDots:false,   expectedResult: 'test'},
            {input:'test a long text with space',   noOfChars:5,    appendDots:false,   expectedResult: 'test '},
            {input:'scenarios is a long word',      noOfChars:5,    appendDots:false,   expectedResult: 'scena'},
            {input:'scenarios is a long word',      noOfChars:10,   appendDots:false,   expectedResult: 'scenarios '}
        ];

        var testCasesNew = [
            {value:'test',                          wordwise:false,    max:20,  tail:'...',  expectedResult: 'test'},
            {value:'LoremIpsumLoremIpsumLoremIpsum',wordwise:false,    max:20,  tail:null,  expectedResult: 'LoremIpsumLoremIpsum…'}
        ];
        
        beforeEach(module('umbraco'));
        
        beforeEach(inject(function($filter) {
            $truncate = $filter('truncate');
        }));
        
        it('empty string as input is expected to give an empty string', function() {
            expect($truncate('', 5, true)).toBe('');
        });
        
        it('null as input is expected to give an empty string', function() {
            expect($truncate(null, 5, true)).toBe('');
        });
        
        it('undefined as input is expected to give an empty string', function() {
            expect($truncate(undefined, 5, true)).toBe('');
        });

        it('null as noOfChars to result in \'test\'', function() {
            expect($truncate('test', null, true)).toBe('test');
        });
        
        it('undefined as noOfChars to result in \'test\'', function() {
            expect($truncate('test', undefined, true)).toBe('test');
        });

        it('null as appendDots to behave as false', function() {
            expect($truncate('test', 5, null)).toBe('test');
        });
        
        testCases.forEach(function(test){
            
            it('Expects \'' + test.input + '\' to be truncated as \''+ test.expectedResult + '\', when noOfChars=' + test.noOfChars + ', and appendDots=' + test.appendDots, function() {
                expect($truncate(test.input, test.noOfChars, test.appendDots)).toBe(test.expectedResult);
            });
        });
        
        testCasesNew.forEach(function(test){
            it('Expects \'' + test.value + '\' to be truncated as \''+ test.expectedResult + '\', when wordwise=' + test.wordwise + ', and max=' + test.max  + ', and tail=' + test.tail, function() {
                expect($truncate(test.value, test.wordwise, test.max, test.tail)).toBe(test.expectedResult);
            });
        });
        
    });

}());
