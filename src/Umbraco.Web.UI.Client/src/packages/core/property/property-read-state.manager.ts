import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { UmbStateManager, type UmbState } from '@umbraco-cms/backoffice/utils';

export interface UmbPropertyReadState extends UmbState {
	propertyType: UmbReferenceByUnique;
}

export class UmbPropertyReadStateManager<
	ReadStateType extends UmbPropertyReadState = UmbPropertyReadState,
> extends UmbStateManager<ReadStateType> {}
