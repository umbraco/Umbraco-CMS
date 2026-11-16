import { expect } from '@open-wc/testing';
import { findExternalContentVariant } from './block-manager-external-content.function.js';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

function variant(culture: string | null, name: string, state = 'Published') {
	return { culture, state, name };
}

describe('findExternalContentVariant', () => {
	it('returns undefined when there are no variants', () => {
		expect(findExternalContentVariant(undefined, undefined)).to.be.undefined;
		expect(findExternalContentVariant([], undefined)).to.be.undefined;
	});

	it('returns the first variant when there is no variant to match against', () => {
		const variants = [variant('en-us', 'English name'), variant('da-dk', 'Danish name')];
		expect(findExternalContentVariant(variants, undefined)?.name).to.equal('English name');
	});

	it('returns the variant matching the given culture', () => {
		const variants = [variant('en-us', 'English name'), variant('da-dk', 'Danish name')];
		const variantId = new UmbVariantId('da-dk', null);
		expect(findExternalContentVariant(variants, variantId)?.name).to.equal('Danish name');
	});

	it('falls back to the first variant when no variant matches the given culture', () => {
		const variants = [variant('en-us', 'English name')];
		const variantId = new UmbVariantId('da-dk', null);
		expect(findExternalContentVariant(variants, variantId)?.name).to.equal('English name');
	});
});
