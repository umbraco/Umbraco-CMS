import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { UMB_SERVER_CONTEXT } from '@umbraco-cms/backoffice/server';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * Lander for the external-login popup flow (`{BackOfficeHost}/auth-callback`, hardcoded server-side in
 * `BackOfficeController.ExternalLoginSuccessRedirectUrl`). The server callback has already set the auth
 * cookie and redirected here; the boot probe + `authorized` broadcast also already ran (`auth-callback`
 * isn't in `UmbAppElement#setup`'s skip list). So this element only waits for `isAuthorized` to settle,
 * then closes — no `window.opener`/`postMessage`, since the same-origin broadcast already reached the opener.
 */
@customElement('umb-app-auth-callback')
export class UmbAppAuthCallbackElement extends UmbLitElement {
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
					if (isAuthorized === undefined) return;
					this.#close();
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
		// Accept only a local, relative path (mirrors the server's Url.IsLocalUrl guard): a single
		// leading slash, and neither a protocol-relative "//host" nor a "/\" backslash trick.
		if (returnUrl && returnUrl.startsWith('/') && !returnUrl.startsWith('//') && !returnUrl.startsWith('/\\')) {
			return returnUrl;
		}
		return this.#backofficePath;
	}

	override render() {
		return html`<div id="loader"><uui-loader></uui-loader></div>`;
	}

	static override styles = css`
		:host {
			display: block;
			height: 100vh;
		}
		#loader {
			display: flex;
			height: 100%;
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
