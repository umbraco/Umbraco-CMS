import { UMB_PICKER_DATA_SOURCE_TYPE } from '@umbraco-cms/backoffice/picker-data-source';
import type { ManifestPropertyEditorDataSource } from '@umbraco-cms/backoffice/property-editor-data-source';

const dataSource: ManifestPropertyEditorDataSource = {
	type: 'propertyEditorDataSource',
	alias: 'Umb.PropertyEditorDataSource.Element',
	dataSourceType: UMB_PICKER_DATA_SOURCE_TYPE,
	name: 'Element Property Data Source',
	api: () => import('./element-tree-data-source.js'),
	meta: {
		label: 'Elements',
		description: 'Umbraco Elements data source for property editors.',
		icon: 'icon-page-add',
	},
};

export const manifests: Array<UmbExtensionManifest> = [dataSource];
