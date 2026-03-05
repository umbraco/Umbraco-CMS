import { UMB_ELEMENT_DETAIL_REPOSITORY_ALIAS } from '../../repository/detail/constants.js';
import { UMB_ELEMENT_ENTITY_TYPE, UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS } from '../../folder/repository/constants.js';
import { UMB_ELEMENT_ITEM_REPOSITORY_ALIAS } from '../../item/constants.js';
import {
	UMB_ELEMENT_FOLDER_RECYCLE_BIN_REPOSITORY_ALIAS,
	UMB_ELEMENT_RECYCLE_BIN_REPOSITORY_ALIAS,
	UMB_ELEMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE,
} from '../constants.js';
import { UMB_ELEMENT_REFERENCE_REPOSITORY_ALIAS } from '../../reference/constants.js';
import {
	UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
	UMB_USER_PERMISSION_ELEMENT_DELETE,
} from '../../user-permissions/constants.js';
import { UMB_ENTITY_HAS_CHILDREN_CONDITION_ALIAS } from '@umbraco-cms/backoffice/entity-action';
import {
	UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
	UMB_ENTITY_IS_TRASHED_CONDITION_ALIAS,
} from '@umbraco-cms/backoffice/recycle-bin';

const elementActions: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'trashWithRelation',
		alias: 'Umb.EntityAction.Element.RecycleBin.Trash',
		name: 'Trash Element Entity Action',
		forEntityTypes: [UMB_ELEMENT_ENTITY_TYPE],
		meta: {
			itemRepositoryAlias: UMB_ELEMENT_ITEM_REPOSITORY_ALIAS,
			recycleBinRepositoryAlias: UMB_ELEMENT_RECYCLE_BIN_REPOSITORY_ALIAS,
			referenceRepositoryAlias: UMB_ELEMENT_REFERENCE_REPOSITORY_ALIAS,
		},

		conditions: [
			{
				alias: UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
				allOf: [UMB_USER_PERMISSION_ELEMENT_DELETE],
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
	{
		type: 'entityAction',
		kind: 'deleteWithRelation',
		alias: 'Umb.EntityAction.Element.Delete',
		name: 'Delete Element Entity Action',
		forEntityTypes: [UMB_ELEMENT_ENTITY_TYPE],
		meta: {
			icon: 'icon-trash-empty',
			itemRepositoryAlias: UMB_ELEMENT_ITEM_REPOSITORY_ALIAS,
			detailRepositoryAlias: UMB_ELEMENT_DETAIL_REPOSITORY_ALIAS,
			referenceRepositoryAlias: UMB_ELEMENT_REFERENCE_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
				allOf: [UMB_USER_PERMISSION_ELEMENT_DELETE],
			},
			{ alias: UMB_ENTITY_IS_TRASHED_CONDITION_ALIAS },
		],
	},
	// {
	// 	type: 'entityAction',
	// 	kind: 'restoreFromRecycleBin',
	// 	alias: 'Umb.EntityAction.Element.RecycleBin.Restore',
	// 	name: 'Restore Element From Recycle Bin Entity Action',
	// 	forEntityTypes: [UMB_ELEMENT_ENTITY_TYPE],
	// 	meta: {
	// 		itemRepositoryAlias: UMB_ELEMENT_ITEM_REPOSITORY_ALIAS,
	// 		itemDataResolver: UmbElementItemDataResolver,
	// 		recycleBinRepositoryAlias: UMB_ELEMENT_RECYCLE_BIN_REPOSITORY_ALIAS,
	// 		pickerModal: UMB_ELEMENT_PICKER_MODAL,
	// 	},
	// 	conditions: [
	// 		{
	// 			alias: UMB_ENTITY_IS_TRASHED_CONDITION_ALIAS,
	// 		},
	// 	],
	// },
];

const folderActions: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'trashFolder',
		alias: 'Umb.EntityAction.Element.Folder.Trash',
		name: 'Trash Element Folder Entity Action',
		forEntityTypes: [UMB_ELEMENT_FOLDER_ENTITY_TYPE],
		meta: {
			folderRepositoryAlias: UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS,
			recycleBinRepositoryAlias: UMB_ELEMENT_FOLDER_RECYCLE_BIN_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
				allOf: [UMB_USER_PERMISSION_ELEMENT_DELETE],
			},
			{ alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS },
		],
	},
];

const emptyRecycleBin: UmbExtensionManifest = {
	type: 'entityAction',
	kind: 'emptyRecycleBin',
	alias: 'Umb.EntityAction.Element.RecycleBin.Empty',
	name: 'Empty Element Recycle Bin Entity Action',
	forEntityTypes: [UMB_ELEMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE],
	meta: {
		recycleBinRepositoryAlias: UMB_ELEMENT_RECYCLE_BIN_REPOSITORY_ALIAS,
	},
	conditions: [
		{
			alias: UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
			allOf: [UMB_USER_PERMISSION_ELEMENT_DELETE],
		},
		{ alias: UMB_ENTITY_HAS_CHILDREN_CONDITION_ALIAS },
	],
};

const reloadTreeItemChildren: UmbExtensionManifest = {
	type: 'entityAction',
	kind: 'reloadTreeItemChildren',
	alias: 'Umb.EntityAction.ElementRecycleBin.Tree.ReloadChildrenOf',
	name: 'Reload Element Recycle Bin Tree Item Children Entity Action',
	forEntityTypes: [UMB_ELEMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE],
};

export const manifests: Array<UmbExtensionManifest> = [
	...elementActions,
	...folderActions,
	emptyRecycleBin,
	reloadTreeItemChildren,
];
