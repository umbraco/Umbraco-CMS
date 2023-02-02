import { CreateDocumentEntityAction } from './document-create.entity-action';
import { TrashDocumentEntityAction } from './document-trash.entity-action';
import { PublishDocumentEntityAction } from './document-publish.entity-action';
import { ManifestEntityAction } from 'libs/extensions-registry/entity-action.models';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Create',
		name: 'Create Document Entity Action ',
		meta: {
			entityType: 'document',
			icon: 'umb:add',
			label: 'Create',
			api: CreateDocumentEntityAction,
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Trash',
		name: 'Trash Document Entity Action ',
		meta: {
			entityType: 'document',
			icon: 'umb:trash',
			label: 'Trash',
			api: TrashDocumentEntityAction,
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Publish',
		name: 'Publish Document Entity Action ',
		meta: {
			entityType: 'document',
			icon: 'umb:document',
			label: 'Publish',
			api: PublishDocumentEntityAction,
		},
	},
];

export const manifests = [...entityActions];
