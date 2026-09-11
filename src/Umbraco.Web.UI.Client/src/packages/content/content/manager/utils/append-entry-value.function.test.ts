import { expect } from '@open-wc/testing';
import { appendEntryValue } from './append-entry-value.function.js';
import type { UmbEntryValueModel } from '../../types.js';

function makeValue(alias: string, culture: string | null, value: unknown = alias): UmbEntryValueModel {
	return { editorAlias: 'Umbraco.TextBox', alias, culture, segment: null, value };
}

const uniqueByAliasAndCulture = (x: UmbEntryValueModel) => `${x.alias}|${x.culture}`;

describe('appendEntryValue', () => {
	it('inserts a new entry in culture order rather than appending it at the end', () => {
		const values = [makeValue('title', null), makeValue('title', 'en-us')];
		const newEntry = makeValue('title', 'da-dk');

		const result = appendEntryValue(values, newEntry, uniqueByAliasAndCulture);

		expect(result.map((v) => v.culture)).to.deep.equal([null, 'da-dk', 'en-us']);
	});

	it('replaces a matching existing entry in place, without re-sorting the rest of the array', () => {
		// Deliberately out of culture order, as could happen from data loaded elsewhere - replacing an
		// existing entry should not silently reorder values the user didn't just change. [NL]
		const values = [makeValue('title', 'en-us'), makeValue('title', null), makeValue('title', 'da-dk')];
		const updatedEntry = makeValue('title', null, 'updated value');

		const result = appendEntryValue(values, updatedEntry, uniqueByAliasAndCulture);

		expect(result.map((v) => v.culture)).to.deep.equal(['en-us', null, 'da-dk']);
		expect(result[1].value).to.equal('updated value');
	});

	it('does not mutate the input array', () => {
		const values = [makeValue('title', 'en-us')];

		appendEntryValue(values, makeValue('title', null), uniqueByAliasAndCulture);

		expect(values.map((v) => v.culture)).to.deep.equal(['en-us']);
	});
});
