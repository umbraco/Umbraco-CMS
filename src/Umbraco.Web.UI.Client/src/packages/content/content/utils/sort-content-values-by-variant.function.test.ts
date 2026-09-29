import { expect } from '@open-wc/testing';
import { sortContentValuesByVariant } from './sort-content-values-by-variant.function.js';
import type { UmbPotentialContentValueModel } from '../types.js';

function makeValue(
	alias: string,
	culture: string | null,
	segment: string | null = null,
): UmbPotentialContentValueModel {
	return { editorAlias: 'Umbraco.TextBox', alias, culture, segment, value: alias };
}

describe('sortContentValuesByVariant', () => {
	it('sorts invariant values first, then culture codes ordinally', () => {
		const values = [makeValue('title', 'en-us'), makeValue('title', null), makeValue('title', 'da-dk')];

		const result = sortContentValuesByVariant(values).map((v) => v.culture);

		expect(result).to.deep.equal([null, 'da-dk', 'en-us']);
	});

	it('compares culture codes case-insensitively', () => {
		const values = [makeValue('title', 'DA-DK'), makeValue('title', 'en-us')];

		const result = sortContentValuesByVariant(values).map((v) => v.culture);

		expect(result).to.deep.equal(['DA-DK', 'en-us']);
	});

	it('sorts by segment within the same culture, invariant segment first', () => {
		const values = [
			makeValue('title', 'en-us', 's2'),
			makeValue('title', 'en-us', null),
			makeValue('title', 'en-us', 's1'),
		];

		const result = sortContentValuesByVariant(values).map((v) => v.segment);

		expect(result).to.deep.equal([null, 's1', 's2']);
	});

	it('keeps values with the same culture and segment in their original relative order', () => {
		const values = [makeValue('subtitle', 'da-dk'), makeValue('title', 'da-dk'), makeValue('body', 'en-us')];

		const result = sortContentValuesByVariant(values).map((v) => v.alias);

		expect(result).to.deep.equal(['subtitle', 'title', 'body']);
	});

	it('does not mutate the input array', () => {
		const values = [makeValue('title', 'en-us'), makeValue('title', null)];

		sortContentValuesByVariant(values);

		expect(values.map((v) => v.culture)).to.deep.equal(['en-us', null]);
	});
});
