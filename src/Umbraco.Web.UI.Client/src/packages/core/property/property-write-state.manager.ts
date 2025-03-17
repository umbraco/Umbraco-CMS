import type { UmbPropertyTypeReferenceTypeUnion } from './types/index.js';
import { UmbStateManager, type UmbState } from '@umbraco-cms/backoffice/utils';

export interface UmbPropertyWriteState<PropertyTypeReferenceType = UmbPropertyTypeReferenceTypeUnion> extends UmbState {
	propertyType: PropertyTypeReferenceType;
}

export class UmbPropertyWriteStateManager<
	ReferenceType = UmbPropertyTypeReferenceTypeUnion,
	WriteStateType extends UmbPropertyWriteState<ReferenceType> = UmbPropertyWriteState<ReferenceType>,
> extends UmbStateManager<WriteStateType> {}
