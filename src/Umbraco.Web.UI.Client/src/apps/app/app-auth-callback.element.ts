import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { UMB_SERVER_CONTEXT } from '@umbraco-cms/backoffice/server';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { ensureLocalPath } from '@umbraco-cms/backoffice/utils';

/**
 * Lander for external logins and the sign-in popup (`{BackOfficeHost}/auth-callback`, hardcoded server-side
 * in `BackOfficeController.ExternalLoginSuccessRedirectUrl`). The server callback has already set the auth
 * cookie and redirected here; the boot probe + `authorized` broadcast also already ran (`auth-callback`
 * isn't in `UmbAppElement#setup`'s skip list). So this element only waits for `isAuthorized` to settle:
 * on success it returns to the backoffice, and on failure it reports, rather than returning on a session
 * that never arrived. It never closes its own window: a script may close any tab another page opened, and
 * only the window that opened a sign-in popup can tell it is one, so that window closes it.
 */
@customElement('umb-app-auth-callback')
export class UmbAppAuthCallbackElement extends UmbLitElement {
	@state()
	private _failed = false;

	#backofficePath = '/umbraco';
	#returnTimeout?: ReturnType<typeof setTimeout>;

	constructor() {
		super();

		this.consumeContext(UMB_SERVER_CONTEXT, (context) => {
			this.#backofficePath = context?.getBackofficePath() ?? this.#backofficePath;
		});

		this.consumeContext(UMB_AUTH_CONTEXT, (authContext) => {
			this.observe(
				authContext?.isAuthorized,
				(isAuthorized) => {
					if (isAuthorized) {
						this.#return();
						return;
					}

					// The boot probe has already run and resolved by the time the router mounts this
					// element, so an unauthorized state here means the sign-in produced no session.
					// Say so instead of returning: the backoffice would only send the user back to sign in.
					this._failed = true;
				},
				'observeIsAuthorized',
			);
		});
	}

	override disconnectedCallback(): void {
		super.disconnectedCallback();
		clearTimeout(this.#returnTimeout);
	}

	#return(): void {
		// Give the window that opened a sign-in popup time to close it, before loading the backoffice in it.
		// Return to the server-carried returnUrl (re-validated local), else the backoffice root.
		this.#returnTimeout = setTimeout(() => {
			if (!window.closed) {
				window.location.href = this.#fallbackUrl();
			}
		}, 300);
	}

	#fallbackUrl(): string {
		const returnUrl = new URLSearchParams(window.location.search).get('returnUrl');
		// Re-validate the server-carried returnUrl against this origin, mirroring the server's own
		// Url.IsLocalUrl guard, so a tampered value can't redirect off-site.
		const backofficeUrl = new URL(this.#backofficePath, window.location.origin);
		return ensureLocalPath(returnUrl ?? backofficeUrl, backofficeUrl).href;
	}

	override render() {
		if (this._failed) {
			return html`<div id="message">
				<p>${this.localize.term('errors_externalLoginFailed')}</p>
				<uui-button
					look="primary"
					href=${this.#fallbackUrl()}
					label=${this.localize.term('login_returnToLogin')}></uui-button>
			</div>`;
		}
		return html`<div id="loader"><uui-loader></uui-loader></div>`;
	}

	static override readonly styles = css`
		:host {
			display: block;
			height: 100vh;
		}
		#loader,
		#message {
			display: flex;
			height: 100%;
			flex-direction: column;
			gap: var(--uui-size-space-4);
			justify-content: center;
			align-items: center;
		}
	`;
}

export default UmbAppAuthCallbackElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-app-auth-callback': UmbAppAuthCallbackElement;
	}
}
