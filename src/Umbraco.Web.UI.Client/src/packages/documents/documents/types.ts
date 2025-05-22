import type { UmbDocumentEntityType } from './entity.js';
import type {
	UmbEntityVariantModel,
	UmbEntityVariantOptionModel,
	UmbEntityVariantPublishModel,
} from '@umbraco-cms/backoffice/variant';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { DocumentVariantStateModel as UmbDocumentVariantState } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbContentDetailModel, UmbElementValueModel } from '@umbraco-cms/backoffice/content';
export { UmbDocumentVariantState };

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

export interface UmbDocumentDetailModel extends UmbContentDetailModel {
	documentType: {
		unique: string;
		collection: UmbReferenceByUnique | null;
		icon?: string | null;
	};
	entityType: UmbDocumentEntityType;
	isTrashed: boolean;
	template: { unique: string } | null;
	values: Array<UmbDocumentValueModel>;
	variants: Array<UmbDocumentVariantModel>;
}

export interface UmbDocumentVariantModel extends UmbEntityVariantModel {
	state: UmbDocumentVariantState | null;
	publishDate: string | null;
	scheduledPublishDate: string | null;
	scheduledUnpublishDate: string | null;
}

export interface UmbDocumentUrlInfoModel {
	culture: string | null;
	url: string;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDocumentValueModel<ValueType = unknown> extends UmbElementValueModel<ValueType> {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDocumentVariantOptionModel extends UmbEntityVariantOptionModel<UmbDocumentVariantModel> {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDocumentVariantPublishModel extends UmbEntityVariantPublishModel {}
