import { UMB_ELEMENT_FOLDER_ENTITY_TYPE, UMB_ELEMENT_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS } from '../repository/constants.js';
import {
	UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
	UMB_USER_PERMISSION_ELEMENT_DELETE,
} from '../../user-permissions/constants.js';
import { manifests as moveManifests } from './move/manifests.js';
import {
	UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
	UMB_ENTITY_IS_TRASHED_CONDITION_ALIAS,
} from '@umbraco-cms/backoffice/recycle-bin';
import type { ManifestEntityCreateOptionActionFolderKind } from '@umbraco-cms/backoffice/tree';

const folderCreateOption: ManifestEntityCreateOptionActionFolderKind = {
	type: 'entityCreateOptionAction',
	kind: 'folder',
	alias: 'Umb.EntityCreateOptionAction.Element.Folder',
	name: 'Element Folder Entity Create Option Action',
	forEntityTypes: [UMB_ELEMENT_ROOT_ENTITY_TYPE, UMB_ELEMENT_FOLDER_ENTITY_TYPE],
	meta: {
		icon: 'icon-folder',
		label: '#create_folder',
		additionalOptions: true,
		folderRepositoryAlias: UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS,
	},
};

const folderDelete: UmbExtensionManifest = {
	type: 'entityAction',
	kind: 'folderDelete',
	alias: 'Umb.EntityAction.Element.Folder.Delete',
	name: 'Delete Element Folder Entity Action',
	forEntityTypes: [UMB_ELEMENT_FOLDER_ENTITY_TYPE],
	meta: {
		icon: 'icon-trash-empty',
		folderRepositoryAlias: UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS, // TODO: [LK] This needs to call the recycle-bin repository instead.
	},
	conditions: [
		{
			alias: UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
			allOf: [UMB_USER_PERMISSION_ELEMENT_DELETE],
		},
		{ alias: UMB_ENTITY_IS_TRASHED_CONDITION_ALIAS },
	],
};

const folderUpdate: UmbExtensionManifest = {
	type: 'entityAction',
	kind: 'folderUpdate',
	alias: 'Umb.EntityAction.Element.Folder.Rename',
	name: 'Rename Element Folder Entity Action',
	forEntityTypes: [UMB_ELEMENT_FOLDER_ENTITY_TYPE],
	meta: {
		folderRepositoryAlias: UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS,
	},
	conditions: [{ alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS }],
};

export const manifests: Array<UmbExtensionManifest> = [
	folderCreateOption,
	folderDelete,
	folderUpdate,
	...moveManifests,
];
