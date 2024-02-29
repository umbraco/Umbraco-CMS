import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

// TODO: Revisit if constructor method should be omitted
export abstract class UmbRepositoryBase extends UmbControllerBase {
	constructor(host: UmbControllerHost) {
		super(host);
	}
}
