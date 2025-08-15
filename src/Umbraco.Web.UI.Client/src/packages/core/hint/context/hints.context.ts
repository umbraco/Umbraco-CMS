import { UMB_HINT_CONTEXT } from './hint.context-token.js';
import { UmbHintController } from './hints.controller.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbHintContext extends UmbHintController {
	constructor(host: UmbControllerHost) {
		super(host);
		this.provideContext(UMB_HINT_CONTEXT, this);
	}
}
