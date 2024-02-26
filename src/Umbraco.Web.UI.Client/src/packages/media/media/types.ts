import type { UmbMediaEntityType } from './entity.js';
import type { UmbVariantModel, UmbVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import type { MediaUrlInfoModel, MediaValueModel } from '@umbraco-cms/backoffice/external/backend-api';

export interface UmbMediaDetailModel {
	mediaType: { unique: string };
	entityType: UmbMediaEntityType;
	isTrashed: boolean;
	unique: string;
	parentUnique: string | null;
	urls: Array<MediaUrlInfoModel>;
	values: Array<MediaValueModel>;
	variants: Array<UmbVariantModel>;
}

export interface UmbMediaVariantOptionModel extends UmbVariantOptionModel<UmbVariantModel> {}
