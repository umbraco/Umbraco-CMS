import { expect } from '@open-wc/testing';
import { isBelowMinItemCount } from './is-below-min-item-count.function.js';

describe('isBelowMinItemCount', () => {
	it('is not below the minimum when no minimum is configured', () => {
		expect(isBelowMinItemCount(0, undefined)).to.equal(false);
		expect(isBelowMinItemCount(1, undefined)).to.equal(false);
		expect(isBelowMinItemCount(0, 0)).to.equal(false);
		expect(isBelowMinItemCount(1, 0)).to.equal(false);
	});

	it('is not below the minimum when the collection is empty', () => {
		// An empty collection is governed by whether the property is mandatory, not by the minimum.
		expect(isBelowMinItemCount(0, 1)).to.equal(false);
		expect(isBelowMinItemCount(0, 3)).to.equal(false);
	});

	it('is below the minimum when the collection is in use but short', () => {
		expect(isBelowMinItemCount(1, 3)).to.equal(true);
		expect(isBelowMinItemCount(2, 3)).to.equal(true);
	});

	it('is not below the minimum when the minimum is met or exceeded', () => {
		expect(isBelowMinItemCount(3, 3)).to.equal(false);
		expect(isBelowMinItemCount(4, 3)).to.equal(false);
	});
});
