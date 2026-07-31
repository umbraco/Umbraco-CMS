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
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('umb-test-controller-host')
export class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

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

type TestBlockValueType = {
	contentData: Array<{ key: string; values: Array<UmbPropertyValueData> }>;
};

/**
 * Mirrors UmbBlockValueResolver._processValueBlockData: the values callback is invoked
 * once per contentData entry, in array order.
 */
export class TestBlockValueResolver
	implements
		UmbPropertyValueResolver<UmbPropertyValueData<TestBlockValueType>, UmbPropertyValueData, UmbVariantDataModel>
{
	async processValues(
		property: UmbPropertyValueData<TestBlockValueType>,
		valuesCallback: (
			values: Array<UmbPropertyValueData>,
			groupIdentifier?: string,
		) => Promise<Array<UmbPropertyValueData> | undefined>,
	) {
		if (property.value) {
			const contentData = await Promise.all(
				property.value.contentData.map(async (entry) => ({
					...entry,
					values: (await valuesCallback(entry.values, `contentData:${entry.key}`)) ?? [],
				})),
			);
			return { ...property, value: { ...property.value, contentData } };
		}
		return property;
	}

	destroy(): void {}
}

const blockValue = (contentData: TestBlockValueType['contentData']) => ({
	editorAlias: 'test-block-editor',
	alias: 'blocks',
	culture: null,
	segment: null,
	entityType: '',
	value: { contentData },
});

const innerValue = (culture: string | null, value: string): UmbPropertyValueData => ({
	editorAlias: 'some-editor',
	alias: 'text',
	culture,
	segment: null,
	value,
});

describe('UmbMergeContentVariantDataController', () => {
	describe('Block-shaped resolver', () => {
		beforeEach(async () => {
			umbExtensionsRegistry.register({
				type: 'propertyValueResolver',
				name: 'test-block-resolver',
				alias: 'Umb.Test.Resolver.Block',
				api: TestBlockValueResolver,
				forEditorAlias: 'test-block-editor',
			} as ManifestPropertyValueResolver);
		});

		afterEach(async () => {
			umbExtensionsRegistry.unregister('Umb.Test.Resolver.Block');
		});

		it('pairs inner values by block key, not by array position', async () => {
			const ctrlHost = new UmbTestControllerHostElement();
			const ctrl = new UmbMergeContentVariantDataController(ctrlHost);

			// Persisted: three blocks, each with a Danish and a German value.
			const persistedData: UmbContentLikeDetailModel = {
				values: [
					blockValue([
						{ key: 'block-a', values: [innerValue('da', 'a-da'), innerValue('de', 'a-de')] },
						{ key: 'block-b', values: [innerValue('da', 'b-da'), innerValue('de', 'b-de')] },
						{ key: 'block-c', values: [innerValue('da', 'c-da'), innerValue('de', 'c-de')] },
					]),
				],
			};

			// Draft: the first block has been deleted, so every remaining block now sits one
			// position earlier than it does in the persisted data.
			const runtimeData: UmbContentLikeDetailModel = {
				values: [
					blockValue([
						{ key: 'block-b', values: [innerValue('da', 'b-da'), innerValue('de', 'b-de-edited')] },
						{ key: 'block-c', values: [innerValue('da', 'c-da'), innerValue('de', 'c-de-edited')] },
					]),
				],
			};

			// Saving German only: Danish values must be carried over from the persisted data.
			const variants = [new UmbVariantId('de')];
			const result = await ctrl.process(persistedData, runtimeData, variants, [
				...variants,
				UmbVariantId.CreateInvariant(),
			]);

			const blocks = (result.values[0].value as TestBlockValueType).contentData;
			const textOf = (key: string, culture: string) =>
				blocks.find((b) => b.key === key)?.values.find((v) => v.culture === culture)?.value;

			// The German edits must land on their own blocks.
			expect(textOf('block-b', 'de'), 'block-b German').to.equal('b-de-edited');
			expect(textOf('block-c', 'de'), 'block-c German').to.equal('c-de-edited');

			// The Danish values must be carried over onto their own blocks, not shifted onto
			// the block that took the deleted one's position.
			expect(textOf('block-b', 'da'), 'block-b Danish').to.equal('b-da');
			expect(textOf('block-c', 'da'), 'block-c Danish').to.equal('c-da');
		});
	});

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
			const ctrlHost = new UmbTestControllerHostElement();
			const ctrl = new UmbMergeContentVariantDataController(ctrlHost);

			const persistedData: UmbContentLikeDetailModel = {
				values: [
					{
						editorAlias: 'test-editor',
						alias: 'test',
						culture: null,
						segment: null,
						entityType: '',
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
						entityType: '',
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
			const ctrlHost = new UmbTestControllerHostElement();
			const ctrl = new UmbMergeContentVariantDataController(ctrlHost);

			const persistedData: UmbContentLikeDetailModel = {
				values: [
					{
						editorAlias: 'test-editor',
						alias: 'test',
						culture: null,
						segment: null,
						entityType: '',
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
						entityType: '',
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
