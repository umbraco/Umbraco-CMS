import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type {
	ManifestPropertyValuePreset,
	UmbPropertyTypePresetModel,
	UmbPropertyTypePresetModelTypeModel,
	UmbPropertyTypePresetWithSchemaAliasModel,
	UmbPropertyValuePreset,
	UmbPropertyValuePresetApiCallArgs,
} from './types.js';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';
import { UmbPropertyValuePresetVariantBuilderController } from './property-value-preset-variant-builder.controller.js';

@customElement('umb-test-controller-host')
export class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

// Test with async APIs, espcially where the first one is slower than the last one.
export class TestPropertyValuePresetFirstApi implements UmbPropertyValuePreset<string, UmbPropertyEditorConfig> {
	async processValue(
		value: undefined | string,
		config: UmbPropertyEditorConfig,
		typeArgs: UmbPropertyTypePresetModelTypeModel,
		callArgs: UmbPropertyValuePresetApiCallArgs,
	): Promise<string> {
		return value
			? value + '_' + 'value for variant ' + callArgs.variantId!.toString()
			: 'value for culture ' + callArgs.variantId?.toString();
	}

	destroy(): void {}
}

describe('UmbPropertyValuePresetVariantBuilderController', () => {
	describe('Create with a variant preset', () => {
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

		it('creates culture variant values', async () => {
			const ctrlHost = new UmbTestControllerHostElement();
			const ctrl = new UmbPropertyValuePresetVariantBuilderController(ctrlHost);
			ctrl.setCultures(['cultureA', 'cultureB']);

			const propertyTypes: Array<UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel> = [
				{
					alias: 'test',
					propertyEditorUiAlias: 'test-editor-ui',
					config: [],
					typeArgs: { variesByCulture: true },
				},
			];

			const result = await ctrl.create(propertyTypes, {
				entityType: 'test',
				entityUnique: 'some-unique',
			});

			expect(result.length).to.be.equal(2);
			expect(result[0]?.value).to.be.equal('value for culture cultureA');
			expect(result[0]?.culture).to.be.equal('cultureA');
			expect(result[1]?.value).to.be.equal('value for culture cultureB');
			expect(result[1]?.culture).to.be.equal('cultureB');
		});

		it('uses the preset value when creating a culture variant values', async () => {
			const ctrlHost = new UmbTestControllerHostElement();
			const ctrl = new UmbPropertyValuePresetVariantBuilderController(ctrlHost);
			ctrl.setCultures(['cultureA', 'cultureB']);
			ctrl.setValues([
				{
					alias: 'test',
					value: 'blueprint value for cultureB',
					culture: 'cultureB',
					segment: null,
					editorAlias: 'test-editor-schema',
				},
			]);

			const propertyTypes: Array<UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel> = [
				{
					alias: 'test',
					propertyEditorUiAlias: 'test-editor-ui',
					propertyEditorSchemaAlias: 'test-editor-schema',
					config: [],
					typeArgs: { variesByCulture: true },
				},
			];

			const result = await ctrl.create(propertyTypes, {
				entityType: 'test',
				entityUnique: 'some-unique',
			});

			expect(result.length).to.be.equal(2);
			expect(result[0]?.value).to.be.equal('value for culture cultureA');
			expect(result[0]?.culture).to.be.equal('cultureA');
			expect(result[1]?.value).to.be.equal('blueprint value for cultureB');
			expect(result[1]?.culture).to.be.equal('cultureB');
		});

		it('creates culture variant values when no cultures available should fail', async () => {
			const ctrlHost = new UmbTestControllerHostElement();
			const ctrl = new UmbPropertyValuePresetVariantBuilderController(ctrlHost);

			const propertyTypes: Array<UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel> = [
				{
					alias: 'test',
					propertyEditorUiAlias: 'test-editor-ui',
					config: [],
					typeArgs: { variesByCulture: true },
				},
			];

			try {
				await ctrl.create(propertyTypes, {
					entityType: 'test',
					entityUnique: 'some-unique',
				});
				expect.fail('Expected to fail');
			} catch (e) {}
		});

		it('creates segmented values', async () => {
			const ctrlHost = new UmbTestControllerHostElement();
			const ctrl = new UmbPropertyValuePresetVariantBuilderController(ctrlHost);
			ctrl.setSegments(['segmentA', 'segmentB']);

			const propertyTypes: Array<UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel> = [
				{
					alias: 'test',
					propertyEditorUiAlias: 'test-editor-ui',
					config: [],
					typeArgs: { variesBySegment: true },
				},
			];

			const result = await ctrl.create(propertyTypes, {
				entityType: 'test',
				entityUnique: 'some-unique',
			});

			expect(result.length).to.be.equal(3);
			expect(result[0]?.value).to.be.equal('value for culture invariant');
			expect(result[0]?.culture).to.be.null;
			expect(result[0]?.segment).to.be.null;
			expect(result[1]?.value).to.be.equal('value for culture invariant_segmentA');
			expect(result[1]?.culture).to.be.null;
			expect(result[1]?.segment).to.be.equal('segmentA');
			expect(result[2]?.value).to.be.equal('value for culture invariant_segmentB');
			expect(result[2]?.segment).to.be.equal('segmentB');
			expect(result[2]?.culture).to.be.null;
		});

		it('creates segmented values when no segments available', async () => {
			const ctrlHost = new UmbTestControllerHostElement();
			const ctrl = new UmbPropertyValuePresetVariantBuilderController(ctrlHost);

			const propertyTypes: Array<UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel> = [
				{
					alias: 'test',
					propertyEditorUiAlias: 'test-editor-ui',
					config: [],
					typeArgs: { variesBySegment: true },
				},
			];

			const result = await ctrl.create(propertyTypes, {
				entityType: 'test',
				entityUnique: 'some-unique',
			});

			expect(result.length).to.be.equal(1);
			expect(result[0]?.value).to.be.equal('value for culture invariant');
			expect(result[0]?.culture).to.be.null;
			expect(result[0]?.segment).to.be.null;
		});

		it('creates culture variant and segmented values', async () => {
			const ctrlHost = new UmbTestControllerHostElement();
			const ctrl = new UmbPropertyValuePresetVariantBuilderController(ctrlHost);
			ctrl.setCultures(['cultureA', 'cultureB']);
			ctrl.setSegments(['segmentA', 'segmentB']);

			const propertyTypes: Array<UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel> = [
				{
					alias: 'test',
					propertyEditorUiAlias: 'test-editor-ui',
					config: [],
					typeArgs: { variesByCulture: true, variesBySegment: true },
				},
			];

			const result = await ctrl.create(propertyTypes, {
				entityType: 'test',
				entityUnique: 'some-unique',
			});

			expect(result.length).to.be.equal(6);

			expect(result[0]?.value).to.be.equal('value for culture cultureA');
			expect(result[0]?.culture).to.be.equal('cultureA');
			expect(result[0]?.segment).to.be.null;
			expect(result[1]?.value).to.be.equal('value for culture cultureA_segmentA');
			expect(result[1]?.culture).to.be.equal('cultureA');
			expect(result[1]?.segment).to.be.equal('segmentA');
			expect(result[2]?.value).to.be.equal('value for culture cultureA_segmentB');
			expect(result[2]?.culture).to.be.equal('cultureA');
			expect(result[2]?.segment).to.be.equal('segmentB');

			expect(result[3]?.value).to.be.equal('value for culture cultureB');
			expect(result[3]?.culture).to.be.equal('cultureB');
			expect(result[3]?.segment).to.be.null;
			expect(result[4]?.value).to.be.equal('value for culture cultureB_segmentA');
			expect(result[4]?.culture).to.be.equal('cultureB');
			expect(result[4]?.segment).to.be.equal('segmentA');
			expect(result[5]?.value).to.be.equal('value for culture cultureB_segmentB');
			expect(result[5]?.culture).to.be.equal('cultureB');
			expect(result[5]?.segment).to.be.equal('segmentB');
		});

		it('migrates invariant value to variant when variation changes', async () => {
			const ctrlHost = new UmbTestControllerHostElement();
			const ctrl = new UmbPropertyValuePresetVariantBuilderController(ctrlHost);
			ctrl.setCultures(['en-US', 'da-DK']);

			// Set existing values as invariant (culture: null) - simulating before variation change
			ctrl.setValues([
				{
					alias: 'test',
					value: 'invariant value',
					culture: null,
					segment: null,
					editorAlias: 'test-editor-schema',
				},
			]);

			// Property now varies by culture (after variation change)
			const propertyTypes: Array<UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel> = [
				{
					alias: 'test',
					propertyEditorUiAlias: 'test-editor-ui',
					propertyEditorSchemaAlias: 'test-editor-schema',
					config: [],
					typeArgs: { variesByCulture: true },
				},
			];

			const result = await ctrl.create(propertyTypes, {
				entityType: 'test',
				entityUnique: 'some-unique',
			});

			// Both culture variants should inherit the invariant value
			// Note: When a value exists, the preset API preserves it without modification
			expect(result.length).to.be.equal(2);
			expect(result[0]?.value).to.be.equal('invariant value');
			expect(result[0]?.culture).to.be.equal('en-US');
			expect(result[1]?.value).to.be.equal('invariant value');
			expect(result[1]?.culture).to.be.equal('da-DK');
		});

		it('migrates variant value to invariant when variation changes', async () => {
			const ctrlHost = new UmbTestControllerHostElement();
			const ctrl = new UmbPropertyValuePresetVariantBuilderController(ctrlHost);

			// Set existing values as variant (culture-specific) - simulating before variation change
			ctrl.setValues([
				{
					alias: 'test',
					value: 'en-US value',
					culture: 'en-US',
					segment: null,
					editorAlias: 'test-editor-schema',
				},
				{
					alias: 'test',
					value: 'da-DK value',
					culture: 'da-DK',
					segment: null,
					editorAlias: 'test-editor-schema',
				},
			]);

			// Property is now invariant (after variation change)
			const propertyTypes: Array<UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel> = [
				{
					alias: 'test',
					propertyEditorUiAlias: 'test-editor-ui',
					propertyEditorSchemaAlias: 'test-editor-schema',
					config: [],
					typeArgs: { variesByCulture: false },
				},
			];

			const result = await ctrl.create(propertyTypes, {
				entityType: 'test',
				entityUnique: 'some-unique',
			});

			// Invariant value should use first culture-specific value found
			// Note: When a value exists, the preset API preserves it without modification
			expect(result.length).to.be.equal(1);
			expect(result[0]?.value).to.be.equal('en-US value');
			expect(result[0]?.culture).to.be.null;
		});
	});
});
