import { stringOrStringArrayContains } from './string-or-string-array-contains.function.js';
import { expect } from '@open-wc/testing';

describe('stringOrStringArrayContains', () => {
	it('returns true if a string matches a search string', () => {
		const result = stringOrStringArrayContains('hello', 'hello');
		expect(result).to.be.true;
	});

	it('returns false if a string does not contain a search string', () => {
		const result = stringOrStringArrayContains('hello', 'world');
		expect(result).to.be.false;
	});

	it('returns false if a search string partly matches the string', () => {
		const result = stringOrStringArrayContains('hello', 'ello');
		expect(result).to.be.false;
	});

	it('returns true if an array of strings contains a search string', () => {
		const result = stringOrStringArrayContains(['hello', 'world'], 'world');
		expect(result).to.be.true;
	});

	it('returns false if an array of strings does not contain a search string', () => {
		const result = stringOrStringArrayContains(['hello', 'world'], 'foo');
		expect(result).to.be.false;
	});

	it('returns false if an empty array is passed', () => {
		const result = stringOrStringArrayContains([], 'foo');
		expect(result).to.be.false;
	});
});
