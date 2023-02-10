import { UmbCopyEntityAction } from '../../../shared/entity-actions/copy/copy.action';
import { UmbMoveEntityAction } from '../../../shared/entity-actions/move/move.action';
import { UmbTrashEntityAction } from '../../../shared/entity-actions/trash/trash.action';
import { UmbSortChildrenOfEntityAction } from '../../../shared/entity-actions/sort-children-of/sort-children-of.action';
import { UmbCreateDocumentEntityAction } from './create/create.action';
import { UmbPublishDocumentEntityAction } from './publish.action';
import { UmbDocumentCultureAndHostnamesEntityAction } from './culture-and-hostnames.action';
import { UmbCreateDocumentBlueprintEntityAction } from './create-blueprint.action';
import { UmbDocumentPublicAccessEntityAction } from './public-access.action';
import { UmbDocumentPermissionsEntityAction } from './permissions.action';
import { UmbUnpublishDocumentEntityAction } from './unpublish.action';
import { UmbRollbackDocumentEntityAction } from './rollback.action';
import { ManifestEntityAction } from '@umbraco-cms/extensions-registry';

const entityType = 'document';
const repositoryAlias = 'Umb.Repository.Documents';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Create',
		name: 'Create Document Entity Action',
		weight: 1000,
		meta: {
			entityType,
			icon: 'umb:add',
			label: 'Create',
			repositoryAlias,
			api: UmbCreateDocumentEntityAction,
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Trash',
		name: 'Trash Document Entity Action',
		weight: 900,
		meta: {
			entityType,
			icon: 'umb:trash',
			label: 'Trash',
			repositoryAlias,
			api: UmbTrashEntityAction,
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.CreateBlueprint',
		name: 'Create Document Blueprint Entity Action',
		weight: 800,
		meta: {
			entityType,
			icon: 'umb:blueprint',
			label: 'Create Content Template',
			repositoryAlias,
			api: UmbCreateDocumentBlueprintEntityAction,
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Move',
		name: 'Move Document Entity Action',
		weight: 700,
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
		alias: 'Umb.EntityAction.Document.Copy',
		name: 'Copy Document Entity Action',
		weight: 600,
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
		alias: 'Umb.EntityAction.Document.Sort',
		name: 'Sort Document Entity Action',
		weight: 500,
		meta: {
			entityType,
			icon: 'umb:navigation-vertical',
			label: 'Sort',
			repositoryAlias,
			api: UmbSortChildrenOfEntityAction,
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.CultureAndHostnames',
		name: 'Culture And Hostnames Document Entity Action',
		weight: 400,
		meta: {
			entityType,
			icon: 'umb:home',
			label: 'Culture And Hostnames',
			repositoryAlias,
			api: UmbDocumentCultureAndHostnamesEntityAction,
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Permissions',
		name: 'Document Permissions Entity Action',
		meta: {
			entityType,
			icon: 'umb:vcard',
			label: 'Permissions',
			repositoryAlias,
			api: UmbDocumentPermissionsEntityAction,
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.PublicAccess',
		name: 'Document Permissions Entity Action',
		meta: {
			entityType,
			icon: 'umb:lock',
			label: 'Public Access',
			repositoryAlias,
			api: UmbDocumentPublicAccessEntityAction,
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Publish',
		name: 'Publish Document Entity Action',
		meta: {
			entityType,
			icon: 'umb:globe',
			label: 'Publish',
			repositoryAlias,
			api: UmbPublishDocumentEntityAction,
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Publish',
		name: 'Publish Document Entity Action',
		meta: {
			entityType,
			icon: 'umb:globe',
			label: 'Publish',
			repositoryAlias,
			api: UmbPublishDocumentEntityAction,
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Unpublish',
		name: 'Unpublish Document Entity Action',
		meta: {
			entityType,
			icon: 'umb:globe',
			label: 'Unpublish',
			repositoryAlias,
			api: UmbUnpublishDocumentEntityAction,
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Rollback',
		name: 'Rollback Document Entity Action',
		meta: {
			entityType,
			icon: 'umb:undo',
			label: 'Rollback',
			repositoryAlias,
			api: UmbRollbackDocumentEntityAction,
		},
	},
];

export const manifests = [...entityActions];
