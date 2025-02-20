import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type {
	ManifestPropertyValuePreset,
	UmbPropertyTypePresetModel,
	UmbPropertyTypePresetWithSchemaAliasModel,
	UmbPropertyValuePresetApi,
} from './types.js';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';
import { UmbPropertyValuePresetBuilderController } from './property-value-preset-builder.controller.js';

@customElement('umb-test-controller-host')
export class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

export class TestPropertyValuePresetFirstApi implements UmbPropertyValuePresetApi<string, UmbPropertyEditorConfig> {
	async processValue(value: string, config: UmbPropertyEditorConfig) {
		return value ? value + '_first' : 'first';
	}

	destroy(): void {}
}

export class TestPropertyValuePresetSecondApi implements UmbPropertyValuePresetApi<string, UmbPropertyEditorConfig> {
	async processValue(value: string, config: UmbPropertyEditorConfig) {
		return value ? value + '_second' : 'second';
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
				forPropertyEditorUiAlias: 'test-editor',
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
					propertyEditorUiAlias: 'test-editor',
					config: [],
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
				api: TestPropertyValuePresetFirstApi,
				forPropertyEditorUiAlias: 'test-editor',
			};

			const manifestSecondPreset: ManifestPropertyValuePreset = {
				type: 'propertyValuePreset',
				name: 'test-preset-2',
				alias: 'Umb.Test.Preset.2',
				api: TestPropertyValuePresetSecondApi,
				forPropertyEditorUiAlias: 'test-editor',
			};

			umbExtensionsRegistry.register(manifestFirstPreset);
			umbExtensionsRegistry.register(manifestSecondPreset);
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
					propertyEditorUiAlias: 'test-editor',
					config: [],
				},
			];

			const result = await ctrl.create(propertyTypes);

			expect(result.length).to.be.equal(1);
			expect(result[0]?.value).to.be.equal('first_second');
		});
	});
});
