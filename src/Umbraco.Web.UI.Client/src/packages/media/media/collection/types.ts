import type { UmbFileDropzoneItemStatus } from '@umbraco-cms/backoffice/dropzone';
import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';
import type { UmbEntityWithFlags } from '@umbraco-cms/backoffice/entity-flag';

export interface UmbMediaCollectionFilterModel extends UmbCollectionFilterModel {
	unique?: string;
	dataTypeId?: string;
	orderBy?: string;
	orderDirection?: 'asc' | 'desc';
	userDefinedProperties: Array<{ alias: string; header: string; isSystem: boolean }>;
}

export interface UmbMediaCollectionItemModel extends UmbEntityWithFlags {
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
	values?: Array<{ alias: string; editorAlias: string; value: string }>;
	url?: string;
	status?: UmbFileDropzoneItemStatus;
	/**
	 * The progress of the item in percentage.
	 */
	progress?: number;
}

export interface UmbEditableMediaCollectionItemModel {
	item: UmbMediaCollectionItemModel;
	editPath: string;
}
