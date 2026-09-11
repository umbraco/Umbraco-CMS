import { expect } from '@open-wc/testing';
import { hasClassNames, splitClassNames } from './class-names.function.js';

describe('splitClassNames', () => {
	it('splits a whitespace-separated class attribute value', () => {
		expect(splitClassNames('title--size-5 title--bold')).to.deep.equal(['title--size-5', 'title--bold']);
	});

	it('filters out extra whitespace', () => {
		expect(splitClassNames('  title--size-5   title--bold  ')).to.deep.equal(['title--size-5', 'title--bold']);
	});

	it('returns an empty array for undefined, null or blank input', () => {
		expect(splitClassNames(undefined)).to.deep.equal([]);
		expect(splitClassNames(null)).to.deep.equal([]);
		expect(splitClassNames('   ')).to.deep.equal([]);
	});
});

describe('hasClassNames', () => {
	it('compares whole class names, not substrings', () => {
		expect(hasClassNames('title--size-10', 'title--size-10')).to.equal(true);
		expect(hasClassNames('title--size-10', 'title--size-1')).to.equal(false);
	});

	it('requires every class name of a multi-class value, in any order', () => {
		expect(hasClassNames('list list--ordered', 'list list--ordered')).to.equal(true);
		expect(hasClassNames('list list--ordered', 'list--ordered list')).to.equal(true);
		expect(hasClassNames('list list--ordered', 'list list--unordered')).to.equal(false);
	});

	it('is false when the class is absent', () => {
		expect(hasClassNames(undefined, 'title--size-5')).to.equal(false);
		expect(hasClassNames('', 'title--size-5')).to.equal(false);
	});

	it('is true when no class names are requested', () => {
		expect(hasClassNames(undefined, '   ')).to.equal(true);
		expect(hasClassNames('anything', '')).to.equal(true);
	});
});
