import type { UmbContextToken } from '../context-api/index.js';
import type { UmbControllerHost } from '../controller-api/index.js';
import { UmbBaseController } from './controller-base.class.js';

/**
 * This base provides the necessary for a class to become a context-api controller.
 *
 */
export abstract class UmbContextBase<
	ContextType,
	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	GivenContextToken extends UmbContextToken<any, ContextType> = UmbContextToken<any, ContextType>,
> extends UmbBaseController {
	constructor(host: UmbControllerHost, contextToken: GivenContextToken | string) {
		super(host, contextToken.toString());
		this.provideContext(contextToken, this as unknown as ContextType);
	}
}
