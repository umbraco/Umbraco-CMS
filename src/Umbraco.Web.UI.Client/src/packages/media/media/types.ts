import type { UmbMediaEntityType } from './entity.js';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbVariantModel } from '@umbraco-cms/backoffice/variant';
import type { MediaUrlInfoModel, MediaValueModel } from '@umbraco-cms/backoffice/external/backend-api';

export interface UmbMediaDetailModel {
	mediaType: {
		unique: string;
		collection: UmbReferenceByUnique | null;
	};
	entityType: UmbMediaEntityType;
	isTrashed: boolean;
	unique: string;
	parentUnique: string | null;
	urls: Array<MediaUrlInfoModel>;
	values: Array<MediaValueModel>;
	variants: Array<UmbVariantModel>;
}
