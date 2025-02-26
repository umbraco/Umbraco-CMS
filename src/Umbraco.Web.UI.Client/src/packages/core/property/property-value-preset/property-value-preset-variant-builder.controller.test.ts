import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbControllerHostElementElement } from '@umbraco-cms/backoffice/controller-api';
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
			const ctrlHost = new UmbControllerHostElementElement();
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

			const result = await ctrl.create(propertyTypes);

			expect(result.length).to.be.equal(2);
			expect(result[0]?.value).to.be.equal('value for culture cultureA');
			expect(result[0]?.culture).to.be.equal('cultureA');
			expect(result[1]?.value).to.be.equal('value for culture cultureB');
			expect(result[1]?.culture).to.be.equal('cultureB');
		});

		it('creates culture variant values when no cultures available should fail', async () => {
			const ctrlHost = new UmbControllerHostElementElement();
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
				await ctrl.create(propertyTypes);
				expect.fail('Expected to fail');
			} catch (e) {}
		});

		it('creates segmented values', async () => {
			const ctrlHost = new UmbControllerHostElementElement();
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

			const result = await ctrl.create(propertyTypes);

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
			const ctrlHost = new UmbControllerHostElementElement();
			const ctrl = new UmbPropertyValuePresetVariantBuilderController(ctrlHost);

			const propertyTypes: Array<UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel> = [
				{
					alias: 'test',
					propertyEditorUiAlias: 'test-editor-ui',
					config: [],
					typeArgs: { variesBySegment: true },
				},
			];

			const result = await ctrl.create(propertyTypes);

			expect(result.length).to.be.equal(1);
			expect(result[0]?.value).to.be.equal('value for culture invariant');
			expect(result[0]?.culture).to.be.null;
			expect(result[0]?.segment).to.be.null;
		});

		it('creates culture variant and segmented values', async () => {
			const ctrlHost = new UmbControllerHostElementElement();
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

			const result = await ctrl.create(propertyTypes);

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
	});
});
