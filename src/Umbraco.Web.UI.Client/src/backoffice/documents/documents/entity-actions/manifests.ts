import { CreateDocumentEntityAction } from './create-document.entity-action';
import { TrashDocumentEntityAction } from './trash-document.entity-action';
import { PublishDocumentEntityAction } from './publish-document.entity-action';
import { SaveDocumentEntityAction } from './save-document.entity-action';
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
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Save',
		name: 'Save Document Entity Action ',
		meta: {
			entityType: 'document',
			icon: 'umb:document',
			label: 'Save',
			api: SaveDocumentEntityAction,
		},
	},
];

export const manifests = [...entityActions];
