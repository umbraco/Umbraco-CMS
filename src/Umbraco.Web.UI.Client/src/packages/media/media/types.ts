import type { UmbMediaEntityType } from './entity.js';
import type { UmbVariantModel, UmbVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

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

export interface UmbMediaVariantModel extends UmbVariantModel {}

export interface UmbMediaValueModel<ValueType = unknown> {
	culture: string | null;
	segment: string | null;
	alias: string;
	value: ValueType;
}

export interface UmbMediaVariantOptionModel extends UmbVariantOptionModel<UmbVariantModel> {}

export type * from './property-editors/types.js';
