import type { ManifestEntityAction, MetaEntityActionDefaultKind } from '@umbraco-cms/backoffice/entity-action';
import type { UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export interface ManifestEntityActionContentUnpublishKind extends ManifestEntityAction<MetaEntityActionContentUnpublishKind> {
	type: 'entityAction';
	kind: 'contentUnpublish';
}

export interface MetaEntityActionContentUnpublishKind extends MetaEntityActionDefaultKind {
	unpublishedNotificationMessage?: string;
	detailRepositoryAlias: string;
	publishingRepositoryAlias: string;
	itemRepositoryAlias: string;
	referenceRepositoryAlias: string;
	configurationRepositoryAlias: string;
}

export interface UmbContentUnpublishingRepository extends UmbApi {
	unpublish(unique: string, variantIds: Array<UmbVariantId>): Promise<UmbRepositoryResponse<void>>;
}

declare global {
	interface UmbExtensionManifestMap {
		umbEntityActionContentUnpublishKind: ManifestEntityActionContentUnpublishKind;
	}
}
