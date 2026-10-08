import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { UMB_SERVER_CONTEXT } from '@umbraco-cms/backoffice/server';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { ensureLocalPath } from '@umbraco-cms/backoffice/utils';

/**
 * Lander for the external-login popup flow (`{BackOfficeHost}/auth-callback`, hardcoded server-side in
 * `BackOfficeController.ExternalLoginSuccessRedirectUrl`). The server callback has already set the auth
 * cookie and redirected here; the boot probe + `authorized` broadcast also already ran (`auth-callback`
 * isn't in `UmbAppElement#setup`'s skip list). So this element only waits for `isAuthorized` to settle:
 * on success it closes — no `window.opener`/`postMessage`, since the same-origin broadcast already
 * reached the opener — and on failure it reports, rather than closing on a session that never arrived.
 */
@customElement('umb-app-auth-callback')
export class UmbAppAuthCallbackElement extends UmbLitElement {
	@state()
	private _failed = false;

	#backofficePath = '/umbraco';

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
						this.#close();
						return;
					}

					// The boot probe has already run and resolved by the time the router mounts this
					// element, so an unauthorized state here means the sign-in produced no session.
					// Say so instead of closing: a silent close leaves the opener waiting forever for
					// an `authorized` broadcast that is never coming.
					this._failed = true;
				},
				'observeIsAuthorized',
			);
		});
	}

	#close(): void {
		window.close();

		// window.close() is a silent no-op when the browser refuses it — e.g. this route loaded in an
		// ordinary tab (the full-page redirect flow) rather than the script-opened popup. Fall back to
		// the server-carried returnUrl (re-validated local), else the backoffice root — never leave a
		// bare loader up indefinitely.
		setTimeout(() => {
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
