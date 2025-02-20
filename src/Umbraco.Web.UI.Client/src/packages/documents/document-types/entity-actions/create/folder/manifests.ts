import { UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE } from '../../../entity.js';
import {
	UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE,
	UMB_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS,
} from '../../../tree/constants.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'entityCreateOptionAction',
		kind: 'folder',
		alias: 'Umb.EntityCreateOptionAction.DocumentType.Folder',
		name: 'Document Type Folder Entity Create Option Action',
		forEntityTypes: [UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE, UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-folder',
			label: '#create_folder',
			description: '#create_folderDescription',
			folderRepositoryAlias: UMB_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS,
		},
	},
];
