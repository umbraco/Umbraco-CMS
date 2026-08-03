import { umbUrlPatternToString } from './url-pattern-to-string.function.js';
import { expect } from '@open-wc/testing';

describe('umbUrlPatternToString', () => {
	it('replaces a single parameter with its value', () => {
		expect(umbUrlPatternToString('/section/:id', { id: '123' })).to.equal('/section/123');
	});

	it('replaces multiple parameters with their values', () => {
		expect(umbUrlPatternToString('/:section/:id/edit', { section: 'content', id: '123' })).to.equal(
			'/content/123/edit',
		);
	});

	it('replaces a parameter that spans the rest of a path segment', () => {
		expect(umbUrlPatternToString('/section/:entityType', { entityType: 'document-type' })).to.equal(
			'/section/document-type',
		);
	});

	it('matches parameter names containing underscores and hyphens', () => {
		expect(umbUrlPatternToString('/section/:user_id-foo', { 'user_id-foo': 'bar' })).to.equal('/section/bar');
	});

	it('coerces a number value to a string', () => {
		expect(umbUrlPatternToString('/page/:index', { index: 42 })).to.equal('/page/42');
	});

	it('calls toString on an object value', () => {
		expect(umbUrlPatternToString('/section/:id', { id: { toString: () => 'custom' } })).to.equal('/section/custom');
	});

	it('renders a null value as the string "null"', () => {
		expect(umbUrlPatternToString('/section/:id', { id: null })).to.equal('/section/null');
	});

	it('keeps the literal token when the value is undefined', () => {
		expect(umbUrlPatternToString('/section/:id', {})).to.equal('/section/:id');
	});

	it('replaces known parameters and keeps unknown ones literal', () => {
		expect(umbUrlPatternToString('/:section/:id', { section: 'content' })).to.equal('/content/:id');
	});

	it('leaves the pattern unchanged when it has no parameters', () => {
		expect(umbUrlPatternToString('/section/dashboard', { id: '123' })).to.equal('/section/dashboard');
	});

	it('returns the pattern unchanged when params is null', () => {
		expect(umbUrlPatternToString('/section/:id/edit', null)).to.equal('/section/:id/edit');
	});

	it('returns a pattern without parameters unchanged when params is null', () => {
		expect(umbUrlPatternToString('/section/dashboard', null)).to.equal('/section/dashboard');
	});
});
