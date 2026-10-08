import { expect } from '@open-wc/testing';
import { umbExpandVariantIdsWithSegmentOptions } from './expand-variant-ids-with-segment-options.function.js';
import { UmbVariantId } from './variant-id.class.js';
import type { UmbEntityVariantOptionModel } from './types.js';

function makeOption(culture: string | null, segment: string | null): UmbEntityVariantOptionModel {
	return {
		unique: new UmbVariantId(culture, segment).toString(),
		culture,
		segment,
		language: {
			entityType: 'language',
			unique: culture ?? 'en-US',
			name: culture ?? 'English (United States)',
			isDefault: culture === null,
			isMandatory: false,
			fallbackIsoCode: null,
		},
	};
}

describe('umbExpandVariantIdsWithSegmentOptions', () => {
	it('expands a culture variant into every segment option for that culture', () => {
		const variantOptions = [
			makeOption('en-US', null),
			makeOption('en-US', 's1'),
			makeOption('en-US', 's2'),
			makeOption('da', null),
			makeOption('da', 's1'),
		];

		const result = umbExpandVariantIdsWithSegmentOptions([UmbVariantId.Create({ culture: 'en-US', segment: null })], variantOptions);

		expect(result.map((x) => x.toString()).sort()).to.deep.equal(['en-US', 'en-US_s1', 'en-US_s2'].sort());
	});

	it('expands the invariant culture into its segment options too', () => {
		const variantOptions = [makeOption(null, null), makeOption(null, 's1'), makeOption(null, 's2')];

		const result = umbExpandVariantIdsWithSegmentOptions([UmbVariantId.CreateInvariant()], variantOptions);

		expect(result.map((x) => x.toString()).sort()).to.deep.equal(['invariant', 'invariant_s1', 'invariant_s2'].sort());
	});

	it('leaves a variant id that already targets a specific segment untouched', () => {
		const variantOptions = [makeOption('en-US', null), makeOption('en-US', 's1'), makeOption('en-US', 's2')];

		const result = umbExpandVariantIdsWithSegmentOptions(
			[UmbVariantId.Create({ culture: 'en-US', segment: 's1' })],
			variantOptions,
		);

		expect(result.length).to.equal(1);
		expect(result[0].culture).to.equal('en-US');
		expect(result[0].segment).to.equal('s1');
	});

	it('is a no-op when there is exactly one option per culture (no segment variance)', () => {
		const variantOptions = [makeOption('en-US', null), makeOption('da', null)];

		const variantIds = [UmbVariantId.Create({ culture: 'en-US', segment: null })];
		const result = umbExpandVariantIdsWithSegmentOptions(variantIds, variantOptions);

		expect(result.length).to.equal(1);
		expect(result[0].culture).to.equal('en-US');
		expect(result[0].segment).to.be.null;
	});

	it('does not affect a selected culture when expanding another selected culture', () => {
		const variantOptions = [
			makeOption('en-US', null),
			makeOption('en-US', 's1'),
			makeOption('da', null),
			makeOption('da', 's1'),
		];

		const result = umbExpandVariantIdsWithSegmentOptions(
			[
				UmbVariantId.Create({ culture: 'en-US', segment: null }),
				UmbVariantId.Create({ culture: 'da', segment: 's1' }),
			],
			variantOptions,
		);

		expect(result.map((x) => x.toString()).sort()).to.deep.equal(['en-US', 'en-US_s1', 'da_s1'].sort());
	});
});
