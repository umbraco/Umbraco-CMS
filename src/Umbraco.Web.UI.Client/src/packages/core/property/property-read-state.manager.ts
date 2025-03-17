import type { UmbPropertyTypeReferenceTypeUnion } from './types/index.js';
import { UmbStateManager, type UmbState } from '@umbraco-cms/backoffice/utils';

export interface UmbPropertyReadState<PropertyTypeReferenceType = UmbPropertyTypeReferenceTypeUnion> extends UmbState {
	propertyType: PropertyTypeReferenceType;
}

export class UmbPropertyReadStateManager<
	ReferenceType = UmbPropertyTypeReferenceTypeUnion,
	ReadStateType extends UmbPropertyReadState<ReferenceType> = UmbPropertyReadState<ReferenceType>,
> extends UmbStateManager<ReadStateType> {}
