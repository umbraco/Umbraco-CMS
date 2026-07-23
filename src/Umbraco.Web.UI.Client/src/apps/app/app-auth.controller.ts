import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbAppAuthController extends UmbControllerBase {
	#retrievedContext: Promise<unknown>;
	#authContext?: typeof UMB_AUTH_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#retrievedContext = this.consumeContext(UMB_AUTH_CONTEXT, (context) => {
			this.#authContext = context;

			// If the session times out mid-use, send the user to the login screen.
			this.observe(context?.timeoutSignal, () => this.#redirectToLogin(), '_authState');
		}).asPromise({ preventTimeout: true });
	}

	/**
	 * Checks if the user is authorized; if not, redirects to the server login screen.
	 * Session verification is handled by setInitialState() (the current-user/configuration
	 * cookie probe) before the router evaluates guards.
	 */
	async isAuthorized(): Promise<boolean> {
		await this.#retrievedContext.catch(() => undefined);
		if (!this.#authContext) {
			throw new Error('[Fatal] Auth context is not available');
		}

		if (this.#authContext.getIsAuthorized()) {
			return true;
		}

		this.#redirectToLogin();
		return false;
	}

	/**
	 * Cookie auth: authentication happens on the server-rendered login screen. Redirect there;
	 * on success it sets the session cookie and returns to the backoffice, where the boot probe
	 * then sees an authenticated session.
	 */
	#redirectToLogin() {
		if (!this.#authContext) {
			throw new Error('[Fatal] Auth context is not available');
		}
		// Stay put on the logout landing: a just-logged-out user is unauthorized, but bouncing them
		// to the login screen here would capture /logout as the ReturnUrl and loop them straight back
		// into logging out after they sign in again.
		const logoutPath = new URL(this.#authContext.getPostLogoutRedirectUrl()).pathname;
		if (window.location.pathname === logoutPath) {
			return;
		}
		const loginUrl = new URL(`${this.#authContext.getServerUrl()}/umbraco/login`);
		// Preserve where the user was (potentially deep in an editor) so the login screen returns
		// them there afterwards. A relative path only — the server rejects non-local URLs. Skip a
		// bare "/": the server already defaults to the backoffice root, so ?ReturnUrl=%2F is just noise.
		const returnUrl = window.location.pathname + window.location.search;
		if (returnUrl !== '/') {
			loginUrl.searchParams.set('ReturnUrl', returnUrl);
		}
		location.href = loginUrl.href;
	}
}
