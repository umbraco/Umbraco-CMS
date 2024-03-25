import { UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS } from '../repository/index.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import { UMB_DOCUMENT_PICKER_MODAL } from '../modals/index.js';
import { manifests as createManifests } from './create/manifests.js';
import { manifests as publicAccessManifests } from './public-access/manifests.js';
import { manifests as cultureAndHostnamesManifests } from './culture-and-hostnames/manifests.js';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		kind: 'delete',
		alias: 'Umb.EntityAction.Document.Delete',
		name: 'Delete Document Entity Action',
		weight: 1100,
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			deleteRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			pickerModalAlias: UMB_DOCUMENT_PICKER_MODAL,
		},
	},
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Document.CreateBlueprint',
		name: 'Create Document Blueprint Entity Action',
		weight: 1000,
		api: () => import('./create-blueprint.action.js'),
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			icon: 'icon-blueprint',
			label: 'Create Document Blueprint (TBD)',
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Move',
		name: 'Move Document Entity Action ',
		kind: 'move',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		weight: 900,
		meta: {
			moveRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			pickerModelAlias: UMB_DOCUMENT_PICKER_MODAL,
		},
	},
	{
		type: 'entityAction',
		kind: 'duplicate',
		alias: 'Umb.EntityAction.Document.Duplicate',
		name: 'Duplicate Document Entity Action',
		weight: 800,
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			duplicateRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			pickerModal: UMB_DOCUMENT_PICKER_MODAL,
		},
	},
	{
		type: 'entityAction',
		kind: 'sort',
		alias: 'Umb.EntityAction.Document.Sort',
		name: 'Sort Document Entity Action',
		weight: 700,
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {},
	},
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Document.Publish',
		name: 'Publish Document Entity Action',
		weight: 600,
		api: () => import('./publish.action.js'),
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			icon: 'icon-globe',
			label: 'Publish',
		},
	},
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Document.Unpublish',
		name: 'Unpublish Document Entity Action',
		weight: 500,
		api: () => import('./unpublish.action.js'),
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			icon: 'icon-globe',
			label: 'Unpublish...',
		},
	},
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Document.Permissions',
		name: 'Permissions Document Entity Action',
		weight: 300,
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		api: () => import('./permissions.action.js'),
		meta: {
			icon: 'icon-name-badge',
			label: 'Permissions...',
		},
	},
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Document.Notifications',
		name: 'Notifications Document Entity Action',
		weight: 100,
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		api: () => import('./permissions.action.js'),
		meta: {
			icon: 'icon-megaphone',
			label: 'Notifications...',
		},
	},
];

export const manifests = [
	...createManifests,
	...publicAccessManifests,
	...cultureAndHostnamesManifests,
	...entityActions,
];
