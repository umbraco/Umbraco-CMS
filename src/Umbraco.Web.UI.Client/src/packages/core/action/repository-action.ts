import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export abstract class UmbActionBase extends UmbControllerBase implements UmbApi {
	constructor(host: UmbControllerHost) {
		super(host);
	}
}
