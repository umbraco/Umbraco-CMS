import { UmbDeleteEntityAction } from '../../../../backoffice/shared/entity-actions/delete/delete.action';
import { UmbMoveEntityAction } from '../../../../backoffice/shared/entity-actions/move/move.action';
import { MEDIA_TYPE_REPOSITORY_ALIAS } from '../repository/manifests';
import { UmbCopyEntityAction } from '../../../../backoffice/shared/entity-actions/copy/copy.action';
import { UmbCreateMediaTypeEntityAction } from './create.action';
import UmbReloadMediaTypeEntityAction from './reload.action';
import type { ManifestEntityAction } from '@umbraco-cms/models';

const entityType = 'media-type';
const repositoryAlias = MEDIA_TYPE_REPOSITORY_ALIAS;

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.MediaType.Create',
		name: 'Create Media Type Entity Action',
		weight: 500,
		meta: {
			entityType,
			icon: 'umb:add',
			label: 'Create',
			repositoryAlias,
			api: UmbCreateMediaTypeEntityAction,
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.MediaType.Move',
		name: 'Move Media Type Entity Action',
		weight: 400,
		meta: {
			entityType,
			icon: 'umb:enter',
			label: 'Move',
			repositoryAlias,
			api: UmbMoveEntityAction,
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.MediaType.Copy',
		name: 'Copy Media Type Entity Action',
		weight: 300,
		meta: {
			entityType,
			icon: 'umb:documents',
			label: 'Copy',
			repositoryAlias,
			api: UmbCopyEntityAction,
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.MediaType.Delete',
		name: 'Delete Media Type Entity Action',
		weight: 200,
		meta: {
			entityType,
			icon: 'umb:trash',
			label: 'Delete',
			repositoryAlias,
			api: UmbDeleteEntityAction,
		},
	},
    {
		type: 'entityAction',
		alias: 'Umb.EntityAction.MediaType.Reload',
		name: 'Reload Media Type Entity Action',
		weight: 100,
		meta: {
			entityType,
			icon: 'umb:refresh',
			label: 'Reload',
			repositoryAlias,
			api: UmbReloadMediaTypeEntityAction,
		},
	},
];

export const manifests = [...entityActions];
