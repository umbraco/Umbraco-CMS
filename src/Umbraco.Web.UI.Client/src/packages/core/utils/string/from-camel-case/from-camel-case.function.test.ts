import { expect } from '@open-wc/testing';
import { fromCamelCase } from './from-camel-case.function.js';

describe('fromCamelCase', () => {
	it('should return the string with spaces between words', () => {
		expect(fromCamelCase('thisIsATest')).to.equal('This Is A Test');
	});

	it('should uppercase the first character', () => {
		const result = fromCamelCase('thisIsATest');
		expect(result.charAt(0)).to.equal('T');
	});
});
