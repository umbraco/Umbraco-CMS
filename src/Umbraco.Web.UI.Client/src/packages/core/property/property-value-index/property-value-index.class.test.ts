import { UmbPropertyValueIndex } from './property-value-index.class.js';
import { expect } from '@open-wc/testing';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

describe('UmbPropertyValueIndex', () => {
	it('returns the first value when several share the same alias and variant', () => {
		const index = new UmbPropertyValueIndex([
			{ alias: 'a', value: 'first', culture: 'cultureA', segment: null },
			{ alias: 'a', value: 'second', culture: 'cultureA', segment: null },
		]);

		expect(index.find('a', new UmbVariantId('cultureA', null))?.value).to.equal('first');
	});

	it('returns the first value for an alias when no variant is given, regardless of variant', () => {
		const index = new UmbPropertyValueIndex([
			{ alias: 'a', value: 'first', culture: 'cultureB', segment: 'segmentA' },
			{ alias: 'a', value: 'second', culture: null, segment: null },
		]);

		expect(index.find('a')?.value).to.equal('first');
	});

	it('distinguishes values by alias, culture and segment', () => {
		const index = new UmbPropertyValueIndex([
			{ alias: 'a', value: 'invariant', culture: null, segment: null },
			{ alias: 'a', value: 'cultureA', culture: 'cultureA', segment: null },
			{ alias: 'a', value: 'cultureA segmentA', culture: 'cultureA', segment: 'segmentA' },
			{ alias: 'b', value: 'other alias', culture: null, segment: null },
		]);

		expect(index.find('a', new UmbVariantId(null, null))?.value).to.equal('invariant');
		expect(index.find('a', new UmbVariantId('cultureA', null))?.value).to.equal('cultureA');
		expect(index.find('a', new UmbVariantId('cultureA', 'segmentA'))?.value).to.equal('cultureA segmentA');
		expect(index.find('b', new UmbVariantId(null, null))?.value).to.equal('other alias');
	});

	it('returns undefined when nothing matches', () => {
		const index = new UmbPropertyValueIndex([{ alias: 'a', value: 'x', culture: 'cultureA', segment: null }]);

		expect(index.find('missing')).to.be.undefined;
		expect(index.find('missing', new UmbVariantId('cultureA', null))).to.be.undefined;
		expect(index.find('a', new UmbVariantId('cultureB', null))).to.be.undefined;
		expect(index.find('a', new UmbVariantId('cultureA', 'segmentA'))).to.be.undefined;
	});

	it('treats values without a culture or segment as invariant', () => {
		const index = new UmbPropertyValueIndex([{ alias: 'a', value: 'no variant' }]);

		expect(index.find('a', new UmbVariantId(null, null))?.value).to.equal('no variant');
		expect(index.find('a', new UmbVariantId('cultureA', null))).to.be.undefined;
	});

	it('treats the "invariant" culture as no culture', () => {
		const index = new UmbPropertyValueIndex([{ alias: 'a', value: 'x', culture: 'invariant', segment: null }]);

		expect(index.find('a', new UmbVariantId(null, null))?.value).to.equal('x');
	});

	it('does not confuse a culture containing an underscore with a culture and segment', () => {
		const index = new UmbPropertyValueIndex([{ alias: 'a', value: 'x', culture: 'a_b', segment: null }]);

		expect(index.find('a', new UmbVariantId('a', 'b'))).to.be.undefined;
		expect(index.find('a', new UmbVariantId('a_b', null))?.value).to.equal('x');
	});

	it('handles an empty list', () => {
		const index = new UmbPropertyValueIndex([]);

		expect(index.find('a')).to.be.undefined;
		expect(index.find('a', new UmbVariantId(null, null))).to.be.undefined;
	});
});
