import {
	UMB_DOCUMENT_DYNAMIC_ROOT_ORIGIN_PICKER_MODAL_ALIAS,
	UMB_DOCUMENT_DYNAMIC_ROOT_QUERY_STEP_PICKER_MODAL_ALIAS,
} from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: UMB_DOCUMENT_DYNAMIC_ROOT_ORIGIN_PICKER_MODAL_ALIAS,
		name: 'Choose an origin',
		element: () => import('./document-dynamic-root-origin-picker-modal.element.js'),
	},
	{
		type: 'modal',
		alias: UMB_DOCUMENT_DYNAMIC_ROOT_QUERY_STEP_PICKER_MODAL_ALIAS,
		name: 'Append step to query',
		element: () => import('./document-dynamic-root-query-step-picker-modal.element.js'),
	},
];
