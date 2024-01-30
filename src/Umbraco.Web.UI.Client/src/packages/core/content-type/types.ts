import type {
	CompositionTypeModel,
	PropertyTypeModelBaseModel,
	ReferenceByIdModel,
} from '@umbraco-cms/backoffice/backend-api';

export type UmbPropertyContainerTypes = 'Group' | 'Tab';
export interface UmbPropertyTypeContainerModel {
	id: string;
	parent?: ReferenceByIdModel | null;
	name?: string | null;
	type: UmbPropertyContainerTypes;
	sortOrder: number;
}

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
	properties: Array<UmbPropertyTypeModel>;
	containers: Array<UmbPropertyTypeContainerModel>;
	allowedContentTypes: Array<UmbContentTypeSortModel>;
	compositions: Array<UmbContentTypeCompositionModel>;
}

export interface UmbPropertyTypeModel extends Omit<PropertyTypeModelBaseModel, 'dataType'> {
	dataType: { unique: string };
}

export interface UmbContentTypeSortModel {
	contentType: { unique: string };
	sortOrder: number;
}

export interface UmbContentTypeCompositionModel {
	contentType: { unique: string };
	compositionType: CompositionTypeModel;
}
