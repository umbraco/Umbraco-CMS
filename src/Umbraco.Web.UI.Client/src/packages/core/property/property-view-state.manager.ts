import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { UmbStateManager, type UmbState } from '@umbraco-cms/backoffice/utils';

export interface UmbPropertyViewState extends UmbState {
	propertyType: UmbReferenceByUnique;
}

export class UmbPropertyViewStateManager<
	ViewStateType extends UmbPropertyViewState = UmbPropertyViewState,
> extends UmbStateManager<ViewStateType> {}
