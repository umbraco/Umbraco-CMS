import { expect } from '@open-wc/testing';
import { umbVariantObjectSortCompare } from './variant-object-sort-compare.function.js';

const v = (culture: string | null, segment: string | null = null) => ({ culture, segment });

describe('umbVariantObjectSortCompare', () => {
	it('sorts invariant first, then culture codes ordinally', () => {
		const result = [v('en-us'), v(null), v('da-dk')].sort(umbVariantObjectSortCompare).map((x) => x.culture);
		expect(result).to.deep.equal([null, 'da-dk', 'en-us']);
	});

	it('compares case-sensitively, matching StringComparer.Ordinal', () => {
		const result = [v('en-us'), v('DA-DK')].sort(umbVariantObjectSortCompare).map((x) => x.culture);
		expect(result).to.deep.equal(['DA-DK', 'en-us']);
	});

	it('sorts by culture before segment, with the unsegmented variant first', () => {
		const result = [v('en-us', 's1'), v('da-dk', 's2'), v('en-us'), v('da-dk')]
			.sort(umbVariantObjectSortCompare)
			.map((x) => `${x.culture}|${x.segment}`);
		expect(result).to.deep.equal(['da-dk|null', 'da-dk|s2', 'en-us|null', 'en-us|s1']);
	});
});
