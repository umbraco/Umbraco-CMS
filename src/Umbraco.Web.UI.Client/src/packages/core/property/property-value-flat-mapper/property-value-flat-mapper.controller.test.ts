import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestPropertyValueResolver, UmbPropertyValueData, UmbPropertyValueResolver } from '../types.js';
import type { UmbVariantDataModel } from '@umbraco-cms/backoffice/variant';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbPropertyValueFlatMapperController } from './property-value-flat-mapper.controller';

@customElement('umb-test-controller-host')
export class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

type TestPropertyValueWithId = {
	id: string;
};

type TestPropertyValueNestedType = TestPropertyValueWithId & {
	nestedValue?: UmbPropertyValueData<TestPropertyValueWithId>;
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
	): Promise<UmbPropertyValueData<TestPropertyValueNestedType>> {
		if (property.value) {
			const nestedValue = property.value.nestedValue;
			const processedValues = nestedValue ? await valuesCallback([nestedValue]) : undefined;
			return {
				...property,
				value: {
					...property.value,
					nestedValue: processedValues ? processedValues[0] : undefined,
				},
			} as UmbPropertyValueData<TestPropertyValueNestedType>;
		}
		return property;
	}

	async processVariants(
		property: UmbPropertyValueData<TestPropertyValueNestedType>,
		variantsCallback: (values: Array<UmbVariantDataModel>) => Promise<Array<UmbVariantDataModel> | undefined>,
	) {
		return property;
	}

	destroy(): void {}
}

describe('UmbPropertyValueFlatMapperController', () => {
	describe('Resolvers and Cloner', () => {
		beforeEach(async () => {
			const manifestResolver: ManifestPropertyValueResolver = {
				type: 'propertyValueResolver',
				name: 'test-resolver-1',
				alias: 'Umb.Test.Resolver.1',
				api: TestPropertyValueResolver,
				forEditorAlias: 'test-editor',
			};

			umbExtensionsRegistry.register(manifestResolver);
		});
		afterEach(async () => {
			umbExtensionsRegistry.unregister('Umb.Test.Resolver.1');
		});

		it('mapper result is returned as an array', async () => {
			const ctrlHost = new UmbTestControllerHostElement();
			const ctrl = new UmbPropertyValueFlatMapperController(ctrlHost);

			const property = {
				editorAlias: 'test-editor-with-no-mapper',
				alias: 'test',
				culture: null,
				segment: null,
				value: {
					id: 'value',
				},
			};

			const result = await ctrl.flatMap(property, (property) => {
				return (property.value as any).id;
			});

			expect(result.length).to.be.equal(1);
			expect(result[0]).to.be.equal('value');
		});

		it('maps first level inner values', async () => {
			const ctrlHost = new UmbTestControllerHostElement();
			const ctrl = new UmbPropertyValueFlatMapperController(ctrlHost);

			const property = {
				editorAlias: 'test-editor',
				alias: 'not-to-be-handled',
				culture: null,
				segment: null,
				value: {
					id: 'value',
					nestedValue: {
						editorAlias: 'test-editor',
						alias: 'some',
						culture: null,
						segment: null,
						value: {
							id: 'inner-value',
						},
					},
				},
			};

			const result = await ctrl.flatMap(property, (property) => {
				return (property.value as any).id;
			});

			expect(result.length).to.be.equal(2);
			expect(result[0]).to.be.equal('value');
			expect(result[1]).to.be.equal('inner-value');
		});

		it('maps values for two levels', async () => {
			const ctrlHost = new UmbTestControllerHostElement();
			const ctrl = new UmbPropertyValueFlatMapperController(ctrlHost);

			const property = {
				editorAlias: 'test-editor',
				alias: 'test',
				culture: null,
				segment: null,
				value: {
					id: 'value',
					nestedValue: {
						editorAlias: 'test-editor',
						alias: 'some',
						culture: null,
						segment: null,
						value: {
							id: 'inner-value',
							nestedValue: {
								editorAlias: 'test-editor-no-mapper',
								alias: 'another',
								culture: null,
								segment: null,
								value: {
									id: 'inner-inner-value',
								},
							},
						},
					},
				},
			};

			const result = await ctrl.flatMap(property, (property) => {
				return (property.value as any).id;
			});

			expect(result.length).to.be.equal(3);
			expect(result[0]).to.be.equal('value');
			expect(result[1]).to.be.equal('inner-value');
			expect(result[2]).to.be.equal('inner-inner-value');
		});
	});
});
