import type { UmbDocumentEntityType } from './entity.js';
import type { UmbVariantModel } from '@umbraco-cms/backoffice/variant';

export interface UmbDocumentDetailModel {
	documentType: { unique: string };
	entityType: UmbDocumentEntityType;
	isTrashed: boolean;
	template: { unique: string } | null;
	unique: string;
	parentUnique: string | null;
	urls: Array<UmbDocumentUrlInfoModel>;
	values: Array<UmbDocumentValueModel>;
	variants: Array<UmbVariantModel>;
}

export interface UmbDocumentUrlInfoModel {
	culture: string | null;
	url: string;
}

export interface UmbDocumentValueModel<ValueType = unknown> {
	culture: string | null;
	segment: string | null;
	alias: string;
	value: ValueType;
}
