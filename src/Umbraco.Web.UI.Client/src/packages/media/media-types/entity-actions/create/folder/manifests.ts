import { UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE, UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE } from '../../../entity.js';
import { UMB_MEDIA_TYPE_FOLDER_REPOSITORY_ALIAS } from '../../../constants.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'entityCreateOptionAction',
		kind: 'folder',
		alias: 'Umb.EntityCreateOptionAction.MediaType.Folder',
		name: 'Media Type Folder Entity Create Option Action',
		forEntityTypes: [UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE, UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-folder',
			label: '#create_folder',
			description: '#create_folderDescription',
			folderRepositoryAlias: UMB_MEDIA_TYPE_FOLDER_REPOSITORY_ALIAS,
		},
	},
];
