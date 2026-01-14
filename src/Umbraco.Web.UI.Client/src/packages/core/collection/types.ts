import type { ManifestCollection } from './extensions/types.js';
import type { UmbCollectionItemModel } from './item/types.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import type { UmbPaginationManager } from '@umbraco-cms/backoffice/utils';

export type * from './action/create/types.js';
export type * from './collection-item-picker-modal/types.js';
export type * from './conditions/types.js';
export type * from './filter/types.js';
export type * from './extensions/types.js';
export type * from './item/types.js';
export type * from './menu/types.js';
export type * from './view/types.js';
export type * from './workspace-view/types.js';

export interface UmbCollectionConfiguration {
	unique?: UmbEntityUnique;
	dataTypeId?: string;
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

export type UmbCollectionSelectionConfiguration = {
	multiple?: boolean;
	selectable?: boolean;
	selection?: Array<string | null>;
};

export interface UmbCollectionContext {
	setConfig(config: UmbCollectionConfiguration): void;
	getConfig(): UmbCollectionConfiguration | undefined;
	setManifest(manifest: ManifestCollection): void;
	getManifest(): ManifestCollection | undefined;
	requestCollection(): Promise<void>;
	requestItemHref?(item: UmbCollectionItemModel): Promise<string | undefined>;
	pagination: UmbPaginationManager;
	items: Observable<any[]>;
	totalItems: Observable<number>;
}
