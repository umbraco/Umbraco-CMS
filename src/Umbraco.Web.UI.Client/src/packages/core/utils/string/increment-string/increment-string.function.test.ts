import { incrementString } from './increment-string.function.js';
import { expect } from '@open-wc/testing';

describe('incrementString', () => {
	it('increments a string with a number at the end', () => {
		const inputString = 'test';
		const result = incrementString(inputString);

		expect(result).to.equal('test1');
	});

	it('increments a string with a number at the end by 1', () => {
		const inputString = 'test1';
		const result = incrementString(inputString);

		expect(result).to.equal('test2');
	});
});
