import { UmbMockDBBase } from './utils/mock-db-base.js';
//import {
//	WebhooksResponseModel,
//} from '@umbraco-cms/backoffice/backend-api';

class UmbWebhooksData extends UmbMockDBBase<any> {
	constructor(data: any[]) {
		super(data);
	}
}

export const umbWebhooksData = {
	//webhooks: new UmbWebhooksData(webhooks),
};
