import {
	UMB_DOCUMENT_DYNAMIC_ROOT_ORIGIN_PICKER_MODAL,
	UMB_DOCUMENT_DYNAMIC_ROOT_ORIGIN_PICKER_MODAL_ALIAS,
	UMB_DOCUMENT_DYNAMIC_ROOT_QUERY_STEP_PICKER_MODAL,
	UMB_DOCUMENT_DYNAMIC_ROOT_QUERY_STEP_PICKER_MODAL_ALIAS,
	UmbDynamicRootRepository,
} from '@umbraco-cms/backoffice/document';

export * from './components/index.js';
export * from './config/source-content/index.js';
export * from './constants.js';
export type * from './types.js';

/**
 * @deprecated Deprecated since v19. Import `UmbDynamicRootRepository` from `@umbraco-cms/backoffice/document` instead. Scheduled for removal in Umbraco 21.
 */
export const UmbContentPickerDynamicRootRepository = UmbDynamicRootRepository;

/**
 * @deprecated Deprecated since v19. Import `UmbDynamicRootRepository` from `@umbraco-cms/backoffice/document` instead. Scheduled for removal in Umbraco 21.
 */
export type UmbContentPickerDynamicRootRepository = UmbDynamicRootRepository;

/**
 * @deprecated Deprecated since v19. Import `UMB_DOCUMENT_DYNAMIC_ROOT_ORIGIN_PICKER_MODAL` from `@umbraco-cms/backoffice/document` instead. Scheduled for removal in Umbraco 21.
 */
export const UMB_CONTENT_PICKER_DOCUMENT_ROOT_ORIGIN_PICKER_MODAL = UMB_DOCUMENT_DYNAMIC_ROOT_ORIGIN_PICKER_MODAL;

/**
 * @deprecated Deprecated since v19. Import `UMB_DOCUMENT_DYNAMIC_ROOT_QUERY_STEP_PICKER_MODAL` from `@umbraco-cms/backoffice/document` instead. Scheduled for removal in Umbraco 21.
 */
export const UMB_CONTENT_PICKER_DOCUMENT_ROOT_QUERY_STEP_PICKER_MODAL =
	UMB_DOCUMENT_DYNAMIC_ROOT_QUERY_STEP_PICKER_MODAL;

/**
 * @deprecated Deprecated since v19. Import `UMB_DOCUMENT_DYNAMIC_ROOT_ORIGIN_PICKER_MODAL_ALIAS` from `@umbraco-cms/backoffice/document` instead. Scheduled for removal in Umbraco 21.
 */
export const UMB_CONTENT_PICKER_DOCUMENT_ROOT_ORIGIN_PICKER_MODAL_ALIAS =
	UMB_DOCUMENT_DYNAMIC_ROOT_ORIGIN_PICKER_MODAL_ALIAS;

/**
 * @deprecated Deprecated since v19. Import `UMB_DOCUMENT_DYNAMIC_ROOT_QUERY_STEP_PICKER_MODAL_ALIAS` from `@umbraco-cms/backoffice/document` instead. Scheduled for removal in Umbraco 21.
 */
export const UMB_CONTENT_PICKER_DOCUMENT_ROOT_QUERY_STEP_PICKER_MODAL_ALIAS =
	UMB_DOCUMENT_DYNAMIC_ROOT_QUERY_STEP_PICKER_MODAL_ALIAS;
