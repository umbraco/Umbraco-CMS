import { expect } from '@open-wc/testing';
import { distance } from './math.js';

describe('distance', () => {
	it('should return the distance between two points', () => {
		expect(distance(5, 10)).to.equal(5);
		expect(distance(5.86732, 10.3989328)).to.equal(4.5316128);
		expect(distance(-25.8673, 12912.47831)).to.equal(12938.34561);
	});
});
