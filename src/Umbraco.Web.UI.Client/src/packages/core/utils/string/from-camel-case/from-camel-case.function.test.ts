import { expect } from '@open-wc/testing';
import { fromCamelCase, fromCamelCaseIfCamelCase } from './from-camel-case.function.js';

describe('fromCamelCase', () => {
	it('should return the string with spaces between words', () => {
		expect(fromCamelCase('thisIsATest')).to.equal('This Is A Test');
	});

	it('should uppercase the first character', () => {
		const result = fromCamelCase('thisIsATest');
		expect(result.charAt(0)).to.equal('T');
	});
});

describe('fromCamelCaseIfCamelCase', () => {
	it('should transform a camelCase string', () => {
		expect(fromCamelCaseIfCamelCase('richContent')).to.equal('Rich Content');
	});

	it('should transform a single lowercase word', () => {
		expect(fromCamelCaseIfCamelCase('common')).to.equal('Common');
	});

	it('should return a string with spaces as-is', () => {
		expect(fromCamelCaseIfCamelCase('Rich Content')).to.equal('Rich Content');
	});

	it('should return a PascalCase string as-is', () => {
		expect(fromCamelCaseIfCamelCase('MyCusTomUIGroup')).to.equal('MyCusTomUIGroup');
	});

	it('should return an uppercase-start string as-is', () => {
		expect(fromCamelCaseIfCamelCase('Common')).to.equal('Common');
	});
});
