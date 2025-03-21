import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type {
	ManifestPropertyValueResolver,
	UmbPropertyValueData,
	UmbPropertyValueResolver,
} from '@umbraco-cms/backoffice/property';
import { UmbVariantId, type UmbVariantDataModel } from '@umbraco-cms/backoffice/variant';
import { UmbMergeContentVariantDataController } from './merge-content-variant-data.controller.js';
import type { UmbContentLikeDetailModel } from '../types.js';
import { UmbControllerHostElementElement } from '@umbraco-cms/backoffice/controller-api';

type TestPropertyValueNestedType = {
	nestedValue: UmbPropertyValueData;
};

export class TestPropertyValueResolver
	implements
		UmbPropertyValueResolver<
			UmbPropertyValueData<TestPropertyValueNestedType>,
			UmbPropertyValueData,
			UmbVariantDataModel
		>
{
	async processValues(
		property: UmbPropertyValueData<TestPropertyValueNestedType>,
		valuesCallback: (values: Array<UmbPropertyValueData>) => Promise<Array<UmbPropertyValueData> | undefined>,
	) {
		if (property.value) {
			const processedValues = await valuesCallback([property.value.nestedValue]);
			return {
				...property,
				value: {
					nestedValue: processedValues?.[0] ?? property.value.nestedValue,
				} as TestPropertyValueNestedType,
			} as UmbPropertyValueData<TestPropertyValueNestedType>;
		}
		return property;
	}

	destroy(): void {}
}

describe('UmbMergeContentVariantDataController', () => {
	describe('Simple resolver', () => {
		beforeEach(async () => {
			const manifest: ManifestPropertyValueResolver = {
				type: 'propertyValueResolver',
				name: 'test-resolver-1',
				alias: 'Umb.Test.Resolver.1',
				api: TestPropertyValueResolver,
				forEditorAlias: 'test-editor',
			};

			umbExtensionsRegistry.register(manifest);
		});

		afterEach(async () => {
			umbExtensionsRegistry.unregister('Umb.Test.Resolver.1');
		});

		it('transfers inner values of select variants', async () => {
			const ctrlHost = new UmbControllerHostElementElement();
			const ctrl = new UmbMergeContentVariantDataController(ctrlHost);

			const persistedData: UmbContentLikeDetailModel = {
				values: [
					{
						editorAlias: 'test-editor',
						alias: 'test',
						culture: null,
						segment: null,
						value: {
							nestedValue: {
								editorAlias: 'some-editor',
								alias: 'some',
								culture: null,
								segment: null,
								value: 'saved-nested-value-invariant',
							},
						},
					},
				],
			};

			const runtimeData: UmbContentLikeDetailModel = {
				values: [
					{
						editorAlias: 'test-editor',
						alias: 'test',
						culture: null,
						segment: null,
						value: {
							nestedValue: {
								editorAlias: 'some-editor',
								alias: 'some',
								value: 'updated-nested-value-invariant',
							},
						},
					},
				],
			};

			const result = await ctrl.process(persistedData, runtimeData, [], [UmbVariantId.CreateInvariant()]);

			expect((result.values[0].value as TestPropertyValueNestedType).nestedValue.value).to.be.equal(
				'updated-nested-value-invariant',
			);
		});

		it('does not transfers inner values of a not selected variant', async () => {
			const ctrlHost = new UmbControllerHostElementElement();
			const ctrl = new UmbMergeContentVariantDataController(ctrlHost);

			const persistedData: UmbContentLikeDetailModel = {
				values: [
					{
						editorAlias: 'test-editor',
						alias: 'test',
						culture: null,
						segment: null,
						value: {
							nestedValue: {
								editorAlias: 'some-editor',
								alias: 'some',
								culture: null,
								segment: null,
								value: 'saved-nested-value-invariant',
							},
						},
					},
				],
			};

			const runtimeData: UmbContentLikeDetailModel = {
				values: [
					{
						editorAlias: 'test-editor',
						alias: 'test',
						culture: null,
						segment: null,
						value: {
							nestedValue: {
								editorAlias: 'some-editor',
								alias: 'some',
								value: 'updated-nested-value-invariant',
							},
						},
					},
				],
			};

			const variants = [new UmbVariantId('da')];
			const result = await ctrl.process(persistedData, runtimeData, variants, variants);

			expect((result.values[0].value as TestPropertyValueNestedType).nestedValue.value).to.be.equal(
				'saved-nested-value-invariant',
			);
		});
	});
});
