import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type {
	ManifestPropertyValuePreset,
	UmbPropertyTypePresetModel,
	UmbPropertyTypePresetWithSchemaAliasModel,
	UmbPropertyValuePreset,
} from './types.js';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';
import { UmbPropertyValuePresetBuilderController } from './property-value-preset-builder.controller.js';
import { UmbControllerHostElementElement } from '@umbraco-cms/backoffice/controller-api';

// TODO: Write test with config, investigate oppertunity to retrieve Config Object, for an simpler DX. [NL]

// Test with async APIs, espcially where the first one is slower than the last one.
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
			const ctrlHost = new UmbControllerHostElementElement();
			const ctrl = new UmbPropertyValuePresetBuilderController(ctrlHost);

			const propertyTypes: Array<UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel> = [
				{
					alias: 'test',
					propertyEditorUiAlias: 'test-editor-ui',
					config: [],
					typeArgs: {},
				},
			];

			const result = await ctrl.create(propertyTypes);

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
			const ctrlHost = new UmbControllerHostElementElement();
			const ctrl = new UmbPropertyValuePresetBuilderController(ctrlHost);

			const propertyTypes: Array<UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel> = [
				{
					alias: 'test',
					propertyEditorUiAlias: 'test-editor-ui',
					config: [],
					typeArgs: {},
				},
			];

			const result = await ctrl.create(propertyTypes);

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
			const ctrlHost = new UmbControllerHostElementElement();
			const ctrl = new UmbPropertyValuePresetBuilderController(ctrlHost);

			const propertyTypes: Array<UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel> = [
				{
					alias: 'test',
					propertyEditorUiAlias: 'test-editor-ui',
					config: [],
					typeArgs: {},
				},
			];

			const result = await ctrl.create(propertyTypes);

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
			const ctrlHost = new UmbControllerHostElementElement();
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

			const result = await ctrl.create(propertyTypes);

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
			const ctrlHost = new UmbControllerHostElementElement();
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

			const result = await ctrl.create(propertyTypes);

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
});
