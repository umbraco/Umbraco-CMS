import { expect } from '@open-wc/testing';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { umbExtractVariantValues } from './merge-variant-values.function.js';

type TestValue = { alias: string; culture: string | null; segment: string | null; value: unknown };

describe('umbExtractVariantValues', () => {
	it('picks the value matching the property variant among multiple variants of the same alias', () => {
		// 'en' comes first and 'da' last, so a naive last-wins flatten would wrongly resolve to 'Danish'. [NL]
		const values: Array<TestValue> = [
			{ alias: 'title', culture: 'en', segment: null, value: 'English' },
			{ alias: 'title', culture: 'da', segment: null, value: 'Danish' },
		];

		const result = umbExtractVariantValues(
			[{ alias: 'title', variantId: UmbVariantId.Create({ culture: 'en', segment: null }) }],
			values,
		);

		expect(result.length).to.equal(1);
		expect(result[0].value).to.equal('English');
	});

	it('returns one entry per property, in property order', () => {
		const values: Array<TestValue> = [
			{ alias: 'body', culture: null, segment: null, value: 'B' },
			{ alias: 'title', culture: null, segment: null, value: 'T' },
		];

		const result = umbExtractVariantValues(
			[
				{ alias: 'title', variantId: UmbVariantId.CreateInvariant() },
				{ alias: 'body', variantId: UmbVariantId.CreateInvariant() },
			],
			values,
		);

		expect(result.map((x) => x.value)).to.eql(['T', 'B']);
	});

	it('skips properties that have no matching value', () => {
		const values: Array<TestValue> = [{ alias: 'title', culture: null, segment: null, value: 'T' }];

		const result = umbExtractVariantValues(
			[
				{ alias: 'title', variantId: UmbVariantId.CreateInvariant() },
				{ alias: 'missing', variantId: UmbVariantId.CreateInvariant() },
			],
			values,
		);

		expect(result.length).to.equal(1);
		expect(result[0].alias).to.equal('title');
	});

	it('returns an empty array when values are undefined', () => {
		const result = umbExtractVariantValues([{ alias: 'title', variantId: UmbVariantId.CreateInvariant() }], undefined);
		expect(result).to.eql([]);
	});
});
