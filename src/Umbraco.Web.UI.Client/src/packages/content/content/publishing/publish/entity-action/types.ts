import type { ManifestEntityAction, MetaEntityActionDefaultKind } from '@umbraco-cms/backoffice/entity-action';
import type { UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbEntityVariantPublishModel } from '@umbraco-cms/backoffice/variant';

export interface ManifestEntityActionContentPublishKind extends ManifestEntityAction<MetaEntityActionContentPublishKind> {
	type: 'entityAction';
	kind: 'contentPublish';
}

export interface MetaEntityActionContentPublishKind extends MetaEntityActionDefaultKind {
	publishedNotificationMessage?: string;
	variantPublishedNotificationMessage?: string;
	detailRepositoryAlias: string;
	publishingRepositoryAlias: string;
}

export interface UmbContentPublishingRepository extends UmbApi {
	publish(unique: string, variants: Array<UmbEntityVariantPublishModel>): Promise<UmbRepositoryResponse<void>>;
}

declare global {
	interface UmbExtensionManifestMap {
		umbEntityActionContentPublishKind: ManifestEntityActionContentPublishKind;
	}
}
