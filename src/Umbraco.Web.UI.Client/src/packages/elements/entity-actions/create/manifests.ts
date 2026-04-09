import { UMB_ELEMENT_FOLDER_ENTITY_TYPE, UMB_ELEMENT_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS } from '../../folder/constants.js';
import {
	UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
	UMB_USER_PERMISSION_ELEMENT_CREATE,
} from '../../user-permissions/constants.js';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/entity-action';
import type { ManifestEntityCreateOptionAction } from '@umbraco-cms/backoffice/entity-create-option-action';
import type { ManifestEntityCreateOptionActionFolderKind } from '@umbraco-cms/backoffice/tree';

const createEntityAction: ManifestEntityAction = {
	type: 'entityAction',
	kind: 'create',
	alias: 'Umb.EntityAction.Element.Create',
	name: 'Create Element Entity Action',
	weight: 1200,
	forEntityTypes: [UMB_ELEMENT_ROOT_ENTITY_TYPE, UMB_ELEMENT_FOLDER_ENTITY_TYPE],
	meta: {
		icon: 'icon-add',
		label: '#actions_createFor',
		additionalOptions: true,
		headline: '#create_createUnder #treeHeaders_elements',
	},
	conditions: [
		{
			alias: UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
			allOf: [UMB_USER_PERMISSION_ELEMENT_CREATE],
		},
		{
			alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
		},
	],
};

const elementCreateOptionAction: ManifestEntityCreateOptionAction = {
	type: 'entityCreateOptionAction',
	alias: 'Umb.EntityCreateOptionAction.Element.Default',
	name: 'Default Element Entity Create Option Action',
	weight: 100,
	api: () => import('./element-create-option-action.js'),
	forEntityTypes: [UMB_ELEMENT_ROOT_ENTITY_TYPE, UMB_ELEMENT_FOLDER_ENTITY_TYPE],
	meta: {
		icon: 'icon-document',
		label: '#create_element',
		description: '#create_elementDescription',
	},
};

const folderCreateOptionAction: ManifestEntityCreateOptionActionFolderKind = {
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

export const manifests: Array<UmbExtensionManifest> = [
	createEntityAction,
	elementCreateOptionAction,
	folderCreateOptionAction,
];
