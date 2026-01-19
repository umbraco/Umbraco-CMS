import { expect } from '@open-wc/testing';
import { UmbContentDetailWorkspaceTypeTransformController } from './content-detail-workspace-type-transform.controller.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import type { UmbContentDetailModel } from '../types.js';
import type { UmbEntityVariantModel } from '@umbraco-cms/backoffice/variant';
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';

@customElement('umb-test-workspace-host')
class UmbTestWorkspaceHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	#propertyTypesState = new UmbObjectState<Array<UmbPropertyTypeModel> | undefined>(undefined);

	structure = {
		contentTypeProperties: this.#propertyTypesState.asObservable(),
	};

	#data: UmbContentDetailModel<UmbEntityVariantModel> | undefined;
	#languages: Array<UmbLanguageDetailModel> = [
		{ unique: 'en-US', name: 'English (US)', isDefault: true } as UmbLanguageDetailModel,
		{ unique: 'da-DK', name: 'Danish', isDefault: false } as UmbLanguageDetailModel,
	];

	getData(): UmbContentDetailModel<UmbEntityVariantModel> | undefined {
		return this.#data;
	}

	setData(data: UmbContentDetailModel<UmbEntityVariantModel>): void {
		this.#data = data;
	}

	getLanguages(): Array<UmbLanguageDetailModel> {
		return this.#languages;
	}

	setPropertyTypes(propertyTypes: Array<UmbPropertyTypeModel>): void {
		this.#propertyTypesState.setValue(propertyTypes);
	}
}

describe('UmbContentDetailWorkspaceTypeTransformController', () => {
	let mockWorkspace: UmbTestWorkspaceHostElement;

	beforeEach(() => {
		mockWorkspace = new UmbTestWorkspaceHostElement();
	});

	it('migrates invariant value to variant when property variation changes', async () => {
		// Set up initial data with invariant value
		mockWorkspace.setData({
			unique: 'test-doc',
			values: [
				{
					alias: 'testProperty',
					value: 'invariant value',
					culture: null,
					segment: null,
					editorAlias: 'test-editor',
				},
			],
		} as UmbContentDetailModel<UmbEntityVariantModel>);

		// Set initial property types (invariant)
		const oldPropertyTypes: Array<UmbPropertyTypeModel> = [
			{
				alias: 'testProperty',
				variesByCulture: false,
				variesBySegment: false,
			} as UmbPropertyTypeModel,
		];
		mockWorkspace.setPropertyTypes(oldPropertyTypes);

		// Create controller - it will observe property type changes
		new UmbContentDetailWorkspaceTypeTransformController(mockWorkspace as any);

		// Wait for initial observation
		await new Promise((resolve) => setTimeout(resolve, 0));

		// Change property to vary by culture
		const newPropertyTypes: Array<UmbPropertyTypeModel> = [
			{
				alias: 'testProperty',
				variesByCulture: true,
				variesBySegment: false,
			} as UmbPropertyTypeModel,
		];
		mockWorkspace.setPropertyTypes(newPropertyTypes);

		// Wait for observation to trigger
		await new Promise((resolve) => setTimeout(resolve, 0));

		// Check that value was migrated to default culture
		const data = mockWorkspace.getData();
		expect(data?.values.length).to.equal(1);
		expect(data?.values[0]?.alias).to.equal('testProperty');
		expect(data?.values[0]?.value).to.equal('invariant value');
		expect(data?.values[0]?.culture).to.equal('en-US'); // Default language
		expect(data?.values[0]?.segment).to.be.null;
	});

	it('migrates variant value to invariant when property variation changes', async () => {
		// Set up initial data with variant values
		mockWorkspace.setData({
			unique: 'test-doc',
			values: [
				{
					alias: 'testProperty',
					value: 'en-US value',
					culture: 'en-US',
					segment: null,
					editorAlias: 'test-editor',
				},
				{
					alias: 'testProperty',
					value: 'da-DK value',
					culture: 'da-DK',
					segment: null,
					editorAlias: 'test-editor',
				},
			],
		} as UmbContentDetailModel<UmbEntityVariantModel>);

		// Set initial property types (variant)
		const oldPropertyTypes: Array<UmbPropertyTypeModel> = [
			{
				alias: 'testProperty',
				variesByCulture: true,
				variesBySegment: false,
			} as UmbPropertyTypeModel,
		];
		mockWorkspace.setPropertyTypes(oldPropertyTypes);

		// Create controller
		new UmbContentDetailWorkspaceTypeTransformController(mockWorkspace as any);

		// Wait for initial observation
		await new Promise((resolve) => setTimeout(resolve, 0));

		// Change property to be invariant
		const newPropertyTypes: Array<UmbPropertyTypeModel> = [
			{
				alias: 'testProperty',
				variesByCulture: false,
				variesBySegment: false,
			} as UmbPropertyTypeModel,
		];
		mockWorkspace.setPropertyTypes(newPropertyTypes);

		// Wait for observation to trigger
		await new Promise((resolve) => setTimeout(resolve, 0));

		// Check that both values were migrated to invariant
		const data = mockWorkspace.getData();
		expect(data?.values.length).to.equal(2);
		expect(data?.values[0]?.alias).to.equal('testProperty');
		expect(data?.values[0]?.value).to.equal('en-US value');
		expect(data?.values[0]?.culture).to.be.null;
		expect(data?.values[1]?.alias).to.equal('testProperty');
		expect(data?.values[1]?.value).to.equal('da-DK value');
		expect(data?.values[1]?.culture).to.be.null;
	});

	it('preserves values during variation change', async () => {
		// Set up initial data with invariant value
		const originalValue = 'important data';
		mockWorkspace.setData({
			unique: 'test-doc',
			values: [
				{
					alias: 'testProperty',
					value: originalValue,
					culture: null,
					segment: null,
					editorAlias: 'test-editor',
				},
			],
		} as UmbContentDetailModel<UmbEntityVariantModel>);

		// Set initial property types (invariant)
		const oldPropertyTypes: Array<UmbPropertyTypeModel> = [
			{
				alias: 'testProperty',
				variesByCulture: false,
				variesBySegment: false,
			} as UmbPropertyTypeModel,
		];
		mockWorkspace.setPropertyTypes(oldPropertyTypes);

		// Create controller
		new UmbContentDetailWorkspaceTypeTransformController(mockWorkspace as any);

		// Wait for initial observation
		await new Promise((resolve) => setTimeout(resolve, 0));

		// Change property to vary by culture
		const newPropertyTypes: Array<UmbPropertyTypeModel> = [
			{
				alias: 'testProperty',
				variesByCulture: true,
				variesBySegment: false,
			} as UmbPropertyTypeModel,
		];
		mockWorkspace.setPropertyTypes(newPropertyTypes);

		// Wait for observation to trigger
		await new Promise((resolve) => setTimeout(resolve, 0));

		// Verify the value was preserved
		const data = mockWorkspace.getData();
		expect(data?.values[0]?.value).to.equal(originalValue);
	});

	it('does not modify data when variation does not change', async () => {
		// Set up initial data
		mockWorkspace.setData({
			unique: 'test-doc',
			values: [
				{
					alias: 'testProperty',
					value: 'test value',
					culture: null,
					segment: null,
					editorAlias: 'test-editor',
				},
			],
		} as UmbContentDetailModel<UmbEntityVariantModel>);

		// Set initial property types (invariant)
		const oldPropertyTypes: Array<UmbPropertyTypeModel> = [
			{
				alias: 'testProperty',
				variesByCulture: false,
				variesBySegment: false,
			} as UmbPropertyTypeModel,
		];
		mockWorkspace.setPropertyTypes(oldPropertyTypes);

		// Create controller
		new UmbContentDetailWorkspaceTypeTransformController(mockWorkspace as any);

		// Wait for initial observation
		await new Promise((resolve) => setTimeout(resolve, 0));

		// Get reference to original data
		const originalData = mockWorkspace.getData();

		// Update property types but keep variation the same
		const newPropertyTypes: Array<UmbPropertyTypeModel> = [
			{
				alias: 'testProperty',
				variesByCulture: false, // Still invariant
				variesBySegment: false,
			} as UmbPropertyTypeModel,
		];
		mockWorkspace.setPropertyTypes(newPropertyTypes);

		// Wait for observation to trigger
		await new Promise((resolve) => setTimeout(resolve, 0));

		// Data should be the same object (not modified)
		const currentData = mockWorkspace.getData();
		expect(currentData).to.equal(originalData);
	});

	it('handles multiple properties with different variations', async () => {
		// Set up initial data with two properties
		mockWorkspace.setData({
			unique: 'test-doc',
			values: [
				{
					alias: 'property1',
					value: 'invariant value',
					culture: null,
					segment: null,
					editorAlias: 'test-editor',
				},
				{
					alias: 'property2',
					value: 'variant value',
					culture: 'en-US',
					segment: null,
					editorAlias: 'test-editor',
				},
			],
		} as UmbContentDetailModel<UmbEntityVariantModel>);

		// Set initial property types
		const oldPropertyTypes: Array<UmbPropertyTypeModel> = [
			{
				alias: 'property1',
				variesByCulture: false,
				variesBySegment: false,
			} as UmbPropertyTypeModel,
			{
				alias: 'property2',
				variesByCulture: true,
				variesBySegment: false,
			} as UmbPropertyTypeModel,
		];
		mockWorkspace.setPropertyTypes(oldPropertyTypes);

		// Create controller
		new UmbContentDetailWorkspaceTypeTransformController(mockWorkspace as any);

		// Wait for initial observation
		await new Promise((resolve) => setTimeout(resolve, 0));

		// Change variations: property1 becomes variant, property2 becomes invariant
		const newPropertyTypes: Array<UmbPropertyTypeModel> = [
			{
				alias: 'property1',
				variesByCulture: true, // Now variant
				variesBySegment: false,
			} as UmbPropertyTypeModel,
			{
				alias: 'property2',
				variesByCulture: false, // Now invariant
				variesBySegment: false,
			} as UmbPropertyTypeModel,
		];
		mockWorkspace.setPropertyTypes(newPropertyTypes);

		// Wait for observation to trigger
		await new Promise((resolve) => setTimeout(resolve, 0));

		// Check that both properties were migrated correctly
		const data = mockWorkspace.getData();
		expect(data?.values.length).to.equal(2);

		// Property1: invariant -> variant (should get default culture)
		const prop1 = data?.values.find((v) => v.alias === 'property1');
		expect(prop1?.value).to.equal('invariant value');
		expect(prop1?.culture).to.equal('en-US');

		// Property2: variant -> invariant (should become null culture)
		const prop2 = data?.values.find((v) => v.alias === 'property2');
		expect(prop2?.value).to.equal('variant value');
		expect(prop2?.culture).to.be.null;
	});
});
