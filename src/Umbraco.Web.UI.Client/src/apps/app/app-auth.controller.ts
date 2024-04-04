import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbAppAuthController extends UmbControllerBase {
	async makeAuthorizationRequest() {
		const authContext = await this.getContext(UMB_AUTH_CONTEXT);
		return authContext.makeAuthorizationRequest();
	}
}
