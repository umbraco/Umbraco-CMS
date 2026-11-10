import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type {
	ManifestPropertyValuePreset,
	UmbPropertyTypePresetModel,
	UmbPropertyTypePresetWithSchemaAliasModel,
	UmbPropertyValuePreset,
} from './types.js';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';
import { UmbPropertyValuePresetBuilderController } from './property-value-preset-builder.controller.js';

@customElement('umb-test-controller-host')
export class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

// TODO: Write test with config, investigate opportunity to retrieve Config Object, for an simpler DX. [NL]

// Test with async APIs, especially where the first one is slower than the last one.
export class TestPropertyValuePresetFirstApi implements UmbPropertyValuePreset<string, UmbPropertyEditorConfig> {
	async processValue(value: undefined | string, config: UmbPropertyEditorConfig) {
		return value ? value + '_first' : 'first';
	}

	destroy(): void {}
}

export class TestPropertyValuePresetSecondApi implements UmbPropertyValuePreset<string, UmbPropertyEditorConfig> {
	async processValue(value: undefined | string, config: UmbPropertyEditorConfig) {
		return value ? value + '_second' : 'second';
	}

	destroy(): void {}
}

export class TestPropertyValuePresetAsyncApi implements UmbPropertyValuePreset<string, UmbPropertyEditorConfig> {
	async processValue(value: undefined | string, config: UmbPropertyEditorConfig) {
		await new Promise((resolve) => setTimeout(resolve, 10));
		return value ? value + '_async' : 'async';
	}

	destroy(): void {}
}

describe('UmbPropertyValuePresetBuilderController', () => {
	describe('Create with a single preset', () => {
		beforeEach(async () => {
			const manifestFirstPreset: ManifestPropertyValuePreset = {
				type: 'propertyValuePreset',
				name: 'test-preset-1',
				alias: 'Umb.Test.Preset.1',
				api: TestPropertyValuePresetFirstApi,
				forPropertyEditorUiAlias: 'test-editor-ui',
			};

			umbExtensionsRegistry.register(manifestFirstPreset);
		});
		afterEach(async () => {
			umbExtensionsRegistry.unregister('Umb.Test.Preset.1');
		});

		it('creates value', async () => {
			const ctrlHost = new UmbTestControllerHostElement();
			const ctrl = new UmbPropertyValuePresetBuilderController(ctrlHost);

			const propertyTypes: Array<UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel> = [
				{
					alias: 'test',
					propertyEditorUiAlias: 'test-editor-ui',
					config: [],
					typeArgs: {},
				},
			];

			const result = await ctrl.create(propertyTypes, {
				entityUnique: 'test-unique',
				entityType: 'test',
			});

			expect(result.length).to.be.equal(1);
			expect(result[0]?.value).to.be.equal('first');
		});
	});

	describe('Create with a two presets', () => {
		beforeEach(async () => {
			const manifestFirstPreset: ManifestPropertyValuePreset = {
				type: 'propertyValuePreset',
				name: 'test-preset-1',
				alias: 'Umb.Test.Preset.1',
				weight: 20,
				api: TestPropertyValuePresetFirstApi,
				forPropertyEditorUiAlias: 'test-editor-ui',
			};

			const manifestSecondPreset: ManifestPropertyValuePreset = {
				type: 'propertyValuePreset',
				name: 'test-preset-2',
				alias: 'Umb.Test.Preset.2',
				weight: 10,
				api: TestPropertyValuePresetSecondApi,
				forPropertyEditorUiAlias: 'test-editor-ui',
			};

			umbExtensionsRegistry.register(manifestSecondPreset);
			umbExtensionsRegistry.register(manifestFirstPreset);
		});
		afterEach(async () => {
			umbExtensionsRegistry.unregister('Umb.Test.Preset.1');
			umbExtensionsRegistry.unregister('Umb.Test.Preset.2');
		});

		it('creates a combined value', async () => {
			const ctrlHost = new UmbTestControllerHostElement();
			const ctrl = new UmbPropertyValuePresetBuilderController(ctrlHost);

			const propertyTypes: Array<UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel> = [
				{
					alias: 'test',
					propertyEditorUiAlias: 'test-editor-ui',
					config: [],
					typeArgs: {},
				},
			];

			const result = await ctrl.create(propertyTypes, {
				entityUnique: 'test-unique',
				entityType: 'test',
			});

			expect(result.length).to.be.equal(1);
			expect(result[0]?.value).to.be.equal('first_second');
		});
	});

	describe('Different weight gets a different execution order', () => {
		beforeEach(async () => {
			const manifestFirstPreset: ManifestPropertyValuePreset = {
				type: 'propertyValuePreset',
				name: 'test-preset-1',
				alias: 'Umb.Test.Preset.1',
				weight: 20,
				api: TestPropertyValuePresetFirstApi,
				forPropertyEditorUiAlias: 'test-editor-ui',
			};

			const manifestSecondPreset: ManifestPropertyValuePreset = {
				type: 'propertyValuePreset',
				name: 'test-preset-2',
				alias: 'Umb.Test.Preset.2',
				weight: 3000,
				api: TestPropertyValuePresetSecondApi,
				forPropertyEditorUiAlias: 'test-editor-ui',
			};

			umbExtensionsRegistry.register(manifestSecondPreset);
			umbExtensionsRegistry.register(manifestFirstPreset);
		});
		afterEach(async () => {
			umbExtensionsRegistry.unregister('Umb.Test.Preset.1');
			umbExtensionsRegistry.unregister('Umb.Test.Preset.2');
		});

		it('creates a combined value', async () => {
			const ctrlHost = new UmbTestControllerHostElement();
			const ctrl = new UmbPropertyValuePresetBuilderController(ctrlHost);

			const propertyTypes: Array<UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel> = [
				{
					alias: 'test',
					propertyEditorUiAlias: 'test-editor-ui',
					config: [],
					typeArgs: {},
				},
			];

			const result = await ctrl.create(propertyTypes, {
				entityUnique: 'test-unique',
				entityType: 'test',
			});

			expect(result.length).to.be.equal(1);
			expect(result[0]?.value).to.be.equal('second_first');
		});
	});

	describe('weigthed async presets keeps the right order', () => {
		beforeEach(async () => {
			const manifestFirstPreset: ManifestPropertyValuePreset = {
				type: 'propertyValuePreset',
				name: 'test-preset-1',
				alias: 'Umb.Test.Preset.1',
				weight: 3,
				api: TestPropertyValuePresetFirstApi,
				forPropertyEditorUiAlias: 'test-editor-ui',
			};

			const manifestAsyncPreset: ManifestPropertyValuePreset = {
				type: 'propertyValuePreset',
				name: 'test-preset-3',
				alias: 'Umb.Test.Preset.3',
				weight: 2,
				api: TestPropertyValuePresetAsyncApi,
				forPropertyEditorUiAlias: 'test-editor-ui',
			};

			const manifestSecondPreset: ManifestPropertyValuePreset = {
				type: 'propertyValuePreset',
				name: 'test-preset-2',
				alias: 'Umb.Test.Preset.2',
				weight: 1,
				api: TestPropertyValuePresetSecondApi,
				forPropertyEditorUiAlias: 'test-editor-ui',
			};

			umbExtensionsRegistry.register(manifestSecondPreset);
			umbExtensionsRegistry.register(manifestAsyncPreset);
			umbExtensionsRegistry.register(manifestFirstPreset);
		});
		afterEach(async () => {
			umbExtensionsRegistry.unregister('Umb.Test.Preset.1');
			umbExtensionsRegistry.unregister('Umb.Test.Preset.2');
			umbExtensionsRegistry.unregister('Umb.Test.Preset.3');
		});

		it('creates a combined value', async () => {
			const ctrlHost = new UmbTestControllerHostElement();
			const ctrl = new UmbPropertyValuePresetBuilderController(ctrlHost);

			const propertyTypes: Array<UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel> = [
				{
					alias: 'test',
					propertyEditorUiAlias: 'test-editor-ui',
					config: [],
					typeArgs: {},
				},
				{
					alias: 'test2',
					propertyEditorUiAlias: 'test-editor-ui',
					config: [],
					typeArgs: {},
				},
			];

			const result = await ctrl.create(propertyTypes, {
				entityUnique: 'test-unique',
				entityType: 'test',
			});

			expect(result.length).to.be.equal(2);
			expect(result[0]?.alias).to.be.equal('test');
			expect(result[0]?.value).to.be.equal('first_async_second');
			expect(result[1]?.alias).to.be.equal('test2');
			expect(result[1]?.value).to.be.equal('first_async_second');
		});
	});

	describe('combine use of UI and Schema Presets', () => {
		beforeEach(async () => {
			const manifestFirstPreset: ManifestPropertyValuePreset = {
				type: 'propertyValuePreset',
				name: 'test-preset-1',
				alias: 'Umb.Test.Preset.1',
				weight: 3,
				api: TestPropertyValuePresetFirstApi,
				forPropertyEditorSchemaAlias: 'test-editor-schema',
			};

			const manifestAsyncPreset: ManifestPropertyValuePreset = {
				type: 'propertyValuePreset',
				name: 'test-preset-3',
				alias: 'Umb.Test.Preset.3',
				weight: 2,
				api: TestPropertyValuePresetAsyncApi,
				forPropertyEditorUiAlias: 'test-editor-ui',
				forPropertyEditorSchemaAlias: 'test-editor-schema',
			};

			const manifestSecondPreset: ManifestPropertyValuePreset = {
				type: 'propertyValuePreset',
				name: 'test-preset-2',
				alias: 'Umb.Test.Preset.2',
				weight: 1,
				api: TestPropertyValuePresetSecondApi,
				forPropertyEditorUiAlias: 'test-editor-ui',
			};

			umbExtensionsRegistry.register(manifestSecondPreset);
			umbExtensionsRegistry.register(manifestAsyncPreset);
			umbExtensionsRegistry.register(manifestFirstPreset);
		});
		afterEach(async () => {
			umbExtensionsRegistry.unregister('Umb.Test.Preset.1');
			umbExtensionsRegistry.unregister('Umb.Test.Preset.2');
			umbExtensionsRegistry.unregister('Umb.Test.Preset.3');
		});

		it('creates only presets that fits the configuration', async () => {
			const ctrlHost = new UmbTestControllerHostElement();
			const ctrl = new UmbPropertyValuePresetBuilderController(ctrlHost);

			const propertyTypes: Array<UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel> = [
				{
					alias: 'test',
					propertyEditorUiAlias: 'test-editor-ui',
					config: [],
					typeArgs: {},
				},
				{
					alias: 'test2',
					propertyEditorUiAlias: 'test-editor-ui',
					propertyEditorSchemaAlias: 'test-editor-schema',
					config: [],
					typeArgs: {},
				},
				{
					alias: 'test3',
					propertyEditorUiAlias: 'some-other-ui',
					propertyEditorSchemaAlias: 'test-editor-schema',
					config: [],
					typeArgs: {},
				},
			];

			const result = await ctrl.create(propertyTypes, {
				entityUnique: 'test-unique',
				entityType: 'test',
			});

			// Test that only the right presets are used:
			expect(result.length).to.be.equal(3);
			expect(result[0]?.alias).to.be.equal('test');
			expect(result[0]?.value).to.be.equal('async_second');
			expect(result[1]?.alias).to.be.equal('test2');
			expect(result[1]?.value).to.be.equal('first_async_second');
			expect(result[2]?.alias).to.be.equal('test3');
			expect(result[2]?.value).to.be.equal('first_async');
		});
	});
	describe('Looking up presets', () => {
		beforeEach(async () => {
			umbExtensionsRegistry.register({
				type: 'propertyValuePreset',
				name: 'test-preset-for-ui',
				alias: 'Umb.Test.Preset.ForUi',
				api: TestPropertyValuePresetFirstApi,
				forPropertyEditorUiAlias: 'test-editor-ui',
			} as ManifestPropertyValuePreset);
			umbExtensionsRegistry.register({
				type: 'propertyValuePreset',
				name: 'test-preset-for-schema',
				alias: 'Umb.Test.Preset.ForSchema',
				api: TestPropertyValuePresetSecondApi,
				forPropertyEditorSchemaAlias: 'test-editor-schema',
			} as ManifestPropertyValuePreset);
		});
		afterEach(async () => {
			umbExtensionsRegistry.unregister('Umb.Test.Preset.ForUi');
			umbExtensionsRegistry.unregister('Umb.Test.Preset.ForSchema');
		});

		const createArgs = { entityUnique: 'test-unique', entityType: 'test' };

		it('applies presets registered for the schema alias only when the property type has a schema alias', async () => {
			const withoutSchema = await new UmbPropertyValuePresetBuilderController(
				new UmbTestControllerHostElement(),
			).create(
				[{ alias: 'test', propertyEditorUiAlias: 'test-editor-ui-other', config: [], typeArgs: {} }],
				createArgs,
			);
			const withSchema = await new UmbPropertyValuePresetBuilderController(new UmbTestControllerHostElement()).create(
				[
					{
						alias: 'test',
						propertyEditorUiAlias: 'test-editor-ui-other',
						propertyEditorSchemaAlias: 'test-editor-schema',
						config: [],
						typeArgs: {},
					} as UmbPropertyTypePresetWithSchemaAliasModel,
				],
				createArgs,
			);

			expect(withoutSchema.length).to.equal(0);
			expect(withSchema.length).to.equal(1);
			expect(withSchema[0].value).to.equal('second');
		});

		it('searches the registry once for property types that share the same editors', async () => {
			const ctrl = new UmbPropertyValuePresetBuilderController(new UmbTestControllerHostElement());
			const original = umbExtensionsRegistry.getByTypeAndFilter;
			let searches = 0;
			umbExtensionsRegistry.getByTypeAndFilter = function (this: unknown, ...args: Array<never>) {
				if ((args[0] as string) === 'propertyValuePreset') searches++;
				return (original as (...a: Array<never>) => unknown).apply(umbExtensionsRegistry, args);
			} as typeof original;

			let result;
			try {
				result = await ctrl.create(
					[
						{ alias: 'a', propertyEditorUiAlias: 'test-editor-ui', config: [], typeArgs: {} },
						{ alias: 'b', propertyEditorUiAlias: 'test-editor-ui', config: [], typeArgs: {} },
						{ alias: 'c', propertyEditorUiAlias: 'test-editor-ui-other', config: [], typeArgs: {} },
					],
					createArgs,
				);
			} finally {
				umbExtensionsRegistry.getByTypeAndFilter = original;
			}

			expect(result.map((x) => x.alias)).to.deep.equal(['a', 'b']);
			expect(searches).to.equal(2);
		});
	});
});
