import type { UmbContextToken } from '../context-api/token/context-token.js';
import type { UmbControllerHost } from './controller-host.interface.js';
import { UmbBaseController } from './controller.class.js';

/**
 * This base provides the necessary for a  class to become a context-api controller.
 *
 */
export abstract class UmbContextBase<
	ContextType,
	GivenContextToken extends UmbContextToken<any, ContextType> = UmbContextToken<any, ContextType>,
> extends UmbBaseController {
	constructor(host: UmbControllerHost, contextToken: GivenContextToken | string) {
		super(host, contextToken.toString());
		this.provideContext(contextToken, this as unknown as ContextType);
	}
}
