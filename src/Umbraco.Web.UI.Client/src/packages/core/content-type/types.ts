import type { CompositionTypeModel, PropertyTypeModelBaseModel } from '@umbraco-cms/backoffice/external/backend-api';

export type UmbPropertyContainerTypes = 'Group' | 'Tab';
export interface UmbPropertyTypeContainerModel {
	id: string;
	parent: { id: string } | null;
	name: string | null;
	type: UmbPropertyContainerTypes;
	sortOrder: number;
}

export interface UmbContentTypeModel {
	unique: string;
	name: string;
	alias: string;
	description: string | null;
	icon: string;
	allowedAsRoot: boolean;
	variesByCulture: boolean;
	variesBySegment: boolean;
	isElement: boolean;
	properties: Array<UmbPropertyTypeModel>;
	containers: Array<UmbPropertyTypeContainerModel>;
	allowedContentTypes: Array<UmbContentTypeSortModel>;
	compositions: Array<UmbContentTypeCompositionModel>;
}

export interface UmbPropertyTypeScaffoldModel extends Omit<UmbPropertyTypeModel, 'dataType'> {
	dataType?: UmbPropertyTypeModel['dataType'];
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
