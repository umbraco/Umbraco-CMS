import { expect } from '@open-wc/testing';
import type { UmbBlockExposeModel } from '../types.js';
import { umbBlockExposeSortCompare } from './block-expose-sort-compare.function.js';

const expose = (contentKey: string, culture: string | null): UmbBlockExposeModel => ({
	contentKey,
	culture,
});

describe('umbBlockExposeSortCompare', () => {
	it('sorts by culture, then content key', () => {
		const result = [expose('b', 'en-us'), expose('b', null), expose('a', 'en-us'), expose('a', null)]
			.sort(umbBlockExposeSortCompare)
			.map((x) => `${x.culture}|${x.contentKey}`);

		expect(result).to.deep.equal(['null|a', 'null|b', 'en-us|a', 'en-us|b']);
	});
});
