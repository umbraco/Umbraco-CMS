import { manifest as schemaManifest } from './Umbraco.MultipleMemberPicker.js';
import { manifests as valueSummaryManifests } from './value-summary/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.MultipleMemberPicker',
		name: 'Multiple Member Picker Property Editor UI',
		element: () => import('./property-editor-ui-multiple-member-picker.element.js'),
		meta: {
			label: 'Multiple Member Picker',
			propertyEditorSchemaAlias: 'Umbraco.MultipleMemberPicker',
			icon: 'icon-users',
			group: '#propertyEditorUIGroups_people',
			keywords: [
				'select',
				'member',
				'person',
				'subscriber',
				'customer',
				'account',
				'profile',
				'contact',
				'author',
				'multiple',
				'several',
			],
			supportsReadOnly: true,
		},
	},
	schemaManifest,
	...valueSummaryManifests,
];
