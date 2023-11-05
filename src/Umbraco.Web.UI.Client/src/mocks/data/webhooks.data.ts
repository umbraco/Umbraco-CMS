import { UmbData } from './data.js';
//import {
//	WebhooksResponseModel,
//} from '@umbraco-cms/backoffice/backend-api';

class UmbWebhooksData extends UmbData<any> {
	constructor(data: any[]) {
		super(data);
	}

}

export const umbWebhooksData = {
	//webhooks: new UmbWebhooksData(webhooks),
};
