import { UMB_MEMBER_TYPE_FOLDER_ENTITY_TYPE, UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE } from '../../../entity.js';
import { UMB_MEMBER_TYPE_FOLDER_REPOSITORY_ALIAS } from '../../../tree/folder/constants.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'entityCreateOptionAction',
		kind: 'folder',
		alias: 'Umb.EntityCreateOptionAction.MemberType.Folder',
		name: 'Member Type Folder Entity Create Option Action',
		forEntityTypes: [UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE, UMB_MEMBER_TYPE_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-folder',
			label: '#create_folder',
			additionalOptions: true,
			folderRepositoryAlias: UMB_MEMBER_TYPE_FOLDER_REPOSITORY_ALIAS,
		},
	},
];
