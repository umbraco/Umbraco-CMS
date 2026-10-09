import type * as DocumentModule from '@umbraco-cms/backoffice/document';
import type * as DynamicRoot from '@umbraco-cms/backoffice/dynamic-root';

/**
 * @deprecated Deprecated since v19. Import `ManifestDynamicRootOrigin` from `@umbraco-cms/backoffice/dynamic-root` instead. Scheduled for removal in Umbraco 21.
 */
export type ManifestDynamicRootOrigin = DynamicRoot.ManifestDynamicRootOrigin;

/**
 * @deprecated Deprecated since v19. Import `ManifestDynamicRootQueryStep` from `@umbraco-cms/backoffice/dynamic-root` instead. Scheduled for removal in Umbraco 21.
 */
export type ManifestDynamicRootQueryStep = DynamicRoot.ManifestDynamicRootQueryStep;

/**
 * @deprecated Deprecated since v19. Import `MetaDynamicRootOrigin` from `@umbraco-cms/backoffice/dynamic-root` instead. Scheduled for removal in Umbraco 21.
 */
export type MetaDynamicRootOrigin = DynamicRoot.MetaDynamicRootOrigin;

/**
 * @deprecated Deprecated since v19. Import `MetaDynamicRootQueryStep` from `@umbraco-cms/backoffice/dynamic-root` instead. Scheduled for removal in Umbraco 21.
 */
export type MetaDynamicRootQueryStep = DynamicRoot.MetaDynamicRootQueryStep;

/**
 * @deprecated Deprecated since v19. Import `UmbDocumentDynamicRootOriginPickerModalData` from `@umbraco-cms/backoffice/document` instead. Scheduled for removal in Umbraco 21.
 */
export type UmbContentPickerDocumentRootOriginModalData = DocumentModule.UmbDocumentDynamicRootOriginPickerModalData;

/**
 * @deprecated Deprecated since v19. Import `UmbDocumentDynamicRootQueryStepPickerModalData` from `@umbraco-cms/backoffice/document` instead. Scheduled for removal in Umbraco 21.
 */
export type UmbContentPickerDocumentRootQueryStepModalData =
	DocumentModule.UmbDocumentDynamicRootQueryStepPickerModalData;

/**
 * @deprecated Deprecated since v19. Import `UmbDynamicRoot` from `@umbraco-cms/backoffice/dynamic-root` instead. Scheduled for removal in Umbraco 21.
 */
export type UmbContentPickerDynamicRoot = DynamicRoot.UmbDynamicRoot;

/**
 * @deprecated Deprecated since v19. Import `UmbDynamicRootQueryStep` from `@umbraco-cms/backoffice/dynamic-root` instead. Scheduled for removal in Umbraco 21.
 */
export type UmbContentPickerDynamicRootQueryStep = DynamicRoot.UmbDynamicRootQueryStep;

export type UmbContentPickerSourceType = 'content' | 'member' | 'media';

export type UmbContentPickerSource = {
	type: UmbContentPickerSourceType;
	id?: string;
	dynamicRoot?: DynamicRoot.UmbDynamicRoot;
};
