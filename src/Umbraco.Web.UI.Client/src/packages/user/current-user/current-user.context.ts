import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbBaseController, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbCurrentUserContext extends UmbBaseController {
	constructor(host: UmbControllerHost) {
		super(host);
		this.provideContext(UMB_CURRENT_USER_CONTEXT, this);
	}
}

export const UMB_CURRENT_USER_CONTEXT = new UmbContextToken<UmbCurrentUserContext>('UmbCurrentUserContext');
