import type {
	PropertyTypeContainerModelBaseModel,
	PropertyTypeModelBaseModel,
	CompositionTypeModel,
} from '@umbraco-cms/backoffice/backend-api';

export interface UmbContentTypeModel {
	unique: string;
	parentUnique: string | null;
	name: string;
	alias: string;
	description: string | null;
	icon: string;
	allowedAsRoot: boolean;
	variesByCulture: boolean;
	variesBySegment: boolean;
	isElement: boolean;
	// TODO: investigate if we need our own model for these
	properties: Array<PropertyTypeModelBaseModel>;
	containers: Array<PropertyTypeContainerModelBaseModel>;
	allowedContentTypes: Array<UmbContentTypeSortModel>;
	compositions: Array<UmbContentTypeCompositionModel>;
}

export interface UmbContentTypeSortModel {
	contentType: { unique: string };
	sortOrder: number;
}

export interface UmbContentTypeCompositionModel {
	contentType: { unique: string };
	compositionType: CompositionTypeModel;
}
