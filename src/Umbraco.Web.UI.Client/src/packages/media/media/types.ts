import type { UmbMediaEntityType, UmbMediaPropertyValueEntityType } from './entity.js';
import type { UmbEntityVariantModel, UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbContentDetailModel, UmbElementValueModel } from '@umbraco-cms/backoffice/content';

export type * from './audit-log/types.js';
export type * from './collection/types.js';
export type * from './dropzone/types.js';
export type * from './modals/types.js';
export type * from './recycle-bin/types.js';
export type * from './repository/types.js';
export type * from './search/types.js';
export type * from './tree/types.js';
export type * from './url/types.js';

export interface UmbMediaDetailModel extends UmbContentDetailModel {
	mediaType: {
		unique: string;
		collection: UmbReferenceByUnique | null;
		icon?: string | null;
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
export interface UmbMediaValueModel<ValueType = unknown> extends UmbElementValueModel<ValueType> {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMediaVariantOptionModel extends UmbEntityVariantOptionModel<UmbEntityVariantModel> {}

export type * from './property-editors/types.js';
