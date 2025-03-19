import { UmbStateManager, type UmbState } from '@umbraco-cms/backoffice/utils';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

export interface UmbPropertyWriteState extends UmbState {
	propertyType: UmbReferenceByUnique;
}

export class UmbPropertyWriteStateManager<
	WriteStateType extends UmbPropertyWriteState = UmbPropertyWriteState,
> extends UmbStateManager<WriteStateType> {}
