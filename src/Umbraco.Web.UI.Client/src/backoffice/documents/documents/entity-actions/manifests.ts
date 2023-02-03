import { UmbCreateDocumentEntityAction } from './create.action';
import { UmbPublishDocumentEntityAction } from './publish.action';
import { UmbSaveDocumentWorkspaceAction } from '../workspace/actions/save.action';
import { UmbCopyDocumentEntityAction } from './copy.action';
import { UmbDocumentCultureAndHostnamesEntityAction } from './culture-and-hostnames.action';
import { UmbMoveDocumentEntityAction } from './move.action';
import { UmbTrashDocumentEntityAction } from './trash.action';
import { UmbSortChildrenOfDocumentEntityAction } from './sort-children-of.action';
import { UmbCreateDocumentBlueprintEntityAction } from './create-blueprint.action';
import { UmbDocumentPublicAccessEntityAction } from './public-access.action';
import { UmbDocumentPermissionsEntityAction } from './permissions.action';
import { UmbSaveAndPublishDocumentWorkspaceAction } from '../workspace/actions/save-and-publish.action';
import { UmbUnpublishDocumentEntityAction } from './unpublish.action';
import { UmbSaveAndPreviewDocumentWorkspaceAction } from '../workspace/actions/save-and-preview.action';
import { UmbSaveAndScheduleDocumentWorkspaceAction } from '../workspace/actions/save-and-schedule.action';
import { UmbRollbackDocumentEntityAction } from './rollback.action';
import { ManifestEntityAction } from '@umbraco-cms/extensions-registry';

const entityType = 'document';

/* TODO: This is a temporary solution to get the entity actions working.
 Some actions will only work in the tree (sort), others will only work in a workspace (Save, Save and Publish, etc.).
 we will ned a way to filter in the manifest. Either on type (treeEntityAction, workspaceEntityAction) or on meta (tree, workspace).*/
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
			icon: 'umb:document',
			label: 'Trash',
			api: UmbTrashDocumentEntityAction,
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
			api: UmbMoveDocumentEntityAction,
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
			api: UmbCopyDocumentEntityAction,
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
			api: UmbSortChildrenOfDocumentEntityAction,
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
			api: UmbRollbackDocumentEntityAction,
		},
	},
];

export const manifests = [...entityActions];
