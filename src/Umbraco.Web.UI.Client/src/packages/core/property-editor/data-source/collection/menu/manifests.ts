import { UMB_PROPERTY_EDITOR_DATA_SOURCE_COLLECTION_REPOSITORY_ALIAS } from '../data/constants.js';
import type { ManifestCollectionMenu } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<ManifestCollectionMenu> = [
	{
		type: 'collectionMenu',
		name: 'Property Editor Data Source Collection Menu',
		alias: 'Umb.CollectionMenu.PropertyEditorDataSource',
		meta: {
			collectionRepositoryAlias: UMB_PROPERTY_EDITOR_DATA_SOURCE_COLLECTION_REPOSITORY_ALIAS,
		},
	},
];
