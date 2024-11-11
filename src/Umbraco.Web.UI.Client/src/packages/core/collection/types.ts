import type { ManifestCollection } from './extensions/index.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbPaginationManager } from '@umbraco-cms/backoffice/utils';

export type * from './extensions/index.js';
export type * from './conditions/types.js';

export interface UmbCollectionBulkActionPermissions {
	allowBulkCopy: boolean;
	allowBulkDelete: boolean;
	allowBulkMove: boolean;
	allowBulkPublish: boolean;
	allowBulkUnpublish: boolean;
}

export interface UmbCollectionConfiguration {
	unique?: string;
	dataTypeId?: string;
	allowedEntityBulkActions?: UmbCollectionBulkActionPermissions;
	layouts?: Array<UmbCollectionLayoutConfiguration>;
	orderBy?: string;
	orderDirection?: string;
	pageSize?: number;
	userDefinedProperties?: Array<UmbCollectionColumnConfiguration>;
}

export interface UmbCollectionColumnConfiguration {
	alias: string;
	header: string;
	isSystem: 1 | 0;
	elementName?: string;
	nameTemplate?: string;
}

export interface UmbCollectionLayoutConfiguration {
	icon?: string;
	name: string;
	collectionView: string;
}

export interface UmbCollectionContext {
	setConfig(config: UmbCollectionConfiguration): void;
	getConfig(): UmbCollectionConfiguration | undefined;
	setManifest(manifest: ManifestCollection): void;
	getManifest(): ManifestCollection | undefined;
	requestCollection(): Promise<void>;
	pagination: UmbPaginationManager;
	items: Observable<any[]>;
	totalItems: Observable<number>;
}

export type * from './extensions/index.js';
