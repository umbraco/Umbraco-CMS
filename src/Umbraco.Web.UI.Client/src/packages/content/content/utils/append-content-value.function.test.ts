import { expect } from '@open-wc/testing';
import { umbAppendContentValue } from './append-content-value.function.js';
import type { UmbPotentialContentValueModel } from '../types.js';

function makeValue(
	alias: string,
	culture: string | null,
	segment: string | null = null,
	value: unknown = alias,
): UmbPotentialContentValueModel {
	return { editorAlias: 'Umbraco.TextBox', alias, culture, segment, value };
}

const uniqueByAliasAndVariant = (x: UmbPotentialContentValueModel) => `${x.alias}|${x.culture}|${x.segment}`;

describe('umbAppendContentValue', () => {
	it('inserts a new entry in variant order rather than appending it at the end', () => {
		const values = [makeValue('title', null), makeValue('title', 'en-us')];
		const newEntry = makeValue('title', 'da-dk');

		const result = umbAppendContentValue(values, newEntry, uniqueByAliasAndVariant);

		expect(result.map((v) => v.culture)).to.deep.equal([null, 'da-dk', 'en-us']);
	});

	it('replaces a matching existing entry in place, without re-sorting the rest of the array', () => {
		// Deliberately out of variant order, as could happen from data loaded elsewhere - replacing an
		// existing entry should not silently reorder values the user didn't just change. [NL]
		const values = [makeValue('title', 'en-us'), makeValue('title', null), makeValue('title', 'da-dk')];
		const updatedEntry = makeValue('title', null, null, 'updated value');

		const result = umbAppendContentValue(values, updatedEntry, uniqueByAliasAndVariant);

		expect(result.map((v) => v.culture)).to.deep.equal(['en-us', null, 'da-dk']);
		expect(result[1].value).to.equal('updated value');
	});

	it('does not mutate the input array', () => {
		const values = [makeValue('title', 'en-us')];

		umbAppendContentValue(values, makeValue('title', null), uniqueByAliasAndVariant);

		expect(values.map((v) => v.culture)).to.deep.equal(['en-us']);
	});
});
