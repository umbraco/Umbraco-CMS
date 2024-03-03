import { UMB_DOCUMENT_ENTITY_TYPE, UMB_DOCUMENT_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UmbCreateDocumentEntityAction } from './create.action.js';
import type { ManifestModal, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Create',
		name: 'Create Document Entity Action',
		weight: 1000,
		api: UmbCreateDocumentEntityAction,
		forEntityTypes: [UMB_DOCUMENT_ROOT_ENTITY_TYPE, UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: 'Create',
		},
		/* removed until we have permissions in place
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission',
				// TODO: investigate why the match property is not typed
				// eslint-disable-next-line @typescript-eslint/ban-ts-comment
				//@ts-ignore
				match: 'Umb.UserPermission.Document.Create',
			},
		],
		*/
	},
];

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Document.CreateOptions',
		name: 'Document Create Options Modal',
		js: () => import('./document-create-options-modal.element.js'),
	},
];

export const manifests = [...entityActions, ...modals];
