import { UmbBaseController, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbRepositoryBase extends UmbBaseController {
	constructor(host: UmbControllerHost) {
		super(host);
	}
}
