import { MEDIA_TYPE_DETAIL_REPOSITORY_ALIAS } from '../index.js';
import { UmbCreateMediaTypeEntityAction } from './create/create.action.js';
import { manifests as createManifests } from './create/manifests.js';
import { UmbDeleteEntityAction, UmbMoveEntityAction, UmbCopyEntityAction } from '@umbraco-cms/backoffice/entity-action';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

const entityType = 'media-type';
const repositoryAlias = MEDIA_TYPE_DETAIL_REPOSITORY_ALIAS;

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.MediaType.Create',
		name: 'Create Media Type Entity Action',
		weight: 500,
		api: UmbCreateMediaTypeEntityAction,
		meta: {
			icon: 'icon-add',
			label: 'Create',
			repositoryAlias,
			entityTypes: [entityType],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.MediaType.Move',
		name: 'Move Media Type Entity Action',
		weight: 400,
		api: UmbMoveEntityAction,
		meta: {
			icon: 'icon-enter',
			label: 'Move',
			repositoryAlias,
			entityTypes: [entityType],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.MediaType.Copy',
		name: 'Copy Media Type Entity Action',
		weight: 300,
		api: UmbCopyEntityAction,
		meta: {
			icon: 'icon-documents',
			label: 'Copy',
			repositoryAlias,
			entityTypes: [entityType],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.MediaType.Delete',
		name: 'Delete Media Type Entity Action',
		weight: 200,
		api: UmbDeleteEntityAction,
		meta: {
			icon: 'icon-trash',
			label: 'Delete',
			repositoryAlias,
			entityTypes: [entityType],
		},
	},
];

export const manifests = [...entityActions, ...createManifests];
