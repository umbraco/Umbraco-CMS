import type { ManifestEntityAction, MetaEntityActionDefaultKind } from '@umbraco-cms/backoffice/entity-action';
import type { UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

/**
 * Manifest for an entity action registered against the shared `contentUnpublish` kind.
 */
export interface ManifestEntityActionContentUnpublishKind extends ManifestEntityAction<MetaEntityActionContentUnpublishKind> {
	type: 'entityAction';
	kind: 'contentUnpublish';
}

/**
 * Meta configuration for the `contentUnpublish` entity action kind.
 */
export interface MetaEntityActionContentUnpublishKind extends MetaEntityActionDefaultKind {
	/**
	 * Overrides the success notification message when content is unpublished.
	 * Use a localization key, e.g. `#speechBubbles_contentUnpublished`.
	 */
	unpublishedNotificationMessage?: string;
	/**
	 * Alias of the detail repository used to load the content item.
	 */
	detailRepositoryAlias: string;
	/**
	 * Alias of the publishing repository used to perform the unpublish operation.
	 */
	publishingRepositoryAlias: string;
	/**
	 * Alias of the item repository used by the modal to present the affected items.
	 */
	itemRepositoryAlias: string;
	/**
	 * Alias of the reference repository used by the modal to warn about inbound references.
	 */
	referenceRepositoryAlias: string;
	/**
	 * Alias of the configuration repository used by the modal to read unpublish behaviour configuration.
	 */
	configurationRepositoryAlias: string;
}

/**
 * Contract that the repository registered under `publishingRepositoryAlias` must fulfil.
 */
export interface UmbContentUnpublishingRepository extends UmbApi {
	unpublish(unique: string, variantIds: Array<UmbVariantId>): Promise<UmbRepositoryResponse<void>>;
}

declare global {
	interface UmbExtensionManifestMap {
		umbEntityActionContentUnpublishKind: ManifestEntityActionContentUnpublishKind;
	}
}
