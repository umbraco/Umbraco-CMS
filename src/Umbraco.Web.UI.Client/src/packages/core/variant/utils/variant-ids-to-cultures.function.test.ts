import { expect } from '@open-wc/testing';
import { umbVariantIdsToCultures } from './variant-ids-to-cultures.function.js';
import { UmbVariantId } from '../variant-id.class.js';

describe('umbVariantIdsToCultures', () => {
	it('returns each culture once, regardless of segments', () => {
		const result = umbVariantIdsToCultures([
			new UmbVariantId('en-us', null),
			new UmbVariantId('en-us', 'seg1'),
			new UmbVariantId('da-dk', 'seg1'),
			new UmbVariantId('en-us', 'seg2'),
		]);
		expect(result).to.deep.equal(['en-us', 'da-dk']);
	});

	it('keeps the invariant culture as null, once', () => {
		const result = umbVariantIdsToCultures([
			UmbVariantId.CreateInvariant(),
			new UmbVariantId(null, 'seg1'),
			new UmbVariantId('en-us', null),
		]);
		expect(result).to.deep.equal([null, 'en-us']);
	});

	it('skips undefined variant ids', () => {
		const result = umbVariantIdsToCultures([undefined, new UmbVariantId('en-us', null), undefined]);
		expect(result).to.deep.equal(['en-us']);
	});

	it('returns an empty array for no variant ids', () => {
		expect(umbVariantIdsToCultures([])).to.deep.equal([]);
	});
});
