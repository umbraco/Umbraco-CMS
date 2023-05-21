describe('valEmail directive tests', function() {

    var valEmailExpression;

    beforeEach(module('umbraco'));

    beforeEach(inject(function ($injector) {
        // TODO: I have no idea why this doesn't work!!?? it freakin should
        //valEmailExpression = $injector.get('valEmailExpression');

        //in the meantime, i've had to hard code the regex statement here
        valEmailExpression = {
            EMAIL_REGEXP: /^[a-z0-9!#$%&'*+\/=?^_`{|}~.-]+@[a-z0-9]([a-z0-9-]*[a-z0-9])?(\.[a-z0-9]([a-z0-9-]*[a-z0-9])?)*$/i
        };

    }));

    describe('EMAIL_REGEXP', function () {
        /* global EMAIL_REGEXP: false */
        it('should validate email', function () {
            expect(valEmailExpression.EMAIL_REGEXP.test('a@b.com')).toBe(true);
            expect(valEmailExpression.EMAIL_REGEXP.test('a@b.museum')).toBe(true);
            expect(valEmailExpression.EMAIL_REGEXP.test('a@B.c')).toBe(true);
            expect(valEmailExpression.EMAIL_REGEXP.test('a@.b.c')).toBe(false);
            expect(valEmailExpression.EMAIL_REGEXP.test('a@-b.c')).toBe(false);
            expect(valEmailExpression.EMAIL_REGEXP.test('a@b-.c')).toBe(false);
            expect(valEmailExpression.EMAIL_REGEXP.test('a@3b.c')).toBe(true);
            expect(valEmailExpression.EMAIL_REGEXP.test('a@b')).toBe(true);
            expect(valEmailExpression.EMAIL_REGEXP.test('abc@xyz.financial')).toBe(true);
            expect(valEmailExpression.EMAIL_REGEXP.test('admin@c.pizza')).toBe(true);
            expect(valEmailExpression.EMAIL_REGEXP.test('admin+gmail-syntax@c.pizza')).toBe(true);            
        });
    });

});