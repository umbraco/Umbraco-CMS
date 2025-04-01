import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { UmbReadOnlyStateManager, type UmbStateEntry } from '@umbraco-cms/backoffice/utils';

export interface UmbPropertyReadOnlyState extends UmbStateEntry {
	propertyType: UmbReferenceByUnique;
}

export class UmbPropertyReadOnlyStateManager<
	ReadOnlyStateType extends UmbPropertyReadOnlyState = UmbPropertyReadOnlyState,
> extends UmbReadOnlyStateManager<ReadOnlyStateType> {}
