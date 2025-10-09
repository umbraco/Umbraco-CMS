import type { UmbContext } from './context.interface.js';
import { UmbControllerBase } from './controller-base.class.js';
import type { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * This base provides the necessary for a class to become a context-api controller.
 *
 */
export abstract class UmbContextBase extends UmbControllerBase implements UmbContext {
	constructor(host: UmbControllerHost, contextToken: UmbContextToken<any> | string) {
		super(host, contextToken.toString());
		this.provideContext(contextToken, this as any);
	}
}
