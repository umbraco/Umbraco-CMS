import { truncateString } from './truncate-string.function.js';
import { expect } from '@open-wc/testing';

describe('truncateString', () => {
	it('returns an empty string when no input is given', () => {
		expect(truncateString(undefined, 10)).to.equal('');
	});

	it('returns the input unchanged when it fits within the maximum length', () => {
		expect(truncateString('0123456789', 10)).to.equal('0123456789');
	});

	it('keeps the start and the end of a longer input', () => {
		expect(truncateString('0123456789ABCDEF', 10)).to.equal('01234…CDEF');
	});

	it('counts the separator towards the maximum length', () => {
		const result = truncateString('0123456789ABCDEF', 10, '...');
		expect(result).to.equal('0123...DEF');
		expect(result.length).to.equal(10);
	});
});
