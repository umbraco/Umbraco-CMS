import { mergeMediaCollectionSortPreference } from './merge-collection-sort-preference.function.js';
import { expect } from '@open-wc/testing';

describe('mergeMediaCollectionSortPreference', () => {
	it('applies the user preference over the collection configuration', () => {
		const result = mergeMediaCollectionSortPreference(
			{ orderBy: 'name', orderDirection: 'asc', pageSize: 50 },
			{ orderBy: 'updateDate', orderDirection: 'desc' },
		);

		expect(result.orderBy).to.equal('updateDate');
		expect(result.orderDirection).to.equal('desc');
		expect(result.pageSize).to.equal(50);
	});

	it('returns the original configuration when no preference is stored', () => {
		const config = { orderBy: 'name', orderDirection: 'asc', pageSize: 50 };

		expect(mergeMediaCollectionSortPreference(config, undefined)).to.equal(config);
		expect(mergeMediaCollectionSortPreference(config, null)).to.equal(config);
	});
});
