import { expect } from '@open-wc/testing';
import { umbGetContentPropertyValidationItems } from './content-property-validation-items.function.js';

describe('umbGetContentPropertyValidationItems', () => {
	it('returns an empty array when there are no properties', () => {
		const result = umbGetContentPropertyValidationItems(
			[],
			[
				{ culture: 'en-US', segment: null },
				{ culture: 'da', segment: null },
			],
		);
		expect(result).to.deep.equal([]);
	});

	it('returns an empty array when there are no variant options', () => {
		const result = umbGetContentPropertyValidationItems([{ alias: 'title' }], []);
		expect(result).to.deep.equal([]);
	});

	it('collapses an invariant property to a single invariant entry, regardless of how many culture options exist', () => {
		const result = umbGetContentPropertyValidationItems(
			[{ alias: 'title', variesByCulture: false, variesBySegment: false }],
			[
				{ culture: null, segment: null },
				{ culture: 'en-US', segment: null },
				{ culture: 'da', segment: null },
			],
		);

		expect(result.length).to.equal(1);
		expect(result[0].alias).to.equal('title');
		expect(result[0].variantId.culture).to.equal(null);
		expect(result[0].variantId.segment).to.equal(null);
	});

	it('skips the invariant/null-culture option for a culture-varying property', () => {
		const result = umbGetContentPropertyValidationItems(
			[{ alias: 'title', variesByCulture: true, variesBySegment: false }],
			[
				{ culture: null, segment: null },
				{ culture: 'en-US', segment: null },
				{ culture: 'da', segment: null },
			],
		);

		expect(result.length).to.equal(2);
		expect(result.map((r) => r.variantId.culture)).to.have.members(['en-US', 'da']);
		expect(result.every((r) => r.variantId.segment === null)).to.be.true;
	});

	it('fans out a segment-varying property per segment', () => {
		const result = umbGetContentPropertyValidationItems(
			[{ alias: 'title', variesByCulture: false, variesBySegment: true }],
			[
				{ culture: null, segment: null },
				{ culture: null, segment: 's1' },
				{ culture: null, segment: 's2' },
			],
		);

		expect(result.length).to.equal(3);
		expect(result.map((r) => r.variantId.segment)).to.have.members([null, 's1', 's2']);
		expect(result.every((r) => r.variantId.culture === null)).to.be.true;
	});

	it('fans out a culture-and-segment-varying property per combination, and dedups repeats', () => {
		const result = umbGetContentPropertyValidationItems(
			[{ alias: 'title', variesByCulture: true, variesBySegment: true }],
			[
				{ culture: null, segment: null }, // skipped: culture-varying property, invariant culture option
				{ culture: 'en-US', segment: null },
				{ culture: 'en-US', segment: 's1' },
				{ culture: 'en-US', segment: 's1' }, // duplicate combination, should not produce a second entry
				{ culture: 'da', segment: null },
			],
		);

		expect(result.length).to.equal(3);
		const keys = result.map((r) => `${r.variantId.culture}_${r.variantId.segment}`);
		expect(keys).to.have.members(['en-US_null', 'en-US_s1', 'da_null']);
	});

	it('handles multiple properties independently', () => {
		const result = umbGetContentPropertyValidationItems(
			[
				{ alias: 'title', variesByCulture: false, variesBySegment: false },
				{ alias: 'heading', variesByCulture: true, variesBySegment: false },
			],
			[
				{ culture: null, segment: null },
				{ culture: 'en-US', segment: null },
			],
		);

		expect(result.length).to.equal(2);
		const title = result.find((r) => r.alias === 'title');
		const heading = result.find((r) => r.alias === 'heading');
		expect(title?.variantId.culture).to.equal(null);
		expect(heading?.variantId.culture).to.equal('en-US');
	});
});
