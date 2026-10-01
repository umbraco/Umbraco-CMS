import type {
	ManifestDynamicRootOrigin as DocumentManifestDynamicRootOrigin,
	ManifestDynamicRootQueryStep as DocumentManifestDynamicRootQueryStep,
	MetaDynamicRootOrigin as DocumentMetaDynamicRootOrigin,
	MetaDynamicRootQueryStep as DocumentMetaDynamicRootQueryStep,
	UmbDocumentDynamicRootOriginPickerModalData,
	UmbDocumentDynamicRootQueryStepPickerModalData,
	UmbDynamicRoot,
	UmbDynamicRootQueryStep,
} from '@umbraco-cms/backoffice/document';

// The dynamic root feature moved to the document package. The names this package exported before the move stay available.
/**
 * @deprecated Deprecated since v19. Import `ManifestDynamicRootOrigin` from `@umbraco-cms/backoffice/document` instead. Scheduled for removal in Umbraco 21.
 */
export type ManifestDynamicRootOrigin = DocumentManifestDynamicRootOrigin;

/**
 * @deprecated Deprecated since v19. Import `ManifestDynamicRootQueryStep` from `@umbraco-cms/backoffice/document` instead. Scheduled for removal in Umbraco 21.
 */
export type ManifestDynamicRootQueryStep = DocumentManifestDynamicRootQueryStep;

/**
 * @deprecated Deprecated since v19. Import `MetaDynamicRootOrigin` from `@umbraco-cms/backoffice/document` instead. Scheduled for removal in Umbraco 21.
 */
export type MetaDynamicRootOrigin = DocumentMetaDynamicRootOrigin;

/**
 * @deprecated Deprecated since v19. Import `MetaDynamicRootQueryStep` from `@umbraco-cms/backoffice/document` instead. Scheduled for removal in Umbraco 21.
 */
export type MetaDynamicRootQueryStep = DocumentMetaDynamicRootQueryStep;

/**
 * @deprecated Deprecated since v19. Import `UmbDocumentDynamicRootOriginPickerModalData` from `@umbraco-cms/backoffice/document` instead. Scheduled for removal in Umbraco 21.
 */
export type UmbContentPickerDocumentRootOriginModalData = UmbDocumentDynamicRootOriginPickerModalData;

/**
 * @deprecated Deprecated since v19. Import `UmbDocumentDynamicRootQueryStepPickerModalData` from `@umbraco-cms/backoffice/document` instead. Scheduled for removal in Umbraco 21.
 */
export type UmbContentPickerDocumentRootQueryStepModalData = UmbDocumentDynamicRootQueryStepPickerModalData;

/**
 * @deprecated Deprecated since v19. Import `UmbDynamicRoot` from `@umbraco-cms/backoffice/document` instead. Scheduled for removal in Umbraco 21.
 */
export type UmbContentPickerDynamicRoot = UmbDynamicRoot;

/**
 * @deprecated Deprecated since v19. Import `UmbDynamicRootQueryStep` from `@umbraco-cms/backoffice/document` instead. Scheduled for removal in Umbraco 21.
 */
export type UmbContentPickerDynamicRootQueryStep = UmbDynamicRootQueryStep;

export type UmbContentPickerSourceType = 'content' | 'member' | 'media';

export type UmbContentPickerSource = {
	type: UmbContentPickerSourceType;
	id?: string;
	dynamicRoot?: UmbDynamicRoot;
};
