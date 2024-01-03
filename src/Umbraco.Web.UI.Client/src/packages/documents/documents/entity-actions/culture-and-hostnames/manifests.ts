import { UMB_DOCUMENT_REPOSITORY_ALIAS } from '../../repository/manifests.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UmbDocumentCultureAndHostnamesEntityAction } from './culture-and-hostnames.action.js';
import { ManifestEntityAction, ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.CultureAndHostnames',
		name: 'Culture And Hostnames Document Entity Action',
		weight: 400,
		api: UmbDocumentCultureAndHostnamesEntityAction,
		meta: {
			icon: 'icon-home',
			label: 'Culture and Hostnames',
			repositoryAlias: UMB_DOCUMENT_REPOSITORY_ALIAS,
			entityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		},
	},
];

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.CultureAndHostnames',
		name: 'Create Document Modal',
		js: () => import('./culture-and-hostnames-modal.element.js'),
	},
];

export const manifests = [...entityActions, ...modals];
