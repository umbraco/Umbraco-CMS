import { MEDIA_TYPE_REPOSITORY_ALIAS } from '../repository/manifests.js';
import { UmbCreateMediaTypeEntityAction } from './create.action.js';
import UmbReloadMediaTypeEntityAction from './reload.action.js';
import { UmbDeleteEntityAction, UmbMoveEntityAction, UmbCopyEntityAction } from '@umbraco-cms/backoffice/entity-action';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

const entityType = 'media-type';
const repositoryAlias = MEDIA_TYPE_REPOSITORY_ALIAS;

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.MediaType.Create',
		name: 'Create Media Type Entity Action',
		weight: 500,
		meta: {
			icon: 'umb:add',
			label: 'Create',
			repositoryAlias,
			api: UmbCreateMediaTypeEntityAction,
		},
		conditions: {
			entityTypes: [entityType],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.MediaType.Move',
		name: 'Move Media Type Entity Action',
		weight: 400,
		meta: {
			icon: 'umb:enter',
			label: 'Move',
			repositoryAlias,
			api: UmbMoveEntityAction,
		},
		conditions: {
			entityTypes: [entityType],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.MediaType.Copy',
		name: 'Copy Media Type Entity Action',
		weight: 300,
		meta: {
			icon: 'umb:documents',
			label: 'Copy',
			repositoryAlias,
			api: UmbCopyEntityAction,
		},
		conditions: {
			entityTypes: [entityType],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.MediaType.Delete',
		name: 'Delete Media Type Entity Action',
		weight: 200,
		meta: {
			icon: 'umb:trash',
			label: 'Delete',
			repositoryAlias,
			api: UmbDeleteEntityAction,
		},
		conditions: {
			entityTypes: [entityType],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.MediaType.Reload',
		name: 'Reload Media Type Entity Action',
		weight: 100,
		meta: {
			icon: 'umb:refresh',
			label: 'Reload',
			repositoryAlias,
			api: UmbReloadMediaTypeEntityAction,
		},
		conditions: {
			entityTypes: [entityType],
		},
	},
];

export const manifests = [...entityActions];
