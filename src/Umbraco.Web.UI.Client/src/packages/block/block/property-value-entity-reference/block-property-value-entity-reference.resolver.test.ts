import type { UmbBlockLayoutBaseModel, UmbBlockValueType } from '../types.js';
import { UmbBlockPropertyValueEntityReferenceResolver } from './block-property-value-entity-reference.resolver.js';
import { expect } from '@open-wc/testing';
import type {
	ManifestPropertyValueEntityReference,
	UmbPropertyValueData,
} from '@umbraco-cms/backoffice/property';

const SCHEMA_ALIAS = 'Umbraco.Test.BlockEditor';

interface TestLayoutEntry extends UmbBlockLayoutBaseModel {
	areas?: Array<{ items: Array<TestLayoutEntry> }>;
}

function createResolver(forEditorAlias: string | undefined = SCHEMA_ALIAS) {
	const resolver = new UmbBlockPropertyValueEntityReferenceResolver();
	resolver.manifest = { forEditorAlias } as ManifestPropertyValueEntityReference;
	return resolver;
}

function createValue(layout?: Array<TestLayoutEntry>, alias = SCHEMA_ALIAS): UmbPropertyValueData<UmbBlockValueType> {
	return {
		alias: 'blocks',
		value: {
			layout: layout ? { [alias]: layout } : {},
			contentData: [],
			settingsData: [],
			expose: [],
		},
	} as UmbPropertyValueData<UmbBlockValueType>;
}

function entry(contentKey: string, isExternalContent?: boolean, areas?: TestLayoutEntry['areas']): TestLayoutEntry {
	return { key: `key-${contentKey}`, contentKey, isExternalContent, areas };
}

describe('UmbBlockPropertyValueEntityReferenceResolver', () => {
	it('resolves nothing when the manifest has no forEditorAlias', async () => {
		const result = await createResolver('').resolveEntityReferences(createValue([entry('a', true)]));
		expect(result).to.deep.equal([]);
	});

	it('resolves nothing when the value has no layout for its schema alias', async () => {
		const result = await createResolver().resolveEntityReferences(createValue([entry('a', true)], 'Other.Alias'));
		expect(result).to.deep.equal([]);
	});

	it('resolves nothing when the value is empty', async () => {
		const result = await createResolver().resolveEntityReferences({ alias: 'blocks', value: undefined } as never);
		expect(result).to.deep.equal([]);
	});

	it('resolves only blocks that reference external content', async () => {
		const result = await createResolver().resolveEntityReferences(
			createValue([entry('owned'), entry('owned-explicit', false), entry('shared', true)]),
		);

		expect(result).to.deep.equal([{ entityType: 'element', unique: 'shared' }]);
	});

	it('ignores external content without a content key', async () => {
		const result = await createResolver().resolveEntityReferences(createValue([entry('', true)]));
		expect(result).to.deep.equal([]);
	});

	it('resolves external content nested in areas, at any depth', async () => {
		const result = await createResolver().resolveEntityReferences(
			createValue([
				entry('top', true, [
					{
						items: [
							entry('area-owned'),
							entry('area-shared', true, [{ items: [entry('deep-shared', true)] }]),
						],
					},
				]),
			]),
		);

		expect(result.map((x) => x.unique)).to.deep.equal(['top', 'area-shared', 'deep-shared']);
	});

	it('walks into areas of a block that is not itself external content', async () => {
		const result = await createResolver().resolveEntityReferences(
			createValue([entry('grid-owned', false, [{ items: [entry('shared-in-area', true)] }])]),
		);

		expect(result).to.deep.equal([{ entityType: 'element', unique: 'shared-in-area' }]);
	});
});
