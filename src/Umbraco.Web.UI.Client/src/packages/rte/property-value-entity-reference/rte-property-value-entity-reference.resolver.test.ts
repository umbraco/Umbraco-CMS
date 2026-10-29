import type { UmbPropertyEditorRteValueType } from '../types.js';
import { UmbRtePropertyValueEntityReferenceResolver } from './rte-property-value-entity-reference.resolver.js';
import { expect } from '@open-wc/testing';
import type {
	ManifestPropertyValueEntityReference,
	UmbPropertyValueData,
} from '@umbraco-cms/backoffice/property';

const SCHEMA_ALIAS = 'Umbraco.Test.RichText';

function createResolver(forEditorAlias: string | undefined = SCHEMA_ALIAS) {
	const resolver = new UmbRtePropertyValueEntityReferenceResolver();
	resolver.manifest = { forEditorAlias } as ManifestPropertyValueEntityReference;
	return resolver;
}

function createValue(
	layout: Array<{ contentKey: string; isExternalContent?: boolean }> | undefined,
	alias = SCHEMA_ALIAS,
): UmbPropertyValueData<UmbPropertyEditorRteValueType> {
	return {
		alias: 'rte',
		value: {
			markup: '',
			blocks: layout
				? {
						layout: { [alias]: layout.map((entry) => ({ key: `key-${entry.contentKey}`, ...entry })) },
						contentData: [],
						settingsData: [],
						expose: [],
					}
				: undefined,
		},
	} as UmbPropertyValueData<UmbPropertyEditorRteValueType>;
}

describe('UmbRtePropertyValueEntityReferenceResolver', () => {
	it('resolves nothing when the manifest has no forEditorAlias', async () => {
		const result = await createResolver('').resolveEntityReferences(
			createValue([{ contentKey: 'a', isExternalContent: true }]),
		);
		expect(result).to.deep.equal([]);
	});

	it('resolves nothing when the value has no blocks', async () => {
		const result = await createResolver().resolveEntityReferences(createValue(undefined));
		expect(result).to.deep.equal([]);
	});

	it('resolves nothing when the value is empty', async () => {
		const result = await createResolver().resolveEntityReferences({ alias: 'rte', value: undefined } as never);
		expect(result).to.deep.equal([]);
	});

	it('resolves nothing when the blocks have no layout for its schema alias', async () => {
		const result = await createResolver().resolveEntityReferences(
			createValue([{ contentKey: 'a', isExternalContent: true }], 'Other.Alias'),
		);
		expect(result).to.deep.equal([]);
	});

	it('resolves only inline blocks that reference external content', async () => {
		const result = await createResolver().resolveEntityReferences(
			createValue([
				{ contentKey: 'owned' },
				{ contentKey: 'owned-explicit', isExternalContent: false },
				{ contentKey: 'shared', isExternalContent: true },
			]),
		);

		expect(result).to.deep.equal([{ entityType: 'element', unique: 'shared' }]);
	});

	it('ignores external content without a content key', async () => {
		const result = await createResolver().resolveEntityReferences(
			createValue([{ contentKey: '', isExternalContent: true }]),
		);
		expect(result).to.deep.equal([]);
	});
});
