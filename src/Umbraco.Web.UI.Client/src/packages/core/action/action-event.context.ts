import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbActionEventContext extends UmbContextBase<UmbActionEventContext> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_ACTION_EVENT_CONTEXT);
	}
}

export const UMB_ACTION_EVENT_CONTEXT = new UmbContextToken<UmbActionEventContext, UmbActionEventContext>(
	'UmbActionEventContext',
);
