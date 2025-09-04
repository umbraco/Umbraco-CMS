import type { ManifestCollection } from './extensions/types.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import type { UmbPaginationManager } from '@umbraco-cms/backoffice/utils';

export type * from './action/create/types.js';
export type * from './extensions/types.js';
export type * from './conditions/types.js';
export type * from './workspace-view/types.js';

/** @deprecated No longer used internally. This will be removed in Umbraco 17. [LK] */
export interface UmbCollectionBulkActionPermissions {
	allowBulkCopy: boolean;
	allowBulkDelete: boolean;
	allowBulkMove: boolean;
	allowBulkPublish: boolean;
	allowBulkUnpublish: boolean;
}

export interface UmbCollectionConfiguration {
	unique?: UmbEntityUnique;
	dataTypeId?: string;
	/** @deprecated No longer used internally. This will be removed in Umbraco 17. [LK] */
	allowedEntityBulkActions?: UmbCollectionBulkActionPermissions;
	layouts?: Array<UmbCollectionLayoutConfiguration>;
	orderBy?: string;
	orderDirection?: string;
	pageSize?: number;
	noItemsLabel?: string;
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
