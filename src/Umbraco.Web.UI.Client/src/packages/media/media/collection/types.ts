import type { UmbFileDropzoneItemStatus } from '../dropzone/types.js';
import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';

export interface UmbMediaCollectionFilterModel extends UmbCollectionFilterModel {
	unique?: string;
	dataTypeId?: string;
	orderBy?: string;
	orderDirection?: 'asc' | 'desc';
	userDefinedProperties: Array<{ alias: string; header: string; isSystem: boolean }>;
}

export interface UmbMediaCollectionItemModel {
	unique: string;
	entityType: string;
	contentTypeAlias?: string;
	createDate: Date;
	creator?: string | null;
	icon?: string;
	name?: string;
	sortOrder?: number;
	updateDate: Date;
	updater?: string | null;
	values?: Array<{ alias: string; value: string }>;
	url?: string;
	status?: UmbFileDropzoneItemStatus;
}

export interface UmbEditableMediaCollectionItemModel {
	item: UmbMediaCollectionItemModel;
	editPath: string;
}
