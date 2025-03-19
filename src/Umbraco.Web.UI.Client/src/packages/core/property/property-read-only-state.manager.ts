import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { UmbReadOnlyStateManager, type UmbState } from '@umbraco-cms/backoffice/utils';

export interface UmbPropertyReadOnlyState extends UmbState {
	propertyType: UmbReferenceByUnique;
}

export class UmbPropertyReadOnlyStateManager<
	ReadOnlyStateType extends UmbPropertyReadOnlyState = UmbPropertyReadOnlyState,
> extends UmbReadOnlyStateManager<ReadOnlyStateType> {}
