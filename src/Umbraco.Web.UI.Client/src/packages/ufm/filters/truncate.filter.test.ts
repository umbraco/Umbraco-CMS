import { expect } from '@open-wc/testing';
import { UmbUfmTruncateFilterApi } from './truncate.filter.js';

describe('UmbUfmTruncateFilter', () => {
	let filter: UmbUfmTruncateFilterApi;

	beforeEach(() => {
		filter = new UmbUfmTruncateFilterApi();
	});

	describe('filter', () => {
		it('should not add ellipsis when text is shorter than length', () => {
			const result = filter.filter('Test', 10);
			expect(result).to.equal('Test');
		});

		it('should not add ellipsis when text equals length', () => {
			const result = filter.filter('Test', 4);
			expect(result).to.equal('Test');
		});

		it('should add ellipsis when text is longer than length', () => {
			const result = filter.filter('Lorem ipsum', 5);
			expect(result).to.equal('Lorem…');
		});

		it('should add custom tail when text is truncated', () => {
			const result = filter.filter('Lorem ipsum', 5, '...');
			expect(result).to.equal('Lorem...');
		});

		it('should not add tail when tail is false and text is truncated', () => {
			const result = filter.filter('Lorem ipsum', 5, 'false');
			expect(result).to.equal('Lorem');
		});

		it('should add ellipsis when tail is true and text is truncated', () => {
			const result = filter.filter('Lorem ipsum', 5, 'true');
			expect(result).to.equal('Lorem…');
		});

		it('should not add ellipsis when tail is false and text is not truncated', () => {
			const result = filter.filter('Test', 10, 'false');
			expect(result).to.equal('Test');
		});

		it('should not add custom tail when text is not truncated', () => {
			const result = filter.filter('Test', 10, '...');
			expect(result).to.equal('Test');
		});

		it('should handle empty string', () => {
			const result = filter.filter('', 10);
			expect(result).to.equal('');
		});

		it('should handle non-string input', () => {
			const result = filter.filter(null as any, 10);
			expect(result).to.equal(null);
		});

		it('should trim whitespace before adding ellipsis', () => {
			const result = filter.filter('Hello world', 6);
			expect(result).to.equal('Hello…');
		});
	});
});
