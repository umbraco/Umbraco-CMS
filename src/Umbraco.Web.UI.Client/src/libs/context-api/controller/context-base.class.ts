import { type UmbContextToken } from '../token/context-token.js';
import { UmbBaseController, type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * This mixin enables a web-component to host controllers.
 * This enables controllers to be added to the life cycle of this element.
 *
 */
export abstract class UmbContextBase extends UmbBaseController {
	constructor(host: UmbControllerHost, contextToken: UmbContextToken | string) {
		super(host, contextToken.toString());
		this.provideContext(contextToken, this);
	}
}
