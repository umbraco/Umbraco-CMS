import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as itemManifests } from './item/manifests.js';
import { manifests as pickerModalManifests } from './picker-modal/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...collectionManifests,
	...itemManifests,
	...pickerModalManifests,
	{
		type: 'propertyEditorDataSource',
		alias: 'Umb.PropertyEditor.DataSource.Test1',
		name: 'Test Data Source 1',
		api: () => import('./test-1.js'),
	},
	{
		type: 'propertyEditorDataSource',
		alias: 'Umb.PropertyEditor.DataSource.Test2',
		name: 'Test Data Source 2',
		api: () => import('./test-1.js'),
	},
	{
		type: 'propertyEditorDataSource',
		alias: 'Umb.PropertyEditor.DataSource.Test3',
		name: 'Test Data Source 3',
		api: () => import('./test-1.js'),
	},
	{
		type: 'propertyEditorDataSource',
		alias: 'Umb.PropertyEditor.DataSource.Test4',
		name: 'Test Data Source 4',
		api: () => import('./test-1.js'),
	},
];
