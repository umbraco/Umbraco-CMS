import { expect } from '@open-wc/testing';
import { lerp } from './math.js';

describe('lerp', () => {
	it('Interpolate between two values.', () => {
		expect(lerp(1, 20, 0.5)).to.equal(10.5);
		expect(lerp(1, 100, 0.2)).to.equal(20.8);
		expect(lerp(2, 23, 0.4)).to.equal(10.4);
		expect(lerp(50, 250, 0.8)).to.equal(210);
	});

	it('Ensure alpha is clamped to the range [0, 1].', () => {
		expect(lerp(10, 20, 1.5)).to.equal(20);
	});
});
