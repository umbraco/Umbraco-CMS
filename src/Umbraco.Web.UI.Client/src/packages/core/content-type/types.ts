import {
	ContentTypeCompositionModel,
	ContentTypeSortModel,
	MediaTypePropertyTypeContainerResponseModel,
	MediaTypePropertyTypeResponseModel,
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
	properties: Array<MediaTypePropertyTypeResponseModel>;
	containers: Array<MediaTypePropertyTypeContainerResponseModel>;
	allowedContentTypes: Array<ContentTypeSortModel>;
	compositions: Array<ContentTypeCompositionModel>;
}
