import type { UmbMockDocumentTypeModel } from '../../mock-data-set.types.js';
import { CompositionTypeModel } from '@umbraco-cms/backoffice/external/backend-api';

// Map string composition type to enum
/**
 *
 * @param type
 */
function mapCompositionType(type: string): CompositionTypeModel {
	switch (type) {
		case 'Composition':
			return CompositionTypeModel.COMPOSITION;
		case 'Inheritance':
			return CompositionTypeModel.INHERITANCE;
		default:
			return CompositionTypeModel.COMPOSITION;
	}
}

const rawData = [
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		alias: 'testdocumenttypes',
		name: 'Test Document Types',
		description: null,
		icon: 'icon-folder',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: true,
		parent: null,
		isFolder: true,
		properties: [],
		containers: [],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: 'a29519c1-1605-4811-8830-dde83e09d892',
		alias: 'testelementtypes',
		name: 'Test Element Types',
		description: null,
		icon: 'icon-folder',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: true,
		parent: null,
		isFolder: true,
		properties: [],
		containers: [],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: '7184285e-9709-4e13-8c72-1fe52f024b28',
		alias: 'home',
		name: 'Home',
		description: null,
		icon: 'icon-home color-black',
		allowedAsRoot: true,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: null,
		isFolder: false,
		properties: [
			{
				id: 'pt-153',
				container: {
					id: 'f4d9a778-8e32-40a7-972c-5faa17e52599',
				},
				alias: 'test',
				name: 'Test',
				description: null,
				dataType: {
					id: '0cc0eba1-9960-42c9-bf9b-60e150b429ae',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: true,
					mandatoryMessage: '#validation_mandatory-field',
					regEx: '^[0-9]*$',
					regExMessage: '#validation_numeric-field',
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: 'f4d9a778-8e32-40a7-972c-5faa17e52599',
				parent: null,
				name: 'Content',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [
			{
				documentType: {
					id: '015bc281-7410-40e2-81b5-b8f7c963bd61',
				},
				sortOrder: 4,
			},
			{
				documentType: {
					id: '13c10f78-bf14-411d-9444-751e4bd1b178',
				},
				sortOrder: 5,
			},
			{
				documentType: {
					id: '41f34bb7-fd63-442f-8dcb-142df4246310',
				},
				sortOrder: 6,
			},
			{
				documentType: {
					id: '9cff8f66-0e13-4617-ab9b-9f845ecc5e24',
				},
				sortOrder: 7,
			},
			{
				documentType: {
					id: '0180d16d-6a87-4631-9802-4e1b1f180bd4',
				},
				sortOrder: 8,
			},
			{
				documentType: {
					id: 'fb88c3ab-40ee-4822-a63e-0edd97ad13f8',
				},
				sortOrder: 9,
			},
			{
				documentType: {
					id: '8856d507-76e0-47c7-8564-56467e717053',
				},
				sortOrder: 10,
			},
			{
				documentType: {
					id: '99431793-6f52-48c7-af53-6bf04668aca2',
				},
				sortOrder: 14,
			},
			{
				documentType: {
					id: '7b61b708-aa42-4978-a86c-f20fd4749a58',
				},
				sortOrder: 15,
			},
			{
				documentType: {
					id: 'f7c73e80-e8f4-4ef6-a710-168d89991c7d',
				},
				sortOrder: 16,
			},
			{
				documentType: {
					id: '48a02560-7ce9-4be4-96e7-e4041cc19622',
				},
				sortOrder: 17,
			},
			{
				documentType: {
					id: '727b819b-af42-443f-a752-c4c5cfd69313',
				},
				sortOrder: 18,
			},
			{
				documentType: {
					id: '7025ee6c-8d6c-4abd-8e32-2cab5fde6f90',
				},
				sortOrder: 19,
			},
			{
				documentType: {
					id: 'cc827fc0-e385-494b-88f6-d4abb47b7081',
				},
				sortOrder: 20,
			},
			{
				documentType: {
					id: 'f984a2dc-01c0-4974-a860-b41dfeacf2b5',
				},
				sortOrder: 21,
			},
			{
				documentType: {
					id: '7b52f09a-3034-43d6-a83e-5f9fadfcc87d',
				},
				sortOrder: 22,
			},
			{
				documentType: {
					id: 'fd62fafc-9cfd-470a-a260-93af5d1ed641',
				},
				sortOrder: 23,
			},
			{
				documentType: {
					id: '23c4c503-bcdf-46a5-9ff9-fb78d9dba4ae',
				},
				sortOrder: 24,
			},
			{
				documentType: {
					id: 'af83a333-d5f9-4467-9013-9eaa8112a571',
				},
				sortOrder: 26,
			},
			{
				documentType: {
					id: '373eaceb-e41e-4dd2-ae3f-b73fd11cf182',
				},
				sortOrder: 28,
			},
			{
				documentType: {
					id: '6717ef28-57a2-4cb4-80fe-ddc7a76da5f4',
				},
				sortOrder: 27,
			},
			{
				documentType: {
					id: 'b85bb884-ed5e-4f0b-8b10-8067090e8ada',
				},
				sortOrder: 3,
			},
			{
				documentType: {
					id: '6dcde803-d22e-4fcf-85a3-3a03be080d3a',
				},
				sortOrder: 25,
			},
			{
				documentType: {
					id: 'dc965257-84c2-4f27-b452-55e8b0f91a96',
				},
				sortOrder: 29,
			},
			{
				documentType: {
					id: '25dd3762-cfdd-43cd-b0a5-8f094f8a7fd2',
				},
				sortOrder: 13,
			},
			{
				documentType: {
					id: '2a773487-9de7-403c-9207-54f4ace7f215',
				},
				sortOrder: 12,
			},
			{
				documentType: {
					id: '61c6b912-8fe8-4e10-a07b-4f777b99489b',
				},
				sortOrder: 1,
			},
			{
				documentType: {
					id: '1addd0ad-0e34-4386-801b-79cf7beb8cf1',
				},
				sortOrder: 0,
			},
			{
				documentType: {
					id: '11b48beb-3fd0-4b72-800e-364f6e833dc7',
				},
				sortOrder: 11,
			},
			{
				documentType: {
					id: '9309d592-ebc7-4f72-a1bd-ebdabca4c643',
				},
				sortOrder: 2,
			},
		],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: '015bc281-7410-40e2-81b5-b8f7c963bd61',
		alias: 'colorPickerDocType',
		name: 'Color Picker',
		description: null,
		icon: 'icon-colorpicker color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-52',
				container: {
					id: '7c07140b-4715-43ab-aa18-01e1456bf72f',
				},
				alias: 'colorPickerNoLabels',
				name: 'Color Picker - No Labels',
				description: null,
				dataType: {
					id: 'a62e05d6-f7f8-4929-b35b-2e3068692eb6',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-53',
				container: {
					id: '7c07140b-4715-43ab-aa18-01e1456bf72f',
				},
				alias: 'colorPickerLabels',
				name: 'Color Picker - Labels',
				description: null,
				dataType: {
					id: 'c4fb2e7f-c707-41c4-994f-b224d0b66612',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 1,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: '7c07140b-4715-43ab-aa18-01e1456bf72f',
				parent: null,
				name: 'Color Pickers',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: 'b818bb55-31e1-4537-9c42-17471a176089',
		alias: 'elementOneElementType',
		name: 'Element One',
		description: null,
		icon: 'icon-attachment color-deep-purple',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: true,
		hasChildren: false,
		parent: {
			id: 'a29519c1-1605-4811-8830-dde83e09d892',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-144',
				container: {
					id: '4f13c5d7-3e86-464b-b0d1-2030f0253450',
				},
				alias: 'title',
				name: 'Title',
				description: null,
				dataType: {
					id: '0cc0eba1-9960-42c9-bf9b-60e150b429ae',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: '4f13c5d7-3e86-464b-b0d1-2030f0253450',
				parent: null,
				name: 'Content',
				type: 'Group',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: '13c10f78-bf14-411d-9444-751e4bd1b178',
		alias: 'contentPickerDocType',
		name: 'Content Picker',
		description: null,
		icon: 'icon-autofill color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-54',
				container: {
					id: '38c46685-f235-4584-b245-11553d500484',
				},
				alias: 'contentPickerDefaultConfig',
				name: 'Content Picker - Default Config',
				description: null,
				dataType: {
					id: '1bd0d68f-8fe9-4906-bb5e-e33eafa83aa3',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-55',
				container: {
					id: '38c46685-f235-4584-b245-11553d500484',
				},
				alias: 'contentPickerIgnoreUserStartNodes',
				name: 'Content Picker - Ignore User Start Nodes',
				description: null,
				dataType: {
					id: 'b01b3451-7875-4ac9-a772-b2f23b865af3',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 1,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-56',
				container: {
					id: '38c46685-f235-4584-b245-11553d500484',
				},
				alias: 'contentPickerShowOpenButton',
				name: 'Content Picker - Show Open Button',
				description: null,
				dataType: {
					id: '8aa44228-5263-4395-9588-9ba401d9e0a1',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 2,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-57',
				container: {
					id: '38c46685-f235-4584-b245-11553d500484',
				},
				alias: 'contentPickerStartNode',
				name: 'Content Picker - Start Node',
				description: null,
				dataType: {
					id: 'adcccf89-532f-4a50-83c8-e742035a12a3',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 3,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: '38c46685-f235-4584-b245-11553d500484',
				parent: null,
				name: 'Content Pickers',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: 'f7f156a0-a3f3-42ec-8b9c-e788157bd84e',
		alias: 'elementTwoElementType',
		name: 'Element Two',
		description: null,
		icon: 'icon-blueprint color-deep-purple',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: true,
		hasChildren: false,
		parent: {
			id: 'a29519c1-1605-4811-8830-dde83e09d892',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-145',
				container: {
					id: '4fe71f18-3851-4335-8875-efe72e7507db',
				},
				alias: 'link',
				name: 'Link',
				description: null,
				dataType: {
					id: '1bd0d68f-8fe9-4906-bb5e-e33eafa83aa3',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: '4fe71f18-3851-4335-8875-efe72e7507db',
				parent: null,
				name: 'Content',
				type: 'Group',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: '41f34bb7-fd63-442f-8dcb-142df4246310',
		alias: 'dateTypePickerDocType',
		name: 'DateType Picker',
		description: null,
		icon: 'icon-time color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-58',
				container: {
					id: 'fa9fb286-8ab3-492a-90fc-5777c0b00742',
				},
				alias: 'dateTimePickerDateFormat',
				name: 'DateTime Picker - Date Format',
				description: null,
				dataType: {
					id: '59bbc6f4-9515-4264-bad5-66762e50c3d6',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-59',
				container: {
					id: 'fa9fb286-8ab3-492a-90fc-5777c0b00742',
				},
				alias: 'dateTimePickerDatePlusTimeFormat',
				name: 'DateTime Picker - Date + Time Format',
				description: null,
				dataType: {
					id: 'e8602b3f-8b89-4f91-8c94-d2fd7df534e6',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 1,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-60',
				container: {
					id: 'fa9fb286-8ab3-492a-90fc-5777c0b00742',
				},
				alias: 'dateTimePickerOffsetTime',
				name: 'DateTime Picker - Offset Time',
				description: null,
				dataType: {
					id: '3773d64f-495d-4e88-9ec4-7eb58c334687',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 2,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: 'fa9fb286-8ab3-492a-90fc-5777c0b00742',
				parent: null,
				name: 'DateTime Pickers',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: '9cff8f66-0e13-4617-ab9b-9f845ecc5e24',
		alias: 'decimalDocType',
		name: 'Decimal',
		description: null,
		icon: 'icon-autofill color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-61',
				container: {
					id: 'e629b81a-6b75-4b7f-9bfa-1bfc29b6e2a2',
				},
				alias: 'decimalDefaultConfig',
				name: 'Decimal - Default Config',
				description: null,
				dataType: {
					id: 'bed85b43-5d17-4676-9cf7-56bd193a053b',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-62',
				container: {
					id: 'e629b81a-6b75-4b7f-9bfa-1bfc29b6e2a2',
				},
				alias: 'decimalFullyConfigured',
				name: 'Decimal - Fully Configured',
				description: null,
				dataType: {
					id: '9ca94289-e14d-460a-bfd1-cb958ed90da5',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 1,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: 'e629b81a-6b75-4b7f-9bfa-1bfc29b6e2a2',
				parent: null,
				name: 'Decimals',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: '0180d16d-6a87-4631-9802-4e1b1f180bd4',
		alias: 'dropdownDocType',
		name: 'Dropdown',
		description: null,
		icon: 'icon-indent color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-63',
				container: {
					id: 'de50f42a-9d88-4c92-b297-a3a61dee7dcc',
				},
				alias: 'dropdownMultiValue',
				name: 'Dropdown - Multi Value',
				description: null,
				dataType: {
					id: '779051c2-7bb7-4ab4-82ac-698faa8286aa',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-64',
				container: {
					id: 'de50f42a-9d88-4c92-b297-a3a61dee7dcc',
				},
				alias: 'dropdownSingleValue',
				name: 'Dropdown - Single Value',
				description: null,
				dataType: {
					id: '3c1f48e0-6eec-44f3-8072-3e22d442a0a0',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 1,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: 'de50f42a-9d88-4c92-b297-a3a61dee7dcc',
				parent: null,
				name: 'Dropdowns',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: 'fb88c3ab-40ee-4822-a63e-0edd97ad13f8',
		alias: 'emailAddressDocType',
		name: 'Email Address',
		description: null,
		icon: 'icon-message color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-65',
				container: {
					id: 'f8082afc-a3fd-4d32-8927-c7159290fb8c',
				},
				alias: 'emailAddress',
				name: 'Email Address',
				description: null,
				dataType: {
					id: '51be6133-1828-4c0a-8db6-7a1df05da7c8',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: 'f8082afc-a3fd-4d32-8927-c7159290fb8c',
				parent: null,
				name: 'Email Address',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: '8856d507-76e0-47c7-8564-56467e717053',
		alias: 'eyeDropperColorPickerDocType',
		name: 'Eye Dropper Color Picker',
		description: null,
		icon: 'icon-colorpicker color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-66',
				container: {
					id: '3f800461-2b29-45a0-a8ad-dc06e43ce6fe',
				},
				alias: 'eyeDropperColorPickerDefaultConfig',
				name: 'Eye Dropper Color Picker - Default Config',
				description: null,
				dataType: {
					id: 'b9484af9-4a64-4aa0-a200-2cfd344c1aa5',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-67',
				container: {
					id: '3f800461-2b29-45a0-a8ad-dc06e43ce6fe',
				},
				alias: 'eyeDropperColorPickerAlpha',
				name: 'Eye Dropper Color Picker - Alpha',
				description: null,
				dataType: {
					id: '36c5e4da-fdf5-4ad7-8a1a-30bcaf453cc8',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 1,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-68',
				container: {
					id: '3f800461-2b29-45a0-a8ad-dc06e43ce6fe',
				},
				alias: 'eyeDropperColorPickerPalette',
				name: 'Eye Dropper Color Picker - Palette',
				description: null,
				dataType: {
					id: 'dcc7e763-4715-47ad-9215-a626e33a4a9f',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 2,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-69',
				container: {
					id: '3f800461-2b29-45a0-a8ad-dc06e43ce6fe',
				},
				alias: 'eyeDropperColorPickerFullyConfigured',
				name: 'Eye Dropper Color Picker - Fully Configured',
				description: null,
				dataType: {
					id: 'fb266db1-3e18-4cf2-9cf6-6f5096b12076',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 3,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: '3f800461-2b29-45a0-a8ad-dc06e43ce6fe',
				parent: null,
				name: 'Eye Dropper Color Pickers',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: '99431793-6f52-48c7-af53-6bf04668aca2',
		alias: 'markdownEditorDocType',
		name: 'Markdown Editor',
		description: null,
		icon: 'icon-code color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-70',
				container: {
					id: '7a2361ed-3f5f-4db5-af90-07f8a7bb60cb',
				},
				alias: 'markdownEditorDefaultConfig',
				name: 'Markdown Editor - Default Config',
				description: null,
				dataType: {
					id: '4f14fe46-522a-4994-ad67-451f78c5d8f6',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-71',
				container: {
					id: '7a2361ed-3f5f-4db5-af90-07f8a7bb60cb',
				},
				alias: 'markdownEditorFullyConfigured',
				name: 'Markdown Editor - Fully Configured',
				description: null,
				dataType: {
					id: '3af82553-4d1e-42c4-9e93-880de599c7b3',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 1,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-72',
				container: {
					id: '7a2361ed-3f5f-4db5-af90-07f8a7bb60cb',
				},
				alias: 'markdownEditorDefaultValue',
				name: 'Markdown Editor - Default Value',
				description: null,
				dataType: {
					id: 'e56554ed-95a4-43c4-96b4-0122569f4193',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 2,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-73',
				container: {
					id: '7a2361ed-3f5f-4db5-af90-07f8a7bb60cb',
				},
				alias: 'markdownEditorLargeOverlaySize',
				name: 'Markdown Editor - Large Overlay Size',
				description: null,
				dataType: {
					id: '9450742c-31ce-4db2-85d5-c85238473c39',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 3,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-74',
				container: {
					id: '7a2361ed-3f5f-4db5-af90-07f8a7bb60cb',
				},
				alias: 'markdownEditorPreview',
				name: 'Markdown Editor - Preview',
				description: null,
				dataType: {
					id: 'a5b1b0d5-f905-413f-8e00-aeb8bf4c0999',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 4,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: '7a2361ed-3f5f-4db5-af90-07f8a7bb60cb',
				parent: null,
				name: 'Markdown Editors',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: '7b61b708-aa42-4978-a86c-f20fd4749a58',
		alias: 'mediaPickerDocType',
		name: 'Media Picker',
		description: null,
		icon: 'icon-umb-media color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-75',
				container: {
					id: 'a2114ac3-a87a-4c7d-a882-d55cefebbf2c',
				},
				alias: 'mediaPickerDefaultConfig',
				name: 'Media Picker - Default Config',
				description: null,
				dataType: {
					id: '87543f25-f2dc-41b4-b861-75159b7baff9',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-76',
				container: {
					id: 'a2114ac3-a87a-4c7d-a882-d55cefebbf2c',
				},
				alias: 'mediaPickerAcceptedTypes',
				name: 'Media Picker - Accepted Types',
				description: null,
				dataType: {
					id: 'ccff0509-5a13-4e86-a341-9b91868922ae',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 1,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-77',
				container: {
					id: 'a2114ac3-a87a-4c7d-a882-d55cefebbf2c',
				},
				alias: 'mediaPickerAmount',
				name: 'Media Picker - Amount',
				description: null,
				dataType: {
					id: 'ff53e2ac-d54b-4c78-a5cc-2bc050605854',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 2,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-78',
				container: {
					id: 'a2114ac3-a87a-4c7d-a882-d55cefebbf2c',
				},
				alias: 'mediaPickerCrops',
				name: 'Media Picker - Crops',
				description: null,
				dataType: {
					id: '0e33a2de-d6a3-4853-8a68-7f56667f1d75',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 3,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-79',
				container: {
					id: 'a2114ac3-a87a-4c7d-a882-d55cefebbf2c',
				},
				alias: 'mediaPickerFocalPoint',
				name: 'Media Picker - Focal Point',
				description: null,
				dataType: {
					id: 'a6089297-6132-4f18-83da-bf11acb5e8e2',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 4,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-80',
				container: {
					id: 'a2114ac3-a87a-4c7d-a882-d55cefebbf2c',
				},
				alias: 'mediaPickerIgnoreUserStartNodes',
				name: 'Media Picker - Ignore User Start Nodes',
				description: null,
				dataType: {
					id: '7c67bee1-7206-40c3-a8f3-cdad500c021b',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 5,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-81',
				container: {
					id: 'a2114ac3-a87a-4c7d-a882-d55cefebbf2c',
				},
				alias: 'mediaPickerPickMultipleItems',
				name: 'Media Picker - Pick Multiple Items',
				description: null,
				dataType: {
					id: 'fec81092-5db6-45b2-8050-09dcfead2901',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 6,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-82',
				container: {
					id: 'a2114ac3-a87a-4c7d-a882-d55cefebbf2c',
				},
				alias: 'mediaPickerPickMultipleItemsWithAmount',
				name: 'Media Picker - Pick Multiple Items, With Amount',
				description: null,
				dataType: {
					id: 'efe5ab7e-e78a-4b3c-8fe2-3c711e073a5d',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 7,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-83',
				container: {
					id: 'a2114ac3-a87a-4c7d-a882-d55cefebbf2c',
				},
				alias: 'mediaPickerStartNode',
				name: 'Media Picker - Start Node',
				description: null,
				dataType: {
					id: 'c686c86b-8941-4656-8f24-eea0afb0abb9',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 8,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-84',
				container: {
					id: 'a2114ac3-a87a-4c7d-a882-d55cefebbf2c',
				},
				alias: 'mediaPickerFullyConfigured',
				name: 'Media Picker - Fully Configured',
				description: null,
				dataType: {
					id: '30f7ee63-69da-4b1f-a8f8-4c5ed882be17',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 9,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: 'a2114ac3-a87a-4c7d-a882-d55cefebbf2c',
				parent: null,
				name: 'Media Pickers',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: 'f7c73e80-e8f4-4ef6-a710-168d89991c7d',
		alias: 'memberGroupPickerDocType',
		name: 'Member Group Picker',
		description: null,
		icon: 'icon-users-alt color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-85',
				container: {
					id: 'dc0c9e8c-e488-4a29-ae72-7e5791078719',
				},
				alias: 'memberGroupPicker',
				name: 'Member Group Picker',
				description: null,
				dataType: {
					id: '2ac54465-7f8c-481e-926b-6fcc8bef1dc3',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: 'dc0c9e8c-e488-4a29-ae72-7e5791078719',
				parent: null,
				name: 'Member Group Picker',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: '48a02560-7ce9-4be4-96e7-e4041cc19622',
		alias: 'memberPickerDocType',
		name: 'Member Picker',
		description: null,
		icon: 'icon-user color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-86',
				container: {
					id: 'f516f9ea-e299-4f9d-892e-46c39d93489c',
				},
				alias: 'memberPicker',
				name: 'Member Picker',
				description: null,
				dataType: {
					id: '2555acc6-6adf-4cc3-b0bd-86a2dfdcc7b1',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: 'f516f9ea-e299-4f9d-892e-46c39d93489c',
				parent: null,
				name: 'Member Picker',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: '727b819b-af42-443f-a752-c4c5cfd69313',
		alias: 'multiUrlPickerDocType',
		name: 'Multi URL Picker',
		description: null,
		icon: 'icon-link color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-87',
				container: {
					id: '5a327c88-8e0f-421b-abf0-01b07533685e',
				},
				alias: 'multiUrlPickerDefaultConfig',
				name: 'Multi URL Picker - Default Config',
				description: null,
				dataType: {
					id: 'f455a80c-7f39-4fbb-b212-cf829dd28f7b',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-88',
				container: {
					id: '5a327c88-8e0f-421b-abf0-01b07533685e',
				},
				alias: 'multiUrlPickerFullyConfigured',
				name: 'Multi URL Picker - Fully Configured',
				description: null,
				dataType: {
					id: '68bee672-9317-40a4-860c-32240d4a2926',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 1,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-89',
				container: {
					id: '5a327c88-8e0f-421b-abf0-01b07533685e',
				},
				alias: 'multiUrlPickerHideAnchorQueryString',
				name: 'Multi URL Picker - Hide Anchor Query String',
				description: null,
				dataType: {
					id: 'f088d56e-9efd-4c4d-8264-accbbe647181',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 2,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-90',
				container: {
					id: '5a327c88-8e0f-421b-abf0-01b07533685e',
				},
				alias: 'multiUrlPickerIgnoreUserStartNodes',
				name: 'Multi URL Picker - Ignore User Start Nodes',
				description: null,
				dataType: {
					id: '50a7ce3b-ba5a-4f20-8264-361d2194a15c',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 3,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-91',
				container: {
					id: '5a327c88-8e0f-421b-abf0-01b07533685e',
				},
				alias: 'multiUrlPickerLargeOverlaySize',
				name: 'Multi URL Picker - Large Overlay Size',
				description: null,
				dataType: {
					id: 'd6d131e2-822f-437a-a1bc-57ddf55e9a5f',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 4,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-92',
				container: {
					id: '5a327c88-8e0f-421b-abf0-01b07533685e',
				},
				alias: 'multiUrlPickerMinAndMax',
				name: 'Multi URL Picker - Min And Max',
				description: null,
				dataType: {
					id: '5efb7a21-cbfa-452b-a7f0-9b6e467f651c',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 5,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: '5a327c88-8e0f-421b-abf0-01b07533685e',
				parent: null,
				name: 'Multi URL Pickers',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: '7025ee6c-8d6c-4abd-8e32-2cab5fde6f90',
		alias: 'multinodeTreepickerDocType',
		name: 'Multinode Treepicker',
		description: null,
		icon: 'icon-page-add color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-93',
				container: {
					id: 'c697eddc-c31a-4886-b14c-22cd9718a477',
				},
				alias: 'multinodeTreepickerDefaultConfig',
				name: 'Multinode Treepicker - Default Config',
				description: null,
				dataType: {
					id: 'fe2a2728-c6bc-450b-9e63-a68d60638b7e',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-94',
				container: {
					id: 'c697eddc-c31a-4886-b14c-22cd9718a477',
				},
				alias: 'multinodeTreepickerAllowedTypes',
				name: 'Multinode Treepicker - Allowed Types',
				description: null,
				dataType: {
					id: '58269615-9b07-481d-aa44-0613b9157f3e',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 1,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-96',
				container: {
					id: 'c697eddc-c31a-4886-b14c-22cd9718a477',
				},
				alias: 'multinodeTreepickerMinAndMax',
				name: 'Multinode Treepicker - Min And Max',
				description: null,
				dataType: {
					id: '07af09f9-819e-404b-b012-226e8978f5c0',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 2,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-97',
				container: {
					id: 'c697eddc-c31a-4886-b14c-22cd9718a477',
				},
				alias: 'multinodeTreepickerStartNode',
				name: 'Multinode Treepicker - Start Node',
				description: null,
				dataType: {
					id: '75800052-b5c4-4a82-a6ed-a7972129bcf6',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 3,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-98',
				container: {
					id: 'c697eddc-c31a-4886-b14c-22cd9718a477',
				},
				alias: 'multinodeTreepickerXPathStartNode',
				name: 'Multinode Treepicker - XPath Start Node',
				description: null,
				dataType: {
					id: '612a8051-191c-4cbc-827f-aae9ec364fbc',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 4,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-95',
				container: {
					id: 'c697eddc-c31a-4886-b14c-22cd9718a477',
				},
				alias: 'multinodeTreepickerFullyConfigured',
				name: 'Multinode Treepicker - Fully Configured',
				description: null,
				dataType: {
					id: '98e2775d-37f6-4625-98f6-1a4cd4f7dac8',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 5,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-99',
				container: {
					id: 'c697eddc-c31a-4886-b14c-22cd9718a477',
				},
				alias: 'multinodeTreepickerMediaDefaultConfig',
				name: 'Multinode Treepicker - Media - Default Config',
				description: null,
				dataType: {
					id: '52d20340-cf21-4256-a136-55f91dbf353a',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 6,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-100',
				container: {
					id: 'c697eddc-c31a-4886-b14c-22cd9718a477',
				},
				alias: 'multinodeTreepickerMediaFullyConfigured',
				name: 'Multinode Treepicker - Media - Fully Configured',
				description: null,
				dataType: {
					id: '67782cbb-ee87-42cf-810a-7aab4b1cbcbc',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 7,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-101',
				container: {
					id: 'c697eddc-c31a-4886-b14c-22cd9718a477',
				},
				alias: 'multinodeTreepickerMembersDefaultConfig',
				name: 'Multinode Treepicker - Members - Default Config',
				description: null,
				dataType: {
					id: '9f1be990-9b28-4817-a662-841875071769',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 8,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-102',
				container: {
					id: 'c697eddc-c31a-4886-b14c-22cd9718a477',
				},
				alias: 'multinodeTreepickerMembersFullyConfigured',
				name: 'Multinode Treepicker - Members - Fully Configured',
				description: null,
				dataType: {
					id: 'f6d4679e-2090-4acb-b7c8-564d908c657c',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 9,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: 'c697eddc-c31a-4886-b14c-22cd9718a477',
				parent: null,
				name: 'Multinode Treepickers',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: 'cc827fc0-e385-494b-88f6-d4abb47b7081',
		alias: 'multipleTextstringDocType',
		name: 'Multiple Textstring',
		description: null,
		icon: 'icon-ordered-list color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-103',
				container: {
					id: '4745f3e2-a4b4-475d-884f-3a615f2e7328',
				},
				alias: 'multipleTextstringDefaultConfig',
				name: 'Multiple Textstring - Default Config',
				description: null,
				dataType: {
					id: 'a9c636c7-d500-4ce5-bfb1-2d508fe79d7c',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-105',
				container: {
					id: '4745f3e2-a4b4-475d-884f-3a615f2e7328',
				},
				alias: 'multipleTextstringMax',
				name: 'Multiple Textstring - Max',
				description: null,
				dataType: {
					id: 'b88cc0ae-9216-45de-83c2-5e92de3ae153',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 1,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-106',
				container: {
					id: '4745f3e2-a4b4-475d-884f-3a615f2e7328',
				},
				alias: 'multipleTextstringMin',
				name: 'Multiple Textstring - Min',
				description: null,
				dataType: {
					id: '26ecc485-c84f-4806-9445-3996da82d0bb',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 2,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-104',
				container: {
					id: '4745f3e2-a4b4-475d-884f-3a615f2e7328',
				},
				alias: 'multipleTextstringFullyConfigured',
				name: 'Multiple Textstring - Fully Configured',
				description: null,
				dataType: {
					id: '78350acf-b981-4a55-96f8-a91001c73eef',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 3,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: '4745f3e2-a4b4-475d-884f-3a615f2e7328',
				parent: null,
				name: 'Multiple Textstrings',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: 'f984a2dc-01c0-4974-a860-b41dfeacf2b5',
		alias: 'numericDocType',
		name: 'Numeric',
		description: null,
		icon: 'icon-autofill color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-107',
				container: {
					id: 'd1812831-2cb7-48bd-83d8-0decd916cd59',
				},
				alias: 'numericDefaultConfig',
				name: 'Numeric - Default Config',
				description: null,
				dataType: {
					id: 'fb9142d1-ce19-40dc-84cf-5f81edb11928',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-108',
				container: {
					id: 'd1812831-2cb7-48bd-83d8-0decd916cd59',
				},
				alias: 'numericMinAndMax',
				name: 'Numeric - Min And Max',
				description: null,
				dataType: {
					id: '1b026e4b-1486-44e1-91c5-62898e813036',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 1,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-109',
				container: {
					id: 'd1812831-2cb7-48bd-83d8-0decd916cd59',
				},
				alias: 'numericStepSize',
				name: 'Numeric - Step Size',
				description: null,
				dataType: {
					id: '5638868a-49df-44b5-8c05-42ca95d8476b',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 2,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-110',
				container: {
					id: 'd1812831-2cb7-48bd-83d8-0decd916cd59',
				},
				alias: 'numericFullyConfigured',
				name: 'Numeric - Fully Configured',
				description: null,
				dataType: {
					id: '4e7ffcd3-e8c6-450f-b5dc-c1bdcd234292',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 3,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: 'd1812831-2cb7-48bd-83d8-0decd916cd59',
				parent: null,
				name: 'Numerics',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: '7b52f09a-3034-43d6-a83e-5f9fadfcc87d',
		alias: 'radioButtonListDocType',
		name: 'Radio Button List',
		description: null,
		icon: 'icon-target color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-111',
				container: {
					id: '1c2c5bf0-b8a2-48eb-945b-1e872772c850',
				},
				alias: 'radioButtonList',
				name: 'Radio Button List',
				description: null,
				dataType: {
					id: '2d3a109a-de8d-4c12-af67-4f7a116cebe5',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: '1c2c5bf0-b8a2-48eb-945b-1e872772c850',
				parent: null,
				name: 'Radio Button List',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: 'fd62fafc-9cfd-470a-a260-93af5d1ed641',
		alias: 'richTextEditorDocType',
		name: 'Rich Text Editor',
		description: null,
		icon: 'icon-browser-window color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-112',
				container: {
					id: '3542f8d3-f045-4e0c-b48d-d2e3bb0d0a6b',
				},
				alias: 'richTextEditorDefaultConfig',
				name: 'Rich Text Editor - Default Config',
				description: null,
				dataType: {
					id: 'e9f410d1-1f37-401d-b3b3-4678e4aab5fa',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-113',
				container: {
					id: '3542f8d3-f045-4e0c-b48d-d2e3bb0d0a6b',
				},
				alias: 'richTextEditorDimensions',
				name: 'Rich Text Editor - Dimensions',
				description: null,
				dataType: {
					id: 'b005391c-4735-4967-9c16-939dc846acac',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 1,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-115',
				container: {
					id: '3542f8d3-f045-4e0c-b48d-d2e3bb0d0a6b',
				},
				alias: 'richTextEditorFullyConfigured',
				name: 'Rich Text Editor - Fully Configured',
				description: null,
				dataType: {
					id: 'dcede488-62e2-49ba-97a0-59c60ae09992',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 3,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: '3542f8d3-f045-4e0c-b48d-d2e3bb0d0a6b',
				parent: null,
				name: 'Rich Text Editors',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: '23c4c503-bcdf-46a5-9ff9-fb78d9dba4ae',
		alias: 'sliderDocType',
		name: 'Slider',
		description: null,
		icon: 'icon-navigation-horizontal color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-116',
				container: {
					id: 'cb366883-9d00-4d98-8d1f-2a10337a63ff',
				},
				alias: 'sliderDefaultConfig',
				name: 'Slider - Default Config',
				description: null,
				dataType: {
					id: 'afc36215-ae90-4290-bf1a-af6ce0ca3c67',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-117',
				container: {
					id: 'cb366883-9d00-4d98-8d1f-2a10337a63ff',
				},
				alias: 'sliderInitialValue',
				name: 'Slider - Initial Value',
				description: null,
				dataType: {
					id: '1158cb90-ad8e-4c62-9ed3-085691d61a39',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 1,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-118',
				container: {
					id: 'cb366883-9d00-4d98-8d1f-2a10337a63ff',
				},
				alias: 'sliderMinAndMax',
				name: 'Slider - Min And Max',
				description: null,
				dataType: {
					id: 'bf4179df-26ec-4c84-9607-0071785730dc',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 2,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-119',
				container: {
					id: 'cb366883-9d00-4d98-8d1f-2a10337a63ff',
				},
				alias: 'sliderStepIncrements',
				name: 'Slider - Step Increments',
				description: null,
				dataType: {
					id: '17d1054c-423a-4f89-a917-cbc055af6fa9',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 3,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-120',
				container: {
					id: 'cb366883-9d00-4d98-8d1f-2a10337a63ff',
				},
				alias: 'sliderFullyConfigured',
				name: 'Slider - Fully Configured',
				description: null,
				dataType: {
					id: 'dae71dda-3838-459e-b018-c432952288b0',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 4,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: 'cb366883-9d00-4d98-8d1f-2a10337a63ff',
				parent: null,
				name: 'Sliders',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: 'af83a333-d5f9-4467-9013-9eaa8112a571',
		alias: 'textareaDocType',
		name: 'Textarea',
		description: null,
		icon: 'icon-application-window-alt color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-121',
				container: {
					id: '1f6e9b00-2294-48f0-bd40-7bc118b992ad',
				},
				alias: 'textareaDefaultConfig',
				name: 'Textarea - Default Config',
				description: null,
				dataType: {
					id: '4a0ca16a-71bc-471b-b002-5a4b1999d2fa',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-123',
				container: {
					id: '1f6e9b00-2294-48f0-bd40-7bc118b992ad',
				},
				alias: 'textareaMaxChars',
				name: 'Textarea - Max Chars',
				description: null,
				dataType: {
					id: '55dcc374-b218-450d-90c3-db3d163dde02',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 1,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-124',
				container: {
					id: '1f6e9b00-2294-48f0-bd40-7bc118b992ad',
				},
				alias: 'textareaRows',
				name: 'Textarea - Rows',
				description: null,
				dataType: {
					id: '0afa9c4b-e2f2-450b-bb13-2e2684a5fe65',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 2,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-122',
				container: {
					id: '1f6e9b00-2294-48f0-bd40-7bc118b992ad',
				},
				alias: 'textareaFullyConfigured',
				name: 'Textarea - Fully Configured',
				description: null,
				dataType: {
					id: '0fa454ae-8671-4c69-a754-7ee738bab707',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 3,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: '1f6e9b00-2294-48f0-bd40-7bc118b992ad',
				parent: null,
				name: 'Textareas',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: '373eaceb-e41e-4dd2-ae3f-b73fd11cf182',
		alias: 'toggleDocType',
		name: 'Toggle',
		description: null,
		icon: 'icon-checkbox color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-125',
				container: {
					id: '5abf162a-2d2c-43da-ac2a-21d1cb4c2f04',
				},
				alias: 'toggleDefaultConfig',
				name: 'Toggle - Default Config',
				description: null,
				dataType: {
					id: 'b6c1b8fc-7aaf-4177-b916-44cda751436d',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-127',
				container: {
					id: '5abf162a-2d2c-43da-ac2a-21d1cb4c2f04',
				},
				alias: 'toggleInitialState',
				name: 'Toggle - Initial State',
				description: null,
				dataType: {
					id: '768f8670-982d-4911-886c-6f97fedf022a',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 1,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-128',
				container: {
					id: '5abf162a-2d2c-43da-ac2a-21d1cb4c2f04',
				},
				alias: 'toggleLabels',
				name: 'Toggle - Labels',
				description: null,
				dataType: {
					id: '1522cae7-487c-427c-9bd1-aecc5eb7fab9',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 2,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-126',
				container: {
					id: '5abf162a-2d2c-43da-ac2a-21d1cb4c2f04',
				},
				alias: 'toggleFullyConfigured',
				name: 'Toggle - Fully Configured',
				description: null,
				dataType: {
					id: 'afb3d794-8214-4888-8030-b3def1e5fe27',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 3,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: '5abf162a-2d2c-43da-ac2a-21d1cb4c2f04',
				parent: null,
				name: 'Toggles',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: '6717ef28-57a2-4cb4-80fe-ddc7a76da5f4',
		alias: 'textboxDocType',
		name: 'Textbox',
		description: null,
		icon: 'icon-autofill color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-129',
				container: {
					id: 'aa20dc91-4bb6-4ec4-980a-a1af95d5b6d8',
				},
				alias: 'textboxDefaultConfig',
				name: 'Textbox - Default Config',
				description: null,
				dataType: {
					id: '3a9b04b9-e96a-46d0-bd84-2c13fc36b70c',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-130',
				container: {
					id: 'aa20dc91-4bb6-4ec4-980a-a1af95d5b6d8',
				},
				alias: 'textboxMaxChars',
				name: 'Textbox - Max Chars',
				description: null,
				dataType: {
					id: '75c0a6a9-61a4-49e2-a8a9-5f19b72760e5',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 1,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: 'aa20dc91-4bb6-4ec4-980a-a1af95d5b6d8',
				parent: null,
				name: 'Textboxes',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: 'b85bb884-ed5e-4f0b-8b10-8067090e8ada',
		alias: 'checkboxListDocType',
		name: 'Checkbox List',
		description: null,
		icon: 'icon-bulleted-list color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-131',
				container: {
					id: '4b4223ef-7957-4be6-9a44-568aa6930b4f',
				},
				alias: 'checkboxList',
				name: 'Checkbox List',
				description: null,
				dataType: {
					id: 'dfa2595b-165c-48a7-b6ff-820914484c12',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: '4b4223ef-7957-4be6-9a44-568aa6930b4f',
				parent: null,
				name: 'Checkbox List',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: '6dcde803-d22e-4fcf-85a3-3a03be080d3a',
		alias: 'tagsDocType',
		name: 'Tags',
		description: null,
		icon: 'icon-tags color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-132',
				container: {
					id: '26e56467-05e0-4e57-853c-f48e8148e8aa',
				},
				alias: 'tagsDefaultGroupJSONStorage',
				name: 'Tags - Default Group, JSON Storage',
				description: null,
				dataType: {
					id: '965dc042-216d-4b0b-84ff-fb0050aed8e8',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-133',
				container: {
					id: '26e56467-05e0-4e57-853c-f48e8148e8aa',
				},
				alias: 'tagsCustomGroupCSVStorage',
				name: 'Tags - Custom Group, CSV Storage',
				description: null,
				dataType: {
					id: 'c6d5b717-ad81-431f-81e4-b289801eb802',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 1,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: '26e56467-05e0-4e57-853c-f48e8148e8aa',
				parent: null,
				name: 'Tags',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: 'dc965257-84c2-4f27-b452-55e8b0f91a96',
		alias: 'userPickerDocType',
		name: 'User Picker',
		description: null,
		icon: 'icon-user color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-134',
				container: {
					id: 'b439bb1f-215f-4dcc-9db5-d27354ad61ef',
				},
				alias: 'userPicker',
				name: 'User Picker',
				description: null,
				dataType: {
					id: '3387e5da-4e32-43dc-b4dc-840fcbc468f9',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: 'b439bb1f-215f-4dcc-9db5-d27354ad61ef',
				parent: null,
				name: 'User Picker',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: '25dd3762-cfdd-43cd-b0a5-8f094f8a7fd2',
		alias: 'labelDocType',
		name: 'Label',
		description: null,
		icon: 'icon-readonly color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-135',
				container: {
					id: '5ab66da4-12dd-4b79-bb8a-bab68bc7a7ac',
				},
				alias: 'labelString',
				name: 'Label - String',
				description: null,
				dataType: {
					id: 'b8164bfc-ae5d-4eee-b80e-bdf00745abba',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-136',
				container: {
					id: '5ab66da4-12dd-4b79-bb8a-bab68bc7a7ac',
				},
				alias: 'labelDecimal',
				name: 'Label - Decimal',
				description: null,
				dataType: {
					id: '3145614f-1e5e-47d2-a587-d7eb2d937a8f',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 1,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-137',
				container: {
					id: '5ab66da4-12dd-4b79-bb8a-bab68bc7a7ac',
				},
				alias: 'labelDateTime',
				name: 'Label - DateTime',
				description: null,
				dataType: {
					id: 'd9e26ead-b55f-4b24-96be-3da10ce87241',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 2,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-138',
				container: {
					id: '5ab66da4-12dd-4b79-bb8a-bab68bc7a7ac',
				},
				alias: 'labelTime',
				name: 'Label - Time',
				description: null,
				dataType: {
					id: '3c78f54b-0812-4f7c-a483-29c54583bf9f',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 3,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-139',
				container: {
					id: '5ab66da4-12dd-4b79-bb8a-bab68bc7a7ac',
				},
				alias: 'labelInteger',
				name: 'Label - Integer',
				description: null,
				dataType: {
					id: '0169a2ba-63b5-442d-af00-98d54bf959d9',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 4,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-140',
				container: {
					id: '5ab66da4-12dd-4b79-bb8a-bab68bc7a7ac',
				},
				alias: 'labelBigInteger',
				name: 'Label - Big Integer',
				description: null,
				dataType: {
					id: '59171d58-e368-42dd-88a9-a3504561442c',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 5,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-141',
				container: {
					id: '5ab66da4-12dd-4b79-bb8a-bab68bc7a7ac',
				},
				alias: 'labelLongString',
				name: 'Label - Long String',
				description: null,
				dataType: {
					id: 'b76769c6-9662-4848-afce-2c06a7464bf8',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 6,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: '5ab66da4-12dd-4b79-bb8a-bab68bc7a7ac',
				parent: null,
				name: 'Labels',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: '2a773487-9de7-403c-9207-54f4ace7f215',
		alias: 'imageCropper',
		name: 'Image Cropper',
		description: null,
		icon: 'icon-crop color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-142',
				container: {
					id: 'e8964451-7d12-407d-a963-cdbfee0eba35',
				},
				alias: 'imageCropperWithoutCrops',
				name: 'Image Cropper -  Without Crops',
				description: null,
				dataType: {
					id: 'eae1607c-15e9-47f3-bc03-88a9480dcee6',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-143',
				container: {
					id: 'e8964451-7d12-407d-a963-cdbfee0eba35',
				},
				alias: 'imageCropperWithCrops',
				name: 'Image Cropper - With Crops',
				description: null,
				dataType: {
					id: '444a843c-93fa-4589-9e66-949cca3f0e84',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 1,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: 'e8964451-7d12-407d-a963-cdbfee0eba35',
				parent: null,
				name: 'Image Croppers',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: '61c6b912-8fe8-4e10-a07b-4f777b99489b',
		alias: 'blockListDocType',
		name: 'Block List',
		description: null,
		icon: 'icon-bulleted-list color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-146',
				container: {
					id: '7e89bad4-a030-4878-a293-bae51cccb74e',
				},
				alias: 'blockListDefaultConfig',
				name: 'Block List - Default Config',
				description: '',
				dataType: {
					id: 'f955664b-9ab0-4f76-b9d6-5742c44a073c',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-147',
				container: {
					id: '7e89bad4-a030-4878-a293-bae51cccb74e',
				},
				alias: 'blockListMinAndMax',
				name: 'Block List - Min And Max',
				description: null,
				dataType: {
					id: '44aae492-649c-4895-8c0a-01d29adf995f',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 1,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-148',
				container: {
					id: '7e89bad4-a030-4878-a293-bae51cccb74e',
				},
				alias: 'blockListSingleTypeOnly',
				name: 'Block List - Single Type Only',
				description: null,
				dataType: {
					id: '135b9faf-a464-4e5a-a02f-6d17b1d807cf',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 2,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: '7e89bad4-a030-4878-a293-bae51cccb74e',
				parent: null,
				name: 'Block Lists',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: '1addd0ad-0e34-4386-801b-79cf7beb8cf1',
		alias: 'blockGrid',
		name: 'Block Grid',
		description: null,
		icon: 'icon-grid color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-149',
				container: {
					id: 'b136c5af-636d-4625-a301-6a1e04ba548d',
				},
				alias: 'blockGridDefaultConfig',
				name: 'Block Grid - Default Config',
				description: null,
				dataType: {
					id: '5555758f-d1c7-4480-a8ee-391fba1ac2ca',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-150',
				container: {
					id: 'b136c5af-636d-4625-a301-6a1e04ba548d',
				},
				alias: 'blockGridWithAreas',
				name: 'Block Grid - With Areas',
				description: null,
				dataType: {
					id: '45ea7c93-cb3e-44d8-ada3-4382dfdc69f1',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 1,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: 'b136c5af-636d-4625-a301-6a1e04ba548d',
				parent: null,
				name: 'Block Grids',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: '11b48beb-3fd0-4b72-800e-364f6e833dc7',
		alias: 'fileUpload',
		name: 'File Upload',
		description: null,
		icon: 'icon-download-alt color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-151',
				container: {
					id: '9849ef3a-73a1-46c4-b444-e4493f02b3c5',
				},
				alias: 'fileUploadDefaultConfig',
				name: 'File Upload - Default Config',
				description: null,
				dataType: {
					id: '2ece7647-e59f-44d1-952e-c28cf763a375',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'pt-152',
				container: {
					id: '9849ef3a-73a1-46c4-b444-e4493f02b3c5',
				},
				alias: 'fileUploadSpecificFileTypes',
				name: 'File Upload - Specific File Types',
				description: null,
				dataType: {
					id: '9510982e-4e51-48d7-bf37-13a189d72658',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 1,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: '9849ef3a-73a1-46c4-b444-e4493f02b3c5',
				parent: null,
				name: 'File Uploads',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: '9309d592-ebc7-4f72-a1bd-ebdabca4c643',
		alias: 'blockSingle',
		name: 'Block Single',
		description: null,
		icon: 'icon-shape-square color-green',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: {
			id: '25b36f28-5051-4073-a0c7-3887f6f8c695',
		},
		isFolder: false,
		properties: [
			{
				id: 'pt-154',
				container: {
					id: '5e91cb5e-6529-4ef5-85d3-08c5ce15f9a9',
				},
				alias: 'blockSingleDefaultConfig',
				name: 'Block Single - Default Config',
				description: '',
				dataType: {
					id: 'c4a55684-0a65-46b5-b990-87125b969fc0',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [
			{
				id: '5e91cb5e-6529-4ef5-85d3-08c5ce15f9a9',
				parent: null,
				name: 'Block Single',
				type: 'Tab',
				sortOrder: 0,
			},
		],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
];

export const data: Array<UmbMockDocumentTypeModel> = rawData.map((dt) => ({
	...dt,
	compositions: dt.compositions.map((c) => ({
		...c,
		compositionType: mapCompositionType(c.compositionType),
	})),
}));
