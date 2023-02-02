import { TrashMediaEntityAction } from './trash-media.entity-action';
import { ManifestEntityAction } from 'libs/extensions-registry/entity-action.models';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Media.Trash',
		name: 'Trash Media Entity Action ',
		meta: {
			entityType: 'media',
			icon: 'umb:trash',
			label: 'Trash',
			api: TrashMediaEntityAction,
		},
	},
];

export const manifests = [...entityActions];
