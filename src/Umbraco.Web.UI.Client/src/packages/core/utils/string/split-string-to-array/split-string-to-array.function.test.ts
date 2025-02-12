import { splitStringToArray } from './split-string-to-array.function.js';
import { expect } from '@open-wc/testing';

describe('splitStringToArray', () => {
	it('splits and cleans a comma-separated string', () => {
		const inputString = 'one, two, three, ,five';
		const result = splitStringToArray(inputString);

		expect(result).to.deep.equal(['one', 'two', 'three', 'five']);
	});

	it('handles custom delimiters', () => {
		const inputString = 'apple | orange | banana';
		const result = splitStringToArray(inputString, ' | ');

		expect(result).to.deep.equal(['apple', 'orange', 'banana']);
	});

	it('handles strings with no non-empty elements', () => {
		const inputString = ', , , , ';
		const result = splitStringToArray(inputString);

		expect(result).to.deep.equal([]);
	});

	it('trims whitespace from each element', () => {
		const inputString = '   one  ,  two  ,  three   ';
		const result = splitStringToArray(inputString);

		expect(result).to.deep.equal(['one', 'two', 'three']);
	});

	it('returns an empty array for an empty string', () => {
		const inputString = '';
		const result = splitStringToArray(inputString);

		expect(result).to.deep.equal([]);
	});
});
