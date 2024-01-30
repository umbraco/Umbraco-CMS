import type { UmbMediaEntityType } from './entity.js';
import type { UmbVariantModel } from '@umbraco-cms/backoffice/variant';
import type { ContentUrlInfoModel, MediaValueModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbMediaDetailModel {
	mediaType: { unique: string };
	entityType: UmbMediaEntityType;
	isTrashed: boolean;
	unique: string;
	parentUnique: string | null;
	urls: Array<ContentUrlInfoModel>;
	values: Array<MediaValueModel>;
	variants: Array<UmbVariantModel>;
}
