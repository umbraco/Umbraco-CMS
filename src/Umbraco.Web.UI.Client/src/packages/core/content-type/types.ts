import type { CompositionTypeModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

export type * from './composition/types.js';

export type UmbPropertyContainerTypes = 'Group' | 'Tab';

export interface UmbPropertyTypeContainerModel {
	id: string;
	parent: { id: string } | null; // TODO: change to unique
	name: string;
	type: UmbPropertyContainerTypes;
	sortOrder: number;
}
/**
 *
 * @deprecated
 * This model is deprecated and will be removed in version 17. Please use the UmbContentTypeDetailModel instead.
 * @export
 * @interface UmbContentTypeModel
 */
export interface UmbContentTypeModel {
	unique: string;
	name: string;
	alias: string;
	description: string;
	icon: string;
	allowedAtRoot: boolean;
	variesByCulture: boolean;
	variesBySegment: boolean;
	isElement: boolean;
	properties: Array<UmbPropertyTypeModel>;
	containers: Array<UmbPropertyTypeContainerModel>;
	allowedContentTypes: Array<UmbContentTypeSortModel>;
	compositions: Array<UmbContentTypeCompositionModel>;
	collection: UmbReferenceByUnique | null;
}

export interface UmbContentTypeDetailModel extends UmbContentTypeModel {
	entityType: string;
}

export interface UmbPropertyTypeScaffoldModel extends Omit<UmbPropertyTypeModel, 'dataType'> {
	dataType?: UmbPropertyTypeModel['dataType'];
}

export interface UmbPropertyTypeModel {
	dataType: { unique: string };
	id: string; // TODO: change to unique
	unique: string;
	container?: { id: string } | null; // TODO: change to unique
	sortOrder: number;
	alias: string;
	name: string;
	description?: string | null;
	variesByCulture: boolean;
	variesBySegment: boolean;
	validation: UmbPropertyTypeValidationModel;
	appearance: UmbPropertyTypeAppearanceModel;
	visibility?: UmbPropertyTypeVisibilityModel;
	isSensitive?: boolean;
}

export interface UmbPropertyTypeVisibilityModel {
	memberCanEdit: boolean;
	memberCanView: boolean;
}

export interface UmbPropertyTypeValidationModel {
	mandatory: boolean;
	mandatoryMessage?: string | null;
	regEx?: string | null;
	regExMessage?: string | null;
}

export interface UmbPropertyTypeAppearanceModel {
	labelOnTop: boolean;
}

export interface UmbContentTypeSortModel {
	contentType: { unique: string };
	sortOrder: number;
}

export interface UmbContentTypeCompositionModel {
	contentType: { unique: string };
	compositionType: CompositionTypeModel;
}
