import type { UmbCollectionItemModel } from '@umbraco-cms/backoffice/collection';
import type { UmbFileDropzoneItemStatus } from '@umbraco-cms/backoffice/dropzone';
import type { UmbEntityWithFlags } from '@umbraco-cms/backoffice/entity-flag';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

export interface UmbMediaCollectionItemModel extends UmbCollectionItemModel, UmbEntityWithFlags {
	/** @deprecated Use `contentType.alias` on `UmbMediaCollectionItemModel` instead. This field will be removed in v19 */
	contentTypeAlias?: string;
	// TODO (V20): make `contentType` required when the deprecated `mediaType` field is removed.
	contentType?: {
		unique: string;
		icon: string;
		alias: string;
		collection: UmbReferenceByUnique | null;
	};
	createDate: Date;
	creator?: string | null;
	/**
	 * @deprecated Use `contentType` instead. This field will be removed in v20.
	 */
	mediaType?: {
		unique: string;
		icon: string;
		alias: string;
	};
	sortOrder?: number;
	updateDate: Date;
	updater?: string | null;
	values?: Array<{ alias: string; value: string }>;
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
