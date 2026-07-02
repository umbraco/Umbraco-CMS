import { umbUrlPatternToString } from './url-pattern-to-string.function.js';
import { expect } from '@open-wc/testing';

describe('umbUrlPatternToString', () => {
	it('substitutes a single :param with the matching value', () => {
		expect(umbUrlPatternToString('/edit/:id', { id: 'abc' })).to.equal('/edit/abc');
	});

	it('substitutes multiple params in one pattern', () => {
		expect(umbUrlPatternToString('/users/:userId/posts/:postId', { userId: '1', postId: '2' })).to.equal(
			'/users/1/posts/2',
		);
	});

	it('coerces number values via toString()', () => {
		expect(umbUrlPatternToString('/edit/:id', { id: 42 })).to.equal('/edit/42');
	});

	it("uses an object's toString() method", () => {
		expect(umbUrlPatternToString('/edit/:id', { id: { toString: () => 'object-id' } })).to.equal('/edit/object-id');
	});

	it('writes the literal "null" when a param value is null', () => {
		expect(umbUrlPatternToString('/edit/:id', { id: null })).to.equal('/edit/null');
	});

	it('keeps :name in the output when the param is missing from the params object', () => {
		expect(umbUrlPatternToString('/edit/:id', {})).to.equal('/edit/:id');
	});

	it('returns the pattern unchanged when params is null', () => {
		expect(umbUrlPatternToString('/edit/:id', null)).to.equal('/edit/:id');
	});

	it('returns the pattern unchanged when it has no parameters', () => {
		expect(umbUrlPatternToString('/static/path', { id: 'abc' })).to.equal('/static/path');
	});

	it('treats a slash as a parameter terminator', () => {
		expect(umbUrlPatternToString('/edit/:id/extra', { id: 'abc' })).to.equal('/edit/abc/extra');
	});

	it('matches param names containing underscores and hyphens', () => {
		expect(umbUrlPatternToString('/edit/:user_id-foo', { 'user_id-foo': 'bar' })).to.equal('/edit/bar');
	});
});
