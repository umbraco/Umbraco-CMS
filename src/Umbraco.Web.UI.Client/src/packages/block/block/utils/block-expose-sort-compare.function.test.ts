import { expect } from '@open-wc/testing';
import type { UmbBlockExposeModel } from '../types.js';
import { umbBlockExposeSortCompare } from './block-expose-sort-compare.function.js';

const expose = (contentKey: string, culture: string | null, segment: string | null = null): UmbBlockExposeModel => ({
	contentKey,
	culture,
	segment,
});

describe('umbBlockExposeSortCompare', () => {
	it('sorts by culture, then segment, then content key', () => {
		const result = [
			expose('b', 'en-us'),
			expose('a', 'en-us', 's1'),
			expose('b', null),
			expose('a', 'en-us'),
			expose('a', null),
		]
			.sort(umbBlockExposeSortCompare)
			.map((x) => `${x.culture}|${x.segment}|${x.contentKey}`);

		expect(result).to.deep.equal(['null|null|a', 'null|null|b', 'en-us|null|a', 'en-us|null|b', 'en-us|s1|a']);
	});
});
