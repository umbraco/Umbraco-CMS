import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';

// TODO: Revisit if constructor method should be omitted
export abstract class UmbRepositoryBase extends UmbBaseController {
	constructor(host: UmbControllerHost) {
		super(host);
	}
}
