import { resolveCollectionOrderByAlias } from './resolve-collection-order-by-alias.function.js';
import { expect } from '@open-wc/testing';

describe('resolveCollectionOrderByAlias', () => {
	const aliases = ['name', 'sortOrder', 'updateDate', 'creator'];

	it('returns the alias that matches exactly', () => {
		expect(resolveCollectionOrderByAlias('sortOrder', aliases)).to.equal('sortOrder');
	});

	it('returns the configured alias when the value differs only in case', () => {
		expect(resolveCollectionOrderByAlias('SortOrder', aliases)).to.equal('sortOrder');
	});

	it('returns the creator alias for a value of owner', () => {
		expect(resolveCollectionOrderByAlias('owner', aliases)).to.equal('creator');
	});

	it('prefers an owner alias over the creator fallback when one is configured', () => {
		expect(resolveCollectionOrderByAlias('owner', ['owner', 'creator'])).to.equal('owner');
	});

	it('returns undefined when no alias matches', () => {
		expect(resolveCollectionOrderByAlias('price', aliases)).to.be.undefined;
	});

	it('returns undefined when there is no value', () => {
		expect(resolveCollectionOrderByAlias(undefined, aliases)).to.be.undefined;
		expect(resolveCollectionOrderByAlias('', aliases)).to.be.undefined;
	});
});
