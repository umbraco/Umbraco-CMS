import { expect } from '@open-wc/testing';
import { api as UmbUfmFallbackFilterApi } from './fallback.filter.js';

describe('UmbUfmFallbackFilter', () => {
	let filter: UmbUfmFallbackFilterApi;

	beforeEach(() => {
		filter = new UmbUfmFallbackFilterApi();
	});

	describe('filter', () => {
		it('should return original value when string has content', () => {
			const result = filter.filter('Test', 'Fallback');

			expect(result).to.equal('Test');
		});

		it('should return fallback when string is empty', () => {
			const result = filter.filter('', 'Fallback');

			expect(result).to.equal('Fallback');
		});

		it('should return fallback when value is null', () => {
			const result = filter.filter(null, 'Fallback');

			expect(result).to.equal('Fallback');
		});

		it('should return fallback when value is undefined', () => {
			const result = filter.filter(undefined, 'Fallback');

			expect(result).to.equal('Fallback');
		});
	});
});
