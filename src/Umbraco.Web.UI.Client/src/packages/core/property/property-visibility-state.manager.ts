import { UmbStateManager, type UmbState } from '@umbraco-cms/backoffice/utils';

export interface UmbPropertyVisibilityState extends UmbState {
	propertyAlias: string;
}

export class UmbPropertyVisibilityStateManager extends UmbStateManager<UmbPropertyVisibilityState> {}
