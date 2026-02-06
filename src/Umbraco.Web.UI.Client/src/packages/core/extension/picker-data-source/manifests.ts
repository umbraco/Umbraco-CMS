import { UMB_EXTENSION_DATA_SOURCE_TYPE } from '../constants.js';
import { UMB_EXTENSION_PICKER_DATA_SOURCE_ALIAS } from './constants.js';
import type { ManifestDataSource } from '@umbraco-cms/backoffice/data-source';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'dataSource',
		alias: UMB_EXTENSION_PICKER_DATA_SOURCE_ALIAS,
		name: 'Extension Picker Data Source',
		api: () => import('./extension.picker-data-source.js'),
		dataSourceType: UMB_EXTENSION_DATA_SOURCE_TYPE,
		meta: {
			label: 'Extensions',
			description: 'Pick from all registered extensions',
			icon: 'icon-plugin',
		},
	} satisfies ManifestDataSource,
];
