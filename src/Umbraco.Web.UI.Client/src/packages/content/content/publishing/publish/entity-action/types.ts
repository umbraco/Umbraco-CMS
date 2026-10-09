import type { ManifestEntityAction, MetaEntityActionDefaultKind } from '@umbraco-cms/backoffice/entity-action';
import type { UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbEntityVariantPublishModel } from '@umbraco-cms/backoffice/variant';

/**
 * Manifest for an entity action registered against the shared `contentPublish` kind.
 */
export interface ManifestEntityActionContentPublishKind extends ManifestEntityAction<MetaEntityActionContentPublishKind> {
	type: 'entityAction';
	kind: 'contentPublish';
}

/**
 * Meta configuration for the `contentPublish` entity action kind.
 */
export interface MetaEntityActionContentPublishKind extends MetaEntityActionDefaultKind {
	/**
	 * Overrides the success notification message when invariant content is published.
	 * Use a localization key, e.g. `#speechBubbles_editContentPublishedText`.
	 */
	publishedNotificationMessage?: string;
	/**
	 * Overrides the success notification message when one or more variants are published.
	 * Use a localization key, e.g. `#speechBubbles_editVariantPublishedText` — the list of published variants
	 * is passed as the first term argument, so a plain (non-key) string will not include it.
	 */
	variantPublishedNotificationMessage?: string;
	/**
	 * Alias of the detail repository used to load the content item.
	 */
	detailRepositoryAlias: string;
	/**
	 * Alias of the publishing repository used to perform the publish operation.
	 */
	publishingRepositoryAlias: string;
}

/**
 * Contract that the repository registered under `publishingRepositoryAlias` must fulfil.
 */
export interface UmbContentPublishingRepository extends UmbApi {
	publish(unique: string, variants: Array<UmbEntityVariantPublishModel>): Promise<UmbRepositoryResponse<void>>;
}

declare global {
	interface UmbExtensionManifestMap {
		umbEntityActionContentPublishKind: ManifestEntityActionContentPublishKind;
	}
}
