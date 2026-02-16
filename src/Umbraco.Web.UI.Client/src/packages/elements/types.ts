import type { UmbElementEntityType } from './entity.js';
import { DocumentVariantStateModel as UmbElementVariantState } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbContentDetailModel, UmbContentValueModel } from '@umbraco-cms/backoffice/content';
import type { UmbEntityVariantModel, UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';

export type * from './audit-log/types.js';
export type * from './collection/types.js';
export type * from './entity.js';
export type * from './item/types.js';
export type * from './modals/types.js';
export type * from './publishing/types.js';
export type * from './recycle-bin/types.js';
export type * from './tree/types.js';
export type * from './user-permissions/types.js';
export type * from './workspace/types.js';

export { UmbElementVariantState };

export interface UmbElementDetailModel extends UmbContentDetailModel {
	documentType: {
		unique: string;
		collection: null;
	};
	entityType: UmbElementEntityType;
	unique: string;
	isTrashed: boolean;
	values: Array<UmbElementValueModel>;
	variants: Array<UmbElementVariantModel>;
}

export interface UmbElementVariantModel extends UmbEntityVariantModel {
	state?: UmbElementVariantState | null;
	publishDate?: string | null;
	scheduledPublishDate?: string | null;
	scheduledUnpublishDate?: string | null;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbElementValueModel<ValueType = unknown> extends UmbContentValueModel<ValueType> {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbElementVariantOptionModel extends UmbEntityVariantOptionModel<UmbElementVariantModel> {}
