import { UmbStateManager, type UmbState } from '@umbraco-cms/backoffice/utils';

export interface UmbPropertyWriteState extends UmbState {
	propertyType: {
		unique: string;
	};
}

export class UmbPropertyWriteStateManager extends UmbStateManager<UmbPropertyWriteState> {}
