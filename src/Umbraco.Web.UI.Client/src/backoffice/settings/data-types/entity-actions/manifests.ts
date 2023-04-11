import { DATA_TYPE_REPOSITORY_ALIAS } from '../repository/manifests';
import { manifests as createManifests } from './create/manifests';
import {
	UmbDeleteEntityAction,
	UmbDeleteFolderEntityAction,
	UmbFolderUpdateEntityAction,
} from '@umbraco-cms/backoffice/entity-action';
import { ManifestEntityAction } from '@umbraco-cms/backoffice/extensions-registry';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DataType.Delete',
		name: 'Delete Data Type Entity Action',
		weight: 900,
		meta: {
			icon: 'umb:trash',
			label: 'Delete...',
			repositoryAlias: DATA_TYPE_REPOSITORY_ALIAS,
			api: UmbDeleteEntityAction,
		},
		conditions: {
			entityType: 'data-type',
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DataType.DeleteFolder',
		name: 'Delete Data Type Folder Entity Action',
		weight: 800,
		meta: {
			icon: 'umb:trash',
			label: 'Delete Folder...',
			repositoryAlias: DATA_TYPE_REPOSITORY_ALIAS,
			api: UmbDeleteFolderEntityAction,
		},
		conditions: {
			entityType: 'data-type',
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DataType.RenameFolder',
		name: 'Rename Data Type Folder Entity Action',
		weight: 700,
		meta: {
			icon: 'umb:edit',
			label: 'Rename Folder...',
			repositoryAlias: DATA_TYPE_REPOSITORY_ALIAS,
			api: UmbFolderUpdateEntityAction,
		},
		conditions: {
			entityType: 'data-type',
		},
	},
];

export const manifests = [...entityActions, ...createManifests];
