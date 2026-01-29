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

	it('migrates variant value to invariant when property variation changes, keeping only default language value', async () => {
		// Set up initial data with variant values for multiple cultures
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

		// Check that only the default language value was kept and migrated to invariant
		const data = mockWorkspace.getData();
		expect(data?.values.length).to.equal(1);
		expect(data?.values[0]?.alias).to.equal('testProperty');
		expect(data?.values[0]?.value).to.equal('en-US value'); // Default language value preserved
		expect(data?.values[0]?.culture).to.be.null;
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

	it('keeps first value when default language value does not exist during variant to invariant change', async () => {
		// Set up initial data with only non-default language values
		mockWorkspace.setData({
			unique: 'test-doc',
			values: [
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

		// Should keep the only available value (da-DK) since default language value doesn't exist
		const data = mockWorkspace.getData();
		expect(data?.values.length).to.equal(1);
		expect(data?.values[0]?.alias).to.equal('testProperty');
		expect(data?.values[0]?.value).to.equal('da-DK value');
		expect(data?.values[0]?.culture).to.be.null;
	});

	it('correctly handles multiple cultures preferring default language during invariant migration', async () => {
		// Set up initial data with three culture values, default language in the middle
		mockWorkspace.setData({
			unique: 'test-doc',
			values: [
				{
					alias: 'testProperty',
					value: 'da-DK value',
					culture: 'da-DK',
					segment: null,
					editorAlias: 'test-editor',
				},
				{
					alias: 'testProperty',
					value: 'en-US value',
					culture: 'en-US',
					segment: null,
					editorAlias: 'test-editor',
				},
				{
					alias: 'testProperty',
					value: 'de-DE value',
					culture: 'de-DE',
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

		// Should keep only the default language (en-US) value
		const data = mockWorkspace.getData();
		expect(data?.values.length).to.equal(1);
		expect(data?.values[0]?.alias).to.equal('testProperty');
		expect(data?.values[0]?.value).to.equal('en-US value');
		expect(data?.values[0]?.culture).to.be.null;
	});

	it('does not affect other properties when consolidating values for invariant migration', async () => {
		// Set up initial data with multiple properties, some variant some invariant
		mockWorkspace.setData({
			unique: 'test-doc',
			values: [
				{
					alias: 'variantProperty',
					value: 'en-US variant',
					culture: 'en-US',
					segment: null,
					editorAlias: 'test-editor',
				},
				{
					alias: 'variantProperty',
					value: 'da-DK variant',
					culture: 'da-DK',
					segment: null,
					editorAlias: 'test-editor',
				},
				{
					alias: 'invariantProperty',
					value: 'invariant value',
					culture: null,
					segment: null,
					editorAlias: 'test-editor',
				},
				{
					alias: 'staysVariantProperty',
					value: 'en-US stays variant',
					culture: 'en-US',
					segment: null,
					editorAlias: 'test-editor',
				},
				{
					alias: 'staysVariantProperty',
					value: 'da-DK stays variant',
					culture: 'da-DK',
					segment: null,
					editorAlias: 'test-editor',
				},
			],
		} as UmbContentDetailModel<UmbEntityVariantModel>);

		// Set initial property types
		const oldPropertyTypes: Array<UmbPropertyTypeModel> = [
			{
				alias: 'variantProperty',
				variesByCulture: true,
				variesBySegment: false,
			} as UmbPropertyTypeModel,
			{
				alias: 'invariantProperty',
				variesByCulture: false,
				variesBySegment: false,
			} as UmbPropertyTypeModel,
			{
				alias: 'staysVariantProperty',
				variesByCulture: true,
				variesBySegment: false,
			} as UmbPropertyTypeModel,
		];
		mockWorkspace.setPropertyTypes(oldPropertyTypes);

		// Create controller
		new UmbContentDetailWorkspaceTypeTransformController(mockWorkspace as any);

		// Wait for initial observation
		await new Promise((resolve) => setTimeout(resolve, 0));

		// Only change variantProperty to invariant, others stay the same
		const newPropertyTypes: Array<UmbPropertyTypeModel> = [
			{
				alias: 'variantProperty',
				variesByCulture: false, // Changed to invariant
				variesBySegment: false,
			} as UmbPropertyTypeModel,
			{
				alias: 'invariantProperty',
				variesByCulture: false, // Stays invariant
				variesBySegment: false,
			} as UmbPropertyTypeModel,
			{
				alias: 'staysVariantProperty',
				variesByCulture: true, // Stays variant
				variesBySegment: false,
			} as UmbPropertyTypeModel,
		];
		mockWorkspace.setPropertyTypes(newPropertyTypes);

		// Wait for observation to trigger
		await new Promise((resolve) => setTimeout(resolve, 0));

		const data = mockWorkspace.getData();

		// variantProperty: should be consolidated to single invariant value
		const variantPropValues = data?.values.filter((v) => v.alias === 'variantProperty');
		expect(variantPropValues?.length).to.equal(1);
		expect(variantPropValues?.[0]?.value).to.equal('en-US variant');
		expect(variantPropValues?.[0]?.culture).to.be.null;

		// invariantProperty: should remain unchanged
		const invariantPropValues = data?.values.filter((v) => v.alias === 'invariantProperty');
		expect(invariantPropValues?.length).to.equal(1);
		expect(invariantPropValues?.[0]?.value).to.equal('invariant value');
		expect(invariantPropValues?.[0]?.culture).to.be.null;

		// staysVariantProperty: should remain unchanged with both culture values
		const staysVariantPropValues = data?.values.filter((v) => v.alias === 'staysVariantProperty');
		expect(staysVariantPropValues?.length).to.equal(2);
		expect(staysVariantPropValues?.find((v) => v.culture === 'en-US')?.value).to.equal('en-US stays variant');
		expect(staysVariantPropValues?.find((v) => v.culture === 'da-DK')?.value).to.equal('da-DK stays variant');
	});

	it('preserves all segment values for the default language when changing to invariant', async () => {
		// Set up initial data with multiple cultures and segments
		mockWorkspace.setData({
			unique: 'test-doc',
			values: [
				{
					alias: 'testProperty',
					value: 'en-US default segment',
					culture: 'en-US',
					segment: null,
					editorAlias: 'test-editor',
				},
				{
					alias: 'testProperty',
					value: 'en-US segment A',
					culture: 'en-US',
					segment: 'segmentA',
					editorAlias: 'test-editor',
				},
				{
					alias: 'testProperty',
					value: 'en-US segment B',
					culture: 'en-US',
					segment: 'segmentB',
					editorAlias: 'test-editor',
				},
				{
					alias: 'testProperty',
					value: 'da-DK default segment',
					culture: 'da-DK',
					segment: null,
					editorAlias: 'test-editor',
				},
				{
					alias: 'testProperty',
					value: 'da-DK segment A',
					culture: 'da-DK',
					segment: 'segmentA',
					editorAlias: 'test-editor',
				},
			],
		} as UmbContentDetailModel<UmbEntityVariantModel>);

		// Set initial property types (variant)
		const oldPropertyTypes: Array<UmbPropertyTypeModel> = [
			{
				alias: 'testProperty',
				variesByCulture: true,
				variesBySegment: true,
			} as UmbPropertyTypeModel,
		];
		mockWorkspace.setPropertyTypes(oldPropertyTypes);

		// Create controller
		new UmbContentDetailWorkspaceTypeTransformController(mockWorkspace as any);

		// Wait for initial observation
		await new Promise((resolve) => setTimeout(resolve, 0));

		// Change property to be invariant (culture only, segments still vary)
		const newPropertyTypes: Array<UmbPropertyTypeModel> = [
			{
				alias: 'testProperty',
				variesByCulture: false,
				variesBySegment: true,
			} as UmbPropertyTypeModel,
		];
		mockWorkspace.setPropertyTypes(newPropertyTypes);

		// Wait for observation to trigger
		await new Promise((resolve) => setTimeout(resolve, 0));

		// Should keep all segment values for the default language (en-US) and discard da-DK values
		const data = mockWorkspace.getData();
		const testPropValues = data?.values.filter((v) => v.alias === 'testProperty');

		expect(testPropValues?.length).to.equal(3);

		// All values should now have culture: null
		expect(testPropValues?.every((v) => v.culture === null)).to.be.true;

		// Should have all three segments from en-US
		const defaultSegment = testPropValues?.find((v) => v.segment === null);
		expect(defaultSegment?.value).to.equal('en-US default segment');

		const segmentA = testPropValues?.find((v) => v.segment === 'segmentA');
		expect(segmentA?.value).to.equal('en-US segment A');

		const segmentB = testPropValues?.find((v) => v.segment === 'segmentB');
		expect(segmentB?.value).to.equal('en-US segment B');
	});

	it('preserves existing culture values and migrates invariant values when changing to variant', async () => {
		// Set up initial data with an invariant value AND an existing culture value
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
				{
					alias: 'testProperty',
					value: 'existing danish value',
					culture: 'da-DK',
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

		// Should keep existing da-DK value AND migrate invariant value to en-US
		const data = mockWorkspace.getData();
		expect(data?.values.length).to.equal(2);

		const enValue = data?.values.find((v) => v.culture === 'en-US');
		expect(enValue?.value).to.equal('invariant value');

		const daValue = data?.values.find((v) => v.culture === 'da-DK');
		expect(daValue?.value).to.equal('existing danish value');
	});

	it('does not overwrite existing culture+segment value with invariant value when changing to variant', async () => {
		// Set up initial data where a culture value already exists for the same segment as the invariant
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
				{
					alias: 'testProperty',
					value: 'existing en-US value',
					culture: 'en-US', // Same as default language
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

		// Should keep existing en-US value and NOT migrate invariant (would conflict with same culture+segment)
		const data = mockWorkspace.getData();
		expect(data?.values.length).to.equal(1);
		expect(data?.values[0]?.culture).to.equal('en-US');
		expect(data?.values[0]?.value).to.equal('existing en-US value');
	});

	it('preserves existing invariant values and migrates culture values when changing to invariant', async () => {
		// Set up initial data with culture values AND an existing invariant value
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
				{
					alias: 'testProperty',
					value: 'existing invariant for segmentA',
					culture: null,
					segment: 'segmentA',
					editorAlias: 'test-editor',
				},
				{
					alias: 'testProperty',
					value: 'en-US segmentA value',
					culture: 'en-US',
					segment: 'segmentA',
					editorAlias: 'test-editor',
				},
			],
		} as UmbContentDetailModel<UmbEntityVariantModel>);

		// Set initial property types (variant)
		const oldPropertyTypes: Array<UmbPropertyTypeModel> = [
			{
				alias: 'testProperty',
				variesByCulture: true,
				variesBySegment: true,
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
				variesBySegment: true,
			} as UmbPropertyTypeModel,
		];
		mockWorkspace.setPropertyTypes(newPropertyTypes);

		// Wait for observation to trigger
		await new Promise((resolve) => setTimeout(resolve, 0));

		const data = mockWorkspace.getData();
		const testPropValues = data?.values.filter((v) => v.alias === 'testProperty');

		// Should have 2 values: migrated en-US (null segment) and existing invariant (segmentA)
		expect(testPropValues?.length).to.equal(2);

		// All should be invariant now
		expect(testPropValues?.every((v) => v.culture === null)).to.be.true;

		// Default segment should be migrated from en-US
		const defaultSegment = testPropValues?.find((v) => v.segment === null);
		expect(defaultSegment?.value).to.equal('en-US value');

		// SegmentA should keep existing invariant value (not overwritten by en-US segmentA)
		const segmentA = testPropValues?.find((v) => v.segment === 'segmentA');
		expect(segmentA?.value).to.equal('existing invariant for segmentA');
	});

	it('does not overwrite existing invariant+segment value with culture value when changing to invariant', async () => {
		// Set up initial data where an invariant value already exists for the same segment
		mockWorkspace.setData({
			unique: 'test-doc',
			values: [
				{
					alias: 'testProperty',
					value: 'existing invariant value',
					culture: null,
					segment: null,
					editorAlias: 'test-editor',
				},
				{
					alias: 'testProperty',
					value: 'en-US value',
					culture: 'en-US',
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

		// Should keep existing invariant value and NOT migrate en-US (would conflict with same segment)
		const data = mockWorkspace.getData();
		expect(data?.values.length).to.equal(1);
		expect(data?.values[0]?.culture).to.be.null;
		expect(data?.values[0]?.value).to.equal('existing invariant value');
	});

	it('preserves all segment values for fallback culture when default language has no values', async () => {
		// Set up initial data with only non-default language values with segments
		mockWorkspace.setData({
			unique: 'test-doc',
			values: [
				{
					alias: 'testProperty',
					value: 'da-DK default segment',
					culture: 'da-DK',
					segment: null,
					editorAlias: 'test-editor',
				},
				{
					alias: 'testProperty',
					value: 'da-DK segment A',
					culture: 'da-DK',
					segment: 'segmentA',
					editorAlias: 'test-editor',
				},
			],
		} as UmbContentDetailModel<UmbEntityVariantModel>);

		// Set initial property types (variant)
		const oldPropertyTypes: Array<UmbPropertyTypeModel> = [
			{
				alias: 'testProperty',
				variesByCulture: true,
				variesBySegment: true,
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
				variesBySegment: true,
			} as UmbPropertyTypeModel,
		];
		mockWorkspace.setPropertyTypes(newPropertyTypes);

		// Wait for observation to trigger
		await new Promise((resolve) => setTimeout(resolve, 0));

		// Should keep all segment values from da-DK (fallback since en-US has no values)
		const data = mockWorkspace.getData();
		const testPropValues = data?.values.filter((v) => v.alias === 'testProperty');

		expect(testPropValues?.length).to.equal(2);

		// All values should now have culture: null
		expect(testPropValues?.every((v) => v.culture === null)).to.be.true;

		// Should have both segments from da-DK
		const defaultSegment = testPropValues?.find((v) => v.segment === null);
		expect(defaultSegment?.value).to.equal('da-DK default segment');

		const segmentA = testPropValues?.find((v) => v.segment === 'segmentA');
		expect(segmentA?.value).to.equal('da-DK segment A');
	});
});
