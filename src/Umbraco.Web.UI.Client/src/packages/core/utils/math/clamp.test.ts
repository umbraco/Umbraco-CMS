import { expect } from '@open-wc/testing';
import { clamp } from './math.js';

describe('clamp', () => {
	it('should not allow the returned value to be lower than min', () => {
		expect(clamp(-1, 0, 1)).to.equal(0);
		expect(clamp(-100, 5, 100)).to.equal(5);
		expect(clamp(-50, -7, 20)).to.equal(-7);
		expect(clamp(100, 500, 502)).to.equal(500);
	});

	it('should not allow the returned value to be higher than max', () => {
		expect(clamp(2, 0, 1)).to.equal(1);
		expect(clamp(100, 5, 100)).to.equal(100);
		expect(clamp(50, -7, 20)).to.equal(20);
		expect(clamp(1000, 500, 502)).to.equal(502);
	});

	it('should return the value if it is within the min and max', () => {
		expect(clamp(0, 0, 1)).to.equal(0);
		expect(clamp(12, 10, 50)).to.equal(12);
		expect(clamp(-48, -50, 50)).to.equal(-48);
		expect(clamp(501, 500, 502)).to.equal(501);
	});
});
