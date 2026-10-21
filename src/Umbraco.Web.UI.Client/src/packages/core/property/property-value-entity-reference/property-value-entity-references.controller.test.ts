import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { ManifestPropertyValueResolver, UmbPropertyValueData, UmbPropertyValueResolver } from '../types.js';
import type { UmbVariantDataModel } from '@umbraco-cms/backoffice/variant';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { ManifestPropertyValueEntityReference, UmbPropertyValueEntityReferenceResolver } from './types.js';
import { UmbPropertyValueEntityReferencesController } from './property-value-entity-references.controller.js';

@customElement('umb-test-entity-reference-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

type TestPropertyValueType = {
	uniques?: Array<string>;
	nestedValue?: UmbPropertyValueData;
};

// Nests one level deep, so the flat-mapper walk it drives can be exercised.
class TestNestingResolver
	implements UmbPropertyValueResolver<UmbPropertyValueData<TestPropertyValueType>, UmbPropertyValueData>
{
	async processValues(
		property: UmbPropertyValueData<TestPropertyValueType>,
		valuesCallback: (values: Array<UmbPropertyValueData>) => Promise<Array<UmbPropertyValueData> | undefined>,
	) {
		const nestedValue = property.value?.nestedValue;
		if (nestedValue) await valuesCallback([nestedValue]);
		return property;
	}
	destroy(): void {}
}

// Resolves each unique in `value.uniques` to a fake element entity reference.
class TestEntityReferenceResolver implements UmbPropertyValueEntityReferenceResolver {
	async resolveEntityReferences(value: UmbPropertyValueData<TestPropertyValueType>): Promise<Array<UmbEntityModel>> {
		return (value.value?.uniques ?? []).map((unique) => ({ entityType: 'element', unique }));
	}
	destroy(): void {}
}

describe('UmbPropertyValueEntityReferencesController', () => {
	beforeEach(() => {
		const resolverManifest: ManifestPropertyValueResolver = {
			type: 'propertyValueResolver',
			name: 'test-nesting-resolver',
			alias: 'Umb.Test.EntityReference.NestingResolver',
			api: TestNestingResolver,
			forEditorAlias: 'test-editor',
		};
		const entityReferenceManifest: ManifestPropertyValueEntityReference = {
			type: 'propertyValueEntityReference',
			name: 'test-entity-reference-resolver',
			alias: 'Umb.Test.EntityReference.Resolver',
			api: TestEntityReferenceResolver,
			forEditorAlias: 'test-editor',
		};
		umbExtensionsRegistry.register(resolverManifest);
		umbExtensionsRegistry.register(entityReferenceManifest);
	});

	afterEach(() => {
		umbExtensionsRegistry.unregister('Umb.Test.EntityReference.NestingResolver');
		umbExtensionsRegistry.unregister('Umb.Test.EntityReference.Resolver');
	});

	it('resolves nothing for a value without an editor alias', async () => {
		const host = new UmbTestControllerHostElement();
		const controller = new UmbPropertyValueEntityReferencesController(host);

		const result = await controller.resolve({ alias: 'test', value: { uniques: ['one'] } });

		expect(result).to.deep.equal([]);
	});

	it('resolves nothing when no resolver is registered for the editor alias', async () => {
		const host = new UmbTestControllerHostElement();
		const controller = new UmbPropertyValueEntityReferencesController(host);

		const result = await controller.resolve({
			editorAlias: 'test-editor-with-no-resolver',
			alias: 'test',
			value: { uniques: ['one'] },
		});

		expect(result).to.deep.equal([]);
	});

	it('resolves the entities referenced by the value', async () => {
		const host = new UmbTestControllerHostElement();
		const controller = new UmbPropertyValueEntityReferencesController(host);

		const result = await controller.resolve({
			editorAlias: 'test-editor',
			alias: 'test',
			value: { uniques: ['one', 'two'] },
		});

		expect(result).to.deep.equal([
			{ entityType: 'element', unique: 'one' },
			{ entityType: 'element', unique: 'two' },
		]);
	});

	it('resolves references from nested property values too', async () => {
		const host = new UmbTestControllerHostElement();
		const controller = new UmbPropertyValueEntityReferencesController(host);

		const result = await controller.resolve({
			editorAlias: 'test-editor',
			alias: 'test',
			value: {
				uniques: ['one'],
				nestedValue: {
					editorAlias: 'test-editor',
					alias: 'nested',
					value: { uniques: ['two'] },
				} as UmbPropertyValueData,
			},
		});

		expect(result).to.deep.equal([
			{ entityType: 'element', unique: 'one' },
			{ entityType: 'element', unique: 'two' },
		]);
	});

	it('deduplicates references by entity type and unique', async () => {
		const host = new UmbTestControllerHostElement();
		const controller = new UmbPropertyValueEntityReferencesController(host);

		const result = await controller.resolve({
			editorAlias: 'test-editor',
			alias: 'test',
			value: {
				uniques: ['one'],
				nestedValue: {
					editorAlias: 'test-editor',
					alias: 'nested',
					value: { uniques: ['one'] },
				} as UmbPropertyValueData,
			},
		});

		expect(result).to.deep.equal([{ entityType: 'element', unique: 'one' }]);
	});
});
