import { UMB_SCRIPT_FOLDER_ENTITY_TYPE, UMB_SCRIPT_ROOT_ENTITY_TYPE } from '../../../entity.js';
import { UMB_SCRIPT_FOLDER_REPOSITORY_ALIAS } from '../../../tree/folder/repository/constants.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'entityCreateOptionAction',
		kind: 'folder',
		alias: 'Umb.EntityCreateOptionAction.Script.Folder',
		name: 'Script Folder Entity Create Option Action',
		forEntityTypes: [UMB_SCRIPT_ROOT_ENTITY_TYPE, UMB_SCRIPT_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-folder',
			label: '#create_folder',
			additionalOptions: true,
			folderRepositoryAlias: UMB_SCRIPT_FOLDER_REPOSITORY_ALIAS,
		},
	},
];
