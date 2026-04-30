import type { UmbCollectionItemModel } from '@umbraco-cms/backoffice/collection';
import type { UmbFileDropzoneItemStatus } from '@umbraco-cms/backoffice/dropzone';
import type { UmbEntityWithFlags } from '@umbraco-cms/backoffice/entity-flag';

export interface UmbMediaCollectionItemModel extends UmbCollectionItemModel, UmbEntityWithFlags {
	/** @deprecated Use `mediaType.alias` on `UmbMediaCollectionItemModel` instead. This field will be removed in v19 */
	contentTypeAlias?: string;
	createDate: Date;
	creator?: string | null;
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
