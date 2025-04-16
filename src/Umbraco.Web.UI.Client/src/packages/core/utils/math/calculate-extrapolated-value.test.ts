import { expect } from '@open-wc/testing';
import { calculateExtrapolatedValue } from './math.js';

describe('calculateExtrapolatedValue', () => {
	it('should return NaN if the increase factor is less than 0', () => {
		expect(calculateExtrapolatedValue(1, -1)).to.be.NaN;
	});

	it('should return NaN if the increase factor is equal to 1', () => {
		expect(calculateExtrapolatedValue(1, 1)).to.be.NaN;
	});

	it('should return the extrapolated value', () => {
		expect(calculateExtrapolatedValue(1, 0)).to.equal(1);
		expect(calculateExtrapolatedValue(1, 0.3)).to.equal(1.4285714285714286);
		expect(calculateExtrapolatedValue(2, 0.5)).to.equal(4);
		expect(calculateExtrapolatedValue(3, 0.6)).to.equal(7.5);
		expect(calculateExtrapolatedValue(100, 0.2)).to.equal(125);
		expect(calculateExtrapolatedValue(500, 0.99)).to.equal(49999.999999999956);
	});
});
