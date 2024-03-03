import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UmbDocumentCultureAndHostnamesEntityAction } from './culture-and-hostnames.action.js';
import type { ManifestModal, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.CultureAndHostnames',
		name: 'Culture And Hostnames Document Entity Action',
		weight: 400,
		api: UmbDocumentCultureAndHostnamesEntityAction,
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			icon: 'icon-home',
			label: 'Culture and Hostnames',
		},
	},
];

const manifestModals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.CultureAndHostnames',
		name: 'Culture And Hostnames Modal',
		js: () => import('./modal/culture-and-hostnames-modal.element.js'),
	},
];

export const manifests = [...entityActions, ...manifestModals];
