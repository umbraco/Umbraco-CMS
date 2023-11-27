import { UmbBaseController, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

// TODO: Revisit if constructor method should be omitted
export abstract class UmbRepositoryBase extends UmbBaseController {
	constructor(host: UmbControllerHost) {
		super(host);
	}
}
