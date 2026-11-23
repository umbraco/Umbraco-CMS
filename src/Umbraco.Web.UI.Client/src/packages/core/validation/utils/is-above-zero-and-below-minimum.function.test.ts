import { expect } from '@open-wc/testing';
import { isAboveZeroAndBelowMinimum } from './is-above-zero-and-below-minimum.function.js';

describe('isAboveZeroAndBelowMinimum', () => {
	it('is false when no minimum is configured', () => {
		expect(isAboveZeroAndBelowMinimum(0, undefined)).to.equal(false);
		expect(isAboveZeroAndBelowMinimum(1, undefined)).to.equal(false);
		expect(isAboveZeroAndBelowMinimum(0, 0)).to.equal(false);
		expect(isAboveZeroAndBelowMinimum(1, 0)).to.equal(false);
	});

	it('is false when the count is zero', () => {
		expect(isAboveZeroAndBelowMinimum(0, 1)).to.equal(false);
		expect(isAboveZeroAndBelowMinimum(0, 3)).to.equal(false);
	});

	it('is true when the count is above zero but below the minimum', () => {
		expect(isAboveZeroAndBelowMinimum(1, 3)).to.equal(true);
		expect(isAboveZeroAndBelowMinimum(2, 3)).to.equal(true);
	});

	it('is false when the minimum is met or exceeded', () => {
		expect(isAboveZeroAndBelowMinimum(3, 3)).to.equal(false);
		expect(isAboveZeroAndBelowMinimum(4, 3)).to.equal(false);
	});
});
