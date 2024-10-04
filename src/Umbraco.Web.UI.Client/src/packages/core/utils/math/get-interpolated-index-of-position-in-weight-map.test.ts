import { expect } from '@open-wc/testing';
import { getInterpolatedIndexOfPositionInWeightMap } from './math.js';

describe('getInterpolatedIndexOfPositionInWeightMap', () => {
	it('should return the interpolated index of a value in a weight map', () => {
		const weights = [10, 20, 30, 40, 50];
		expect(getInterpolatedIndexOfPositionInWeightMap(-10, weights)).to.equal(0);
		expect(getInterpolatedIndexOfPositionInWeightMap(0, weights)).to.equal(0);
		expect(getInterpolatedIndexOfPositionInWeightMap(5, weights)).to.equal(0.5);
		expect(getInterpolatedIndexOfPositionInWeightMap(15, weights)).to.equal(1.25);
		expect(getInterpolatedIndexOfPositionInWeightMap(35, weights)).to.equal(2.1666666666666665);
		expect(getInterpolatedIndexOfPositionInWeightMap(45, weights)).to.equal(2.5);
		expect(getInterpolatedIndexOfPositionInWeightMap(50, weights)).to.equal(2.6666666666666665);
		expect(getInterpolatedIndexOfPositionInWeightMap(60, weights)).to.equal(3);
		expect(getInterpolatedIndexOfPositionInWeightMap(100, weights)).to.equal(4);
		expect(getInterpolatedIndexOfPositionInWeightMap(5000, weights)).to.equal(5);
	});
});
