import { expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type {
	ManifestPropertyValueResolver,
	ManifestPropertyValueCloner,
	UmbPropertyValueData,
	UmbPropertyValueResolver,
	UmbPropertyValueCloner,
} from '../types.js';
import type { UmbVariantDataModel } from '@umbraco-cms/backoffice/variant';
import { UmbPropertyValueCloneController } from './property-value-clone.controller.js';
import { UmbControllerHostElementElement } from '@umbraco-cms/backoffice/controller-api';

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

export class TestPropertyValueCloner implements UmbPropertyValueCloner<TestPropertyValueWithId> {
	async cloneValue(value: TestPropertyValueWithId): Promise<TestPropertyValueWithId> {
		return { ...value, id: 'updated-id' };
	}

	destroy(): void {}
}

describe('UmbPropertyValueCloneController', () => {
	describe('Cloner', () => {
		beforeEach(async () => {
			const manifestCloner: ManifestPropertyValueCloner = {
				type: 'propertyValueCloner',
				name: 'test-cloner-1',
				alias: 'Umb.Test.Cloner.1',
				api: TestPropertyValueCloner,
				forEditorAlias: 'test-editor',
			};

			umbExtensionsRegistry.register(manifestCloner);
		});
		afterEach(async () => {
			umbExtensionsRegistry.unregister('Umb.Test.Cloner.1');
		});

		it('clones value', async () => {
			const ctrlHost = new UmbControllerHostElementElement();
			const ctrl = new UmbPropertyValueCloneController(ctrlHost);

			const value = {
				editorAlias: 'test-editor',
				alias: 'test',
				culture: null,
				segment: null,
				value: {
					id: 'not-updated-id',
				},
			};

			const result = await ctrl.clone(value);

			expect((result.value as TestPropertyValueNestedType | undefined)?.id).to.be.equal('updated-id');
		});
	});

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

			const manifestResolverOnly: ManifestPropertyValueResolver = {
				type: 'propertyValueResolver',
				name: 'test-resolver-1',
				alias: 'Umb.Test.Resolver.2',
				api: TestPropertyValueResolver,
				forEditorAlias: 'only-resolver-editor',
			};

			umbExtensionsRegistry.register(manifestResolverOnly);

			const manifestCloner: ManifestPropertyValueCloner = {
				type: 'propertyValueCloner',
				name: 'test-cloner-1',
				alias: 'Umb.Test.Cloner.1',
				api: TestPropertyValueCloner,
				forEditorAlias: 'test-editor',
			};

			umbExtensionsRegistry.register(manifestCloner);
		});
		afterEach(async () => {
			umbExtensionsRegistry.unregister('Umb.Test.Resolver.1');
			umbExtensionsRegistry.unregister('Umb.Test.Resolver.2');
			umbExtensionsRegistry.unregister('Umb.Test.Cloner.1');
		});

		it('clones value and inner values', async () => {
			const ctrlHost = new UmbControllerHostElementElement();
			const ctrl = new UmbPropertyValueCloneController(ctrlHost);

			const value = {
				editorAlias: 'test-editor',
				alias: 'test',
				culture: null,
				segment: null,
				value: {
					id: 'not-updated-id',
				},
			};

			const result = await ctrl.clone(value);

			expect((result.value as TestPropertyValueNestedType | undefined)?.id).to.be.equal('updated-id');
		});

		it('clones only inner values', async () => {
			const ctrlHost = new UmbControllerHostElementElement();
			const ctrl = new UmbPropertyValueCloneController(ctrlHost);

			const value = {
				editorAlias: 'only-resolver-editor',
				alias: 'not-to-be-handled',
				culture: null,
				segment: null,
				value: {
					id: 'not-updated-id',
					nestedValue: {
						editorAlias: 'test-editor',
						alias: 'some',
						culture: null,
						segment: null,
						value: {
							id: 'inner-not-updated-id',
						},
					},
				},
			};

			const result = await ctrl.clone(value);

			expect((result.value as TestPropertyValueNestedType | undefined)?.id).to.be.equal('not-updated-id');
			expect((result.value as TestPropertyValueNestedType | undefined)?.nestedValue?.value?.id).to.be.equal(
				'updated-id',
			);
		});

		it('clones value and inner values', async () => {
			const ctrlHost = new UmbControllerHostElementElement();
			const ctrl = new UmbPropertyValueCloneController(ctrlHost);

			const value = {
				editorAlias: 'test-editor',
				alias: 'test',
				culture: null,
				segment: null,
				value: {
					id: 'not-updated-id',
					nestedValue: {
						editorAlias: 'test-editor',
						alias: 'some',
						culture: null,
						segment: null,
						value: {
							id: 'inner-not-updated-id',
						},
					},
				},
			};

			const result = await ctrl.clone(value);

			expect((result.value as TestPropertyValueNestedType | undefined)?.id).to.be.equal('updated-id');
			expect((result.value as TestPropertyValueNestedType | undefined)?.nestedValue?.value?.id).to.be.equal(
				'updated-id',
			);
		});

		it('clones value and inner values for two levels', async () => {
			const ctrlHost = new UmbControllerHostElementElement();
			const ctrl = new UmbPropertyValueCloneController(ctrlHost);

			const value = {
				editorAlias: 'test-editor',
				alias: 'test',
				culture: null,
				segment: null,
				value: {
					id: 'not-updated-id',
					nestedValue: {
						editorAlias: 'test-editor',
						alias: 'some',
						culture: null,
						segment: null,
						value: {
							id: 'inner-not-updated-id',
							nestedValue: {
								editorAlias: 'test-editor',
								alias: 'another',
								culture: null,
								segment: null,
								value: {
									id: 'inner-inner-not-updated-id',
								},
							},
						},
					},
				},
			};

			const result = await ctrl.clone(value);

			expect((result.value as TestPropertyValueNestedType | undefined)?.id).to.be.equal('updated-id');
			expect((result.value as TestPropertyValueNestedType | undefined)?.nestedValue?.value?.id).to.be.equal(
				'updated-id',
			);
			expect(
				(
					(result.value as TestPropertyValueNestedType | undefined)?.nestedValue?.value as
						| TestPropertyValueNestedType
						| undefined
				)?.nestedValue?.value?.id,
			).to.be.equal('updated-id');
		});
	});
});
