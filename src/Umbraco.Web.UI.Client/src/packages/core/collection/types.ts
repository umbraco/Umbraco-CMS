import { ManifestCollection } from '@umbraco-cms/backoffice/extension-registry';
import { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbPaginationManager } from '@umbraco-cms/backoffice/utils';

export interface UmbCollectionConfiguration {
	pageSize: number;
	defaultView?: string;
}

export interface UmbCollectionContext {
	setManifest(manifest: ManifestCollection): void;
	getManifest(): ManifestCollection | undefined;
	requestCollection(): Promise<void>;
	pagination: UmbPaginationManager;
	items: Observable<any[]>;
	totalItems: Observable<number>;
}
