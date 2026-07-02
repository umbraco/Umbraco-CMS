import { umbUrlPatternToString } from './url-pattern-to-string.function.js';
import { expect } from '@open-wc/testing';

describe('umbUrlPatternToString', () => {
	describe('when params is null', () => {
		it('should return the pattern unchanged', () => {
			expect(umbUrlPatternToString('/section/:id/edit', null)).to.eq('/section/:id/edit');
		});

		it('should return a pattern without parameters unchanged', () => {
			expect(umbUrlPatternToString('/section/dashboard', null)).to.eq('/section/dashboard');
		});
	});

	describe('parameter replacement', () => {
		it('should replace a single parameter with its value', () => {
			expect(umbUrlPatternToString('/section/:id', { id: '123' })).to.eq('/section/123');
		});

		it('should replace multiple parameters with their values', () => {
			expect(umbUrlPatternToString('/:section/:id/edit', { section: 'content', id: '123' })).to.eq(
				'/content/123/edit',
			);
		});

		it('should leave the pattern unchanged when it has no parameters', () => {
			expect(umbUrlPatternToString('/section/dashboard', { id: '123' })).to.eq('/section/dashboard');
		});

		it('should replace a parameter that spans the rest of a path segment', () => {
			expect(umbUrlPatternToString('/section/:entityType', { entityType: 'document-type' })).to.eq(
				'/section/document-type',
			);
		});

		it('should match parameter names containing underscores and hyphens', () => {
			expect(umbUrlPatternToString('/section/:user_id-foo', { 'user_id-foo': 'bar' })).to.eq('/section/bar');
		});
	});

	describe('value coercion', () => {
		it('should coerce a number value to a string', () => {
			expect(umbUrlPatternToString('/page/:index', { index: 42 })).to.eq('/page/42');
		});

		it('should call toString on an object value', () => {
			const value = { toString: () => 'custom' };
			expect(umbUrlPatternToString('/section/:id', { id: value })).to.eq('/section/custom');
		});

		it('should render a null value as the string "null"', () => {
			expect(umbUrlPatternToString('/section/:id', { id: null })).to.eq('/section/null');
		});
	});

	describe('missing parameters', () => {
		it('should keep the literal token when the value is undefined', () => {
			expect(umbUrlPatternToString('/section/:id', {})).to.eq('/section/:id');
		});

		it('should replace known parameters and keep unknown ones literal', () => {
			expect(umbUrlPatternToString('/:section/:id', { section: 'content' })).to.eq('/content/:id');
		});
	});
});
