import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type {
	ManifestPropertyValueResolver,
	UmbPropertyValueData,
	UmbPropertyValueResolver,
} from '@umbraco-cms/backoffice/property';
import { UmbVariantId, type UmbVariantDataModel } from '@umbraco-cms/backoffice/variant';
import { UmbMergeContentVariantDataController } from './merge-content-variant-data.controller.js';
import type { UmbContentLikeDetailModel, UmbEntryValueModel, UmbPotentialContentValueModel } from '../types.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('umb-test-controller-host')
export class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

type TestPropertyValueNestedType = {
	nestedValue: UmbPotentialContentValueModel;
};

export class TestPropertyValueResolver implements UmbPropertyValueResolver<
	UmbPropertyValueData<TestPropertyValueNestedType>,
	UmbPotentialContentValueModel,
	UmbVariantDataModel
> {
	async processValues(
		property: UmbPropertyValueData<TestPropertyValueNestedType>,
		valuesCallback: (
			values: Array<UmbPotentialContentValueModel>,
		) => Promise<Array<UmbPotentialContentValueModel> | undefined>,
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
	contentData: Array<{ key: string; values: Array<UmbPotentialContentValueModel> }>;
};

/**
 * Mirrors UmbBlockValueResolver._processValueBlockData: the values callback is invoked
 * once per contentData entry, in array order.
 */
export class TestBlockValueResolver implements UmbPropertyValueResolver<
	UmbPropertyValueData<TestBlockValueType>,
	UmbPotentialContentValueModel,
	UmbVariantDataModel
> {
	async processValues(
		property: UmbPropertyValueData<TestBlockValueType>,
		valuesCallback: (
			values: Array<UmbPotentialContentValueModel>,
			groupIdentifier?: string,
		) => Promise<Array<UmbPotentialContentValueModel> | undefined>,
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

const innerValue = (culture: string | null, value: string): UmbPotentialContentValueModel => ({
	editorAlias: 'some-editor',
	alias: 'text',
	culture,
	segment: null,
	value,
});

/** A block holding one `text` value per culture, keyed by culture. */
const block = (key: string, valuesByCulture: Record<string, string>) => ({
	key,
	values: Object.entries(valuesByCulture).map(([culture, value]) => innerValue(culture, value)),
});

/** Registers a property value resolver for the duration of each test in the current describe. */
const useResolver = (alias: string, api: ManifestPropertyValueResolver['api'], forEditorAlias: string) => {
	beforeEach(async () => {
		umbExtensionsRegistry.register({
			type: 'propertyValueResolver',
			name: alias,
			alias,
			api,
			forEditorAlias,
		} as ManifestPropertyValueResolver);
	});

	afterEach(async () => {
		umbExtensionsRegistry.unregister(alias);
	});
};

describe('UmbMergeContentVariantDataController', () => {
	describe('Block-shaped resolver', () => {
		useResolver('Umb.Test.Resolver.Block', TestBlockValueResolver, 'test-block-editor');

		it('pairs inner values by block key, not by array position', async () => {
			const ctrlHost = new UmbTestControllerHostElement();
			const ctrl = new UmbMergeContentVariantDataController(ctrlHost);

			// Persisted: three blocks, each with a Danish and a German value.
			const persistedData: UmbContentLikeDetailModel = {
				values: [
					blockValue([
						block('block-a', { da: 'a-da', de: 'a-de' }),
						block('block-b', { da: 'b-da', de: 'b-de' }),
						block('block-c', { da: 'c-da', de: 'c-de' }),
					]),
				],
			};

			// Draft: the first block has been deleted, so every remaining block now sits one
			// position earlier than it does in the persisted data.
			const runtimeData: UmbContentLikeDetailModel = {
				values: [
					blockValue([
						block('block-b', { da: 'b-da', de: 'b-de-edited' }),
						block('block-c', { da: 'c-da', de: 'c-de-edited' }),
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

		it('sorts inner block values by culture, invariant first', async () => {
			const ctrlHost = new UmbTestControllerHostElement();
			const ctrl = new UmbMergeContentVariantDataController(ctrlHost);

			const data: UmbContentLikeDetailModel = {
				values: [
					blockValue([
						{
							key: 'block-a',
							values: [innerValue('en-us', 'a-en'), innerValue(null, 'a-invariant'), innerValue('da-dk', 'a-da')],
						},
					]),
				],
			};

			const variants = [new UmbVariantId('en-us'), new UmbVariantId('da-dk')];
			const result = await ctrl.process(data, data, variants, [...variants, UmbVariantId.CreateInvariant()]);

			const blocks = (result.values[0].value as TestBlockValueType).contentData;
			expect(blocks[0].values.map((v) => v.culture)).to.deep.equal([null, 'da-dk', 'en-us']);
		});
	});

	describe('Simple resolver', () => {
		useResolver('Umb.Test.Resolver.1', TestPropertyValueResolver, 'test-editor');

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
			const ctrlHost = new UmbTestControllerHostElement();
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

	describe('Value ordering', () => {
		const value = (alias: string, culture: string | null): UmbEntryValueModel => ({
			editorAlias: 'some-editor',
			alias,
			culture,
			segment: null,
			value: `${alias}-${culture}`,
		});

		it('sorts values by culture, invariant first', async () => {
			const ctrlHost = new UmbTestControllerHostElement();
			const ctrl = new UmbMergeContentVariantDataController(ctrlHost);

			const data: UmbContentLikeDetailModel = {
				values: [value('title', 'en-us'), value('title', null), value('title', 'da-dk')],
			};

			const variants = [new UmbVariantId('en-us'), new UmbVariantId('da-dk')];
			const result = await ctrl.process(data, data, variants, [...variants, UmbVariantId.CreateInvariant()]);

			expect(result.values.map((v) => v.culture)).to.deep.equal([null, 'da-dk', 'en-us']);
		});

		it('places a draft-only new culture by culture, not at the end', async () => {
			const ctrlHost = new UmbTestControllerHostElement();
			const ctrl = new UmbMergeContentVariantDataController(ctrlHost);

			const persistedData: UmbContentLikeDetailModel = {
				values: [value('title', null), value('title', 'en-us')],
			};
			const runtimeData: UmbContentLikeDetailModel = {
				values: [value('title', null), value('title', 'en-us'), value('title', 'da-dk')],
			};

			const variants = [new UmbVariantId('en-us'), new UmbVariantId('da-dk')];
			const result = await ctrl.process(persistedData, runtimeData, variants, [
				...variants,
				UmbVariantId.CreateInvariant(),
			]);

			expect(result.values.map((v) => v.culture)).to.deep.equal([null, 'da-dk', 'en-us']);
		});

		it('keeps values sharing a culture in their existing relative order', async () => {
			const ctrlHost = new UmbTestControllerHostElement();
			const ctrl = new UmbMergeContentVariantDataController(ctrlHost);

			const data: UmbContentLikeDetailModel = {
				values: [value('subtitle', 'da-dk'), value('title', 'da-dk'), value('body', 'en-us')],
			};

			const variants = [new UmbVariantId('da-dk'), new UmbVariantId('en-us')];
			const result = await ctrl.process(data, data, variants, variants);

			expect(result.values.map((v) => v.alias)).to.deep.equal(['subtitle', 'title', 'body']);
		});
	});
});
