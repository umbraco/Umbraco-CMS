import { expect } from '@open-wc/testing';
import { batchArray } from './batch-array.js';

describe('batchArray', () => {
	it('should split an array into chunks of the specified size', () => {
		const array = [1, 2, 3, 4, 5];
		const batchSize = 2;
		const result = batchArray(array, batchSize);
		expect(result).to.deep.equal([[1, 2], [3, 4], [5]]);
	});

	it('should handle arrays smaller than the batch size', () => {
		const array = [1];
		const batchSize = 2;
		const result = batchArray(array, batchSize);
		expect(result).to.deep.equal([[1]]);
	});

	it('should handle empty arrays', () => {
		const array: number[] = [];
		const batchSize = 2;
		const result = batchArray(array, batchSize);
		expect(result).to.deep.equal([]);
	});
});
