import { expect } from '@open-wc/testing';
import { isWithinRect } from './math.js';

describe('isWithinRect', () => {
	const rect = new DOMRect(0, 0, 100, 100);

	it('should return true if the point is within the rectangle', () => {
		expect(isWithinRect(50, 50, rect)).to.be.true;
		expect(isWithinRect(1, 1, rect)).to.be.true;
		expect(isWithinRect(99, 99, rect)).to.be.true;
	});

	it('should return false if the point is outside the rectangle', () => {
		expect(isWithinRect(0, 0, rect)).to.be.false;
		expect(isWithinRect(100, 100, rect)).to.be.false;
		expect(isWithinRect(101, 50, rect)).to.be.false;
		expect(isWithinRect(50, 101, rect)).to.be.false;
		expect(isWithinRect(-1, 50, rect)).to.be.false;
		expect(isWithinRect(50, -1, rect)).to.be.false;
	});

	it('should return false if the point is on the "border" of the rectangle', () => {
		expect(isWithinRect(0, 0, rect)).to.be.false;
		expect(isWithinRect(100, 100, rect)).to.be.false;
		expect(isWithinRect(100, 0, rect)).to.be.false;
		expect(isWithinRect(0, 100, rect)).to.be.false;
	});

	it('should return true if the point is within the expanded rectangle', () => {
		expect(isWithinRect(110, 80, rect, 20)).to.be.true;
		expect(isWithinRect(80, 110, rect, 20)).to.be.true;
	});

	it('should return false if the point is outside the expanded rectangle', () => {
		expect(isWithinRect(130, 80, rect, 20)).to.be.false;
		expect(isWithinRect(80, 130, rect, 20)).to.be.false;
	});
});
