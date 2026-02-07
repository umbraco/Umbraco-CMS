import { umbDeepEqual } from './deep-equal.function.js';
import { expect } from '@open-wc/testing';

describe('umbDeepEqual', () => {
	describe('primitives', () => {
		it('returns true for equal strings', () => {
			expect(umbDeepEqual('hello', 'hello')).to.be.true;
		});

		it('returns false for different strings', () => {
			expect(umbDeepEqual('hello', 'world')).to.be.false;
		});

		it('returns true for equal numbers', () => {
			expect(umbDeepEqual(42, 42)).to.be.true;
		});

		it('returns false for different numbers', () => {
			expect(umbDeepEqual(42, 43)).to.be.false;
		});

		it('returns true for both null', () => {
			expect(umbDeepEqual(null, null)).to.be.true;
		});

		it('returns false for null vs undefined', () => {
			expect(umbDeepEqual(null, undefined)).to.be.false;
		});

		it('returns true for both undefined', () => {
			expect(umbDeepEqual(undefined, undefined)).to.be.true;
		});
	});

	describe('objects', () => {
		it('returns true for equal objects', () => {
			expect(umbDeepEqual({ a: 1, b: 2 }, { a: 1, b: 2 })).to.be.true;
		});

		it('returns false for objects with different values', () => {
			expect(umbDeepEqual({ a: 1, b: 2 }, { a: 1, b: 3 })).to.be.false;
		});

		it('returns false for objects with different keys', () => {
			expect(umbDeepEqual({ a: 1 }, { b: 1 })).to.be.false;
		});

		it('returns true for objects with same keys in different order', () => {
			expect(umbDeepEqual({ a: 1, b: 2, c: 3 }, { c: 3, a: 1, b: 2 })).to.be.true;
		});

		it('returns true for nested objects with different key order', () => {
			const a = { outer: { x: 1, y: 2 }, name: 'test' };
			const b = { name: 'test', outer: { y: 2, x: 1 } };
			expect(umbDeepEqual(a, b)).to.be.true;
		});

		it('returns false for nested objects with different values', () => {
			const a = { outer: { x: 1, y: 2 } };
			const b = { outer: { x: 1, y: 3 } };
			expect(umbDeepEqual(a, b)).to.be.false;
		});
	});

	describe('arrays', () => {
		it('returns true for equal arrays', () => {
			expect(umbDeepEqual([1, 2, 3], [1, 2, 3])).to.be.true;
		});

		it('returns false for arrays with different order', () => {
			expect(umbDeepEqual([1, 2, 3], [3, 2, 1])).to.be.false;
		});

		it('returns false for arrays with different lengths', () => {
			expect(umbDeepEqual([1, 2], [1, 2, 3])).to.be.false;
		});

		it('returns true for arrays of objects with different key order', () => {
			const a = [{ alias: 'text', value: 'aaa', culture: null }];
			const b = [{ culture: null, alias: 'text', value: 'aaa' }];
			expect(umbDeepEqual(a, b)).to.be.true;
		});

		it('returns false for arrays of objects with different values', () => {
			const a = [{ alias: 'text', value: 'aaa' }];
			const b = [{ alias: 'text', value: 'bbb' }];
			expect(umbDeepEqual(a, b)).to.be.false;
		});
	});

	describe('mixed structures', () => {
		it('returns true for complex structures with different key order at all levels', () => {
			const a = {
				entityType: 'document',
				unique: '123',
				values: [
					{ editorAlias: 'Umbraco.TextBox', alias: 'text1', value: 'aaa', culture: null, segment: null },
				],
				variants: [{ culture: null, segment: null, name: 'Test' }],
			};
			const b = {
				unique: '123',
				entityType: 'document',
				variants: [{ name: 'Test', culture: null, segment: null }],
				values: [
					{ culture: null, segment: null, editorAlias: 'Umbraco.TextBox', alias: 'text1', value: 'aaa' },
				],
			};
			expect(umbDeepEqual(a, b)).to.be.true;
		});

		it('returns false when a nested value differs in a complex structure', () => {
			const a = {
				entityType: 'document',
				values: [{ alias: 'text1', value: 'aaa', culture: null }],
			};
			const b = {
				entityType: 'document',
				values: [{ alias: 'text1', value: 'bbb', culture: null }],
			};
			expect(umbDeepEqual(a, b)).to.be.false;
		});

		it('returns false for object vs array', () => {
			expect(umbDeepEqual({ 0: 'a' }, ['a'])).to.be.false;
		});
	});
});
