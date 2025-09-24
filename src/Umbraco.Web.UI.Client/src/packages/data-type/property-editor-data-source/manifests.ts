import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as itemManifests } from './item/manifests.js';
import { manifests as pickerModalManifests } from './picker-modal/manifests.js';
import { manifests as searchManifests } from './search/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...collectionManifests,
	...itemManifests,
	...pickerModalManifests,
	...searchManifests,
	{
		type: 'propertyEditorDataSource',
		alias: 'Umb.PropertyEditor.DataSource.Test1',
		name: 'Test Data Source 1',
		api: () => import('./test-1.js'),
		meta: {
			label: 'Test Data Source 1',
			icon: 'icon-document',
		},
	},
	{
		type: 'propertyEditorDataSource',
		alias: 'Umb.PropertyEditor.DataSource.Test2',
		name: 'Test Data Source 2',
		api: () => import('./test-1.js'),
		meta: {
			label: 'Test Data Source 2',
			icon: 'icon-wand',
		},
	},
	{
		type: 'propertyEditorDataSource',
		alias: 'Umb.PropertyEditor.DataSource.Test3',
		name: 'Test Data Source 3',
		api: () => import('./test-1.js'),
		meta: {
			label: 'Test Data Source 3',
			icon: 'icon-settings',
		},
	},
	{
		type: 'propertyEditorDataSource',
		alias: 'Umb.PropertyEditor.DataSource.Test4',
		name: 'Test Data Source 4',
		api: () => import('./test-1.js'),
		meta: {
			label: 'Test Data Source 4',
			icon: 'icon-alarm-clock',
		},
	},
];
