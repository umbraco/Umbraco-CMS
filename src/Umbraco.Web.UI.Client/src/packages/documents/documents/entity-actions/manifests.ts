import { UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS } from '../repository/index.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import { manifests as createManifests } from './create/manifests.js';
import { manifests as publicAccessManifests } from './public-access/manifests.js';
import { manifests as cultureAndHostnamesManifests } from './culture-and-hostnames/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_DOCUMENT_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';

const entityActions: Array<ManifestTypes> = [
	...createManifests,
	...publicAccessManifests,
	...cultureAndHostnamesManifests,
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Document.CreateBlueprint',
		name: 'Create Document Blueprint Entity Action',
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
		meta: {
			moveRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			pickerModelAlias: UMB_DOCUMENT_PICKER_MODAL.toString(),
		},
	},
	{
		type: 'entityAction',
		kind: 'duplicate',
		alias: 'Umb.EntityAction.Document.Duplicate',
		name: 'Duplicate Document Entity Action',
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			duplicateRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			pickerModalAlias: UMB_DOCUMENT_PICKER_MODAL.toString(),
		},
	},
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Document.Publish',
		name: 'Publish Document Entity Action',
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
		api: () => import('./unpublish.action.js'),
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			icon: 'icon-globe',
			label: 'Unpublish',
		},
	},
];

export const manifests = [...entityActions];
