import { expect } from '@open-wc/testing';
import { inverseLerp, lerp } from './math.js';

describe('inverse lerp', () => {
	it('Calculate the inverse lerp factor for a value between two points.', () => {
		expect(inverseLerp(10, 20, 15)).to.equal(0.5);
		expect(inverseLerp(10, 20, 10)).to.equal(0);
		expect(inverseLerp(10, 20, 20)).to.equal(1);
		expect(inverseLerp(10, 20, 5)).to.equal(-0.5);
		expect(inverseLerp(10, 20, 25)).to.equal(1.5);
	});

	it('Handle the case where start and end are equal.', () => {
		expect(inverseLerp(5, 5, 5)).to.equal(0);
	});
});
