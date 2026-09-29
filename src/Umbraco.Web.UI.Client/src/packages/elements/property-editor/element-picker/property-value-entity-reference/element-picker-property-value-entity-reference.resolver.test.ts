import { UmbElementPickerPropertyValueEntityReferenceResolver } from './element-picker-property-value-entity-reference.resolver.js';
import { expect } from '@open-wc/testing';
import type { UmbPropertyValueData } from '@umbraco-cms/backoffice/property';

function createValue(value: Array<string> | undefined): UmbPropertyValueData<Array<string> | undefined> {
	return { alias: 'picker', value } as UmbPropertyValueData<Array<string> | undefined>;
}

describe('UmbElementPickerPropertyValueEntityReferenceResolver', () => {
	let resolver: UmbElementPickerPropertyValueEntityReferenceResolver;

	beforeEach(() => {
		resolver = new UmbElementPickerPropertyValueEntityReferenceResolver();
	});

	it('resolves nothing when nothing is picked', async () => {
		expect(await resolver.resolveEntityReferences(createValue(undefined))).to.deep.equal([]);
		expect(await resolver.resolveEntityReferences(createValue([]))).to.deep.equal([]);
	});

	it('resolves each picked element as an element entity reference, in order', async () => {
		const result = await resolver.resolveEntityReferences(createValue(['element-b', 'element-a']));

		expect(result).to.deep.equal([
			{ entityType: 'element', unique: 'element-b' },
			{ entityType: 'element', unique: 'element-a' },
		]);
	});
});
