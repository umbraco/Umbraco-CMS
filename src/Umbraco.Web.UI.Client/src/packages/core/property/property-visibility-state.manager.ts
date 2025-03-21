import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { UmbStateManager, type UmbState } from '@umbraco-cms/backoffice/utils';

export interface UmbPropertyVisibilityState extends UmbState {
	propertyType: UmbReferenceByUnique;
}

export class UmbPropertyVisibilityStateManager<
	VisibilityStateType extends UmbPropertyVisibilityState = UmbPropertyVisibilityState,
> extends UmbStateManager<VisibilityStateType> {}
