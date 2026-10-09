import type { UmbMockDocumentTypeModel } from '../../mock-data-set.types.js';

export const data: Array<UmbMockDocumentTypeModel> = [
	{
		allowedTemplates: [],
		defaultTemplate: null,
		id: 'the-simplest-document-type-id',
		alias: 'permissionsTestDocumentType',
		name: 'Permissions Test Document Type',
		description: null,
		icon: 'icon-document',
		allowedAsRoot: true,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: null,
		isFolder: false,
		properties: [
			{
				id: '1680d4d2-cda8-4ac2-affd-a69fc10382b1',
				container: null,
				alias: 'text1',
				name: 'Text 1',
				description: null,
				dataType: { id: '0cc0eba1-9960-42c9-bf9b-60e150b429ae' },
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
];
