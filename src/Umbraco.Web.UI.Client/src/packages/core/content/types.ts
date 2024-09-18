import type { UmbPropertyValueData } from '@umbraco-cms/backoffice/property';

export interface UmbContentValueModel<ValueType = unknown> extends UmbPropertyValueData<ValueType> {
	editorAlias: string;
	culture: string | null;
	segment: string | null;
}

export interface UmbPotentialContentValueModel<ValueType = unknown> extends UmbPropertyValueData<ValueType> {
	editorAlias?: string;
	culture?: string | null;
	segment?: string | null;
}
