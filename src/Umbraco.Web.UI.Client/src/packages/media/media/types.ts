import type { UmbMediaEntityType } from './entity.js';
import type { UmbVariantModel, UmbVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import type { MediaUrlInfoModel, MediaValueModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

export interface UmbMediaDetailModel {
	mediaType: {
		unique: string;
		collection: UmbReferenceByUnique | null;
	};
	entityType: UmbMediaEntityType;
	isTrashed: boolean;
	unique: string;
	urls: Array<MediaUrlInfoModel>;
	values: Array<MediaValueModel>;
	variants: Array<UmbVariantModel>;
}

export interface UmbMediaVariantOptionModel extends UmbVariantOptionModel<UmbVariantModel> {}
