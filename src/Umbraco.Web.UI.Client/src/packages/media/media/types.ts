import type { UmbMediaEntityType } from './entity.js';
import type { UmbVariantModel, UmbVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbContentValueModel } from '@umbraco-cms/backoffice/content';

export interface UmbMediaDetailModel {
	mediaType: {
		unique: string;
		collection: UmbReferenceByUnique | null;
	};
	entityType: UmbMediaEntityType;
	isTrashed: boolean;
	unique: string;
	urls: Array<UmbMediaUrlInfoModel>;
	values: Array<UmbMediaValueModel>;
	variants: Array<UmbVariantModel>;
}

export interface UmbMediaUrlInfoModel {
	culture: string | null;
	url: string;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMediaVariantModel extends UmbVariantModel {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMediaValueModel<ValueType = unknown> extends UmbContentValueModel<ValueType> {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMediaVariantOptionModel extends UmbVariantOptionModel<UmbVariantModel> {}

export type * from './property-editors/types.js';
