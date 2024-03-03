import { UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS } from '../repository/index.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import { UmbPublishDocumentEntityAction } from './publish.action.js';
import { UmbCreateDocumentBlueprintEntityAction } from './create-blueprint.action.js';
import { UmbUnpublishDocumentEntityAction } from './unpublish.action.js';
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
		alias: 'Umb.EntityAction.Document.CreateBlueprint',
		name: 'Create Document Blueprint Entity Action',
		weight: 800,
		api: UmbCreateDocumentBlueprintEntityAction,
		kind: 'default',
		meta: {
			entityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
			icon: 'icon-blueprint',
			label: 'Create Document Blueprint (TBD)',
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Move',
		name: 'Move Document Entity Action ',
		kind: 'move',
		meta: {
			entityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
			moveRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			pickerModelAlias: UMB_DOCUMENT_PICKER_MODAL.toString(),
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Copy',
		name: 'Duplicate Document Entity Action',
		kind: 'duplicate',
		meta: {
			entityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
			duplicateRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			pickerModalAlias: UMB_DOCUMENT_PICKER_MODAL.toString(),
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Publish',
		name: 'Publish Document Entity Action',
		api: UmbPublishDocumentEntityAction,
		kind: 'default',
		meta: {
			entityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
			icon: 'icon-globe',
			label: 'Publish',
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Unpublish',
		name: 'Unpublish Document Entity Action',
		api: UmbUnpublishDocumentEntityAction,
		kind: 'default',
		meta: {
			entityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
			icon: 'icon-globe',
			label: 'Unpublish',
		},
	},
];

export const manifests = [...entityActions];
