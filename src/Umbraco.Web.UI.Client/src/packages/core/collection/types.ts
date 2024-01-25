import type { ManifestCollection } from '@umbraco-cms/backoffice/extension-registry';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbPaginationManager } from '@umbraco-cms/backoffice/utils';

export interface UmbCollectionConfiguration {
	pageSize?: number;
	defaultViewAlias?: string;
}

export interface UmbCollectionContext {
	setManifest(manifest: ManifestCollection): void;
	getManifest(): ManifestCollection | undefined;
	requestCollection(): Promise<void>;
	pagination: UmbPaginationManager;
	items: Observable<any[]>;
	totalItems: Observable<number>;
}
