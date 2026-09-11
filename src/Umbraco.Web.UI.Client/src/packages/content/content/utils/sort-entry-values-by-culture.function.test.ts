import { expect } from '@open-wc/testing';
import { sortEntryValuesByCulture } from './sort-entry-values-by-culture.function.js';
import type { UmbEntryValueModel } from '../types.js';

function makeValue(alias: string, culture: string | null): UmbEntryValueModel {
	return { editorAlias: 'Umbraco.TextBox', alias, culture, segment: null, value: alias };
}

describe('sortEntryValuesByCulture', () => {
	it('sorts invariant values first, then culture codes ordinally', () => {
		const values = [makeValue('title', 'en-us'), makeValue('title', null), makeValue('title', 'da-dk')];

		const result = sortEntryValuesByCulture(values).map((v) => v.culture);

		expect(result).to.deep.equal([null, 'da-dk', 'en-us']);
	});

	it('compares culture codes case-insensitively', () => {
		const values = [makeValue('title', 'DA-DK'), makeValue('title', 'en-us')];

		const result = sortEntryValuesByCulture(values).map((v) => v.culture);

		expect(result).to.deep.equal(['DA-DK', 'en-us']);
	});

	it('keeps values with the same culture in their original relative order', () => {
		const values = [makeValue('subtitle', 'da-dk'), makeValue('title', 'da-dk'), makeValue('body', 'en-us')];

		const result = sortEntryValuesByCulture(values).map((v) => v.alias);

		expect(result).to.deep.equal(['subtitle', 'title', 'body']);
	});

	it('does not mutate the input array', () => {
		const values = [makeValue('title', 'en-us'), makeValue('title', null)];

		sortEntryValuesByCulture(values);

		expect(values.map((v) => v.culture)).to.deep.equal(['en-us', null]);
	});
});
