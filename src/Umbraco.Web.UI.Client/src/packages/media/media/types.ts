import type { UmbMediaEntityType } from './entity.js';
import type { UmbEntityVariantModel, UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbContentDetailModel, UmbContentValueModel } from '@umbraco-cms/backoffice/content';

export interface UmbMediaDetailModel extends UmbContentDetailModel {
	mediaType: {
		unique: string;
		collection: UmbReferenceByUnique | null;
	};
	entityType: UmbMediaEntityType;
	isTrashed: boolean;
	unique: string;
	urls: Array<UmbMediaUrlInfoModel>;
	values: Array<UmbMediaValueModel>;
	variants: Array<UmbEntityVariantModel>;
}

export interface UmbMediaUrlInfoModel {
	culture: string | null;
	url: string;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMediaVariantModel extends UmbEntityVariantModel {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMediaValueModel<ValueType = unknown> extends UmbContentValueModel<ValueType> {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMediaVariantOptionModel extends UmbEntityVariantOptionModel<UmbEntityVariantModel> {}

export type * from './property-editors/types.js';
