import {
	UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE,
	UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE,
} from '../../../entity.js';
import { UMB_DOCUMENT_BLUEPRINT_FOLDER_REPOSITORY_ALIAS } from '../../../tree/folder/constants.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'entityCreateOptionAction',
		kind: 'folder',
		alias: 'Umb.EntityCreateOptionAction.DocumentBlueprint.Folder',
		name: 'Document Blueprint Folder Entity Create Option Action',
		forEntityTypes: [UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE, UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-folder',
			label: '#create_folder',
			additionalOptions: true,
			folderRepositoryAlias: UMB_DOCUMENT_BLUEPRINT_FOLDER_REPOSITORY_ALIAS,
		},
	},
];
