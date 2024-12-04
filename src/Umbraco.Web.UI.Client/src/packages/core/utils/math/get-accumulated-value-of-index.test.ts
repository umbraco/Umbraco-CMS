import { expect } from '@open-wc/testing';
import { getAccumulatedValueOfIndex } from './math.js';

describe('getAccumulatedValueOfIndex', () => {
	it('should return the accumulated value of an array up to a certain index', () => {
		expect(getAccumulatedValueOfIndex(0, [1, 2, 3, 4, 5])).to.equal(0);
		expect(getAccumulatedValueOfIndex(1, [1, 2, 3, 4, 5])).to.equal(1);
		expect(getAccumulatedValueOfIndex(2, [1, 2, 3, 4, 5])).to.equal(3);
		expect(getAccumulatedValueOfIndex(3, [1, 2, 3, 4, 5])).to.equal(6);
		expect(getAccumulatedValueOfIndex(4, [1, 2, 3, 4, 5])).to.equal(10);
		expect(getAccumulatedValueOfIndex(5, [1, 2, 3, 4, 5])).to.equal(15);
	});
});
