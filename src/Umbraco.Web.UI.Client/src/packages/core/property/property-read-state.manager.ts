import { UmbStateManager, type UmbState } from '@umbraco-cms/backoffice/utils';

export interface UmbPropertyReadState extends UmbState {
	propertyType: {
		unique: string;
	};
}

export class UmbPropertyReadStateManager extends UmbStateManager<UmbPropertyReadState> {}
