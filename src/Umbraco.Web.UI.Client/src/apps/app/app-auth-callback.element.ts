import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { UMB_SERVER_CONTEXT } from '@umbraco-cms/backoffice/server';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * Lander for the external-login popup flow (`{BackOfficeHost}/auth-callback`, hardcoded server-side
 * in `BackOfficeController.ExternalLoginSuccessRedirectUrl`). The server callback has already set the
 * auth cookie and redirected the popup here.
 *
 * `UmbAppElement#setup` probes the server and broadcasts `authorized` on the `umb:auth`
 * BroadcastChannel for every route except `/logout` and `/error` — `auth-callback` is not in that
 * skip list, so the probe (and broadcast) already ran and resolved before the router even mounts this
 * element. This element only has to wait for `isAuthorized` to settle and then close the popup; it
 * does not trigger the probe itself. Same-origin BroadcastChannel already reached the opener, so no
 * `window.opener`/`postMessage` is used here.
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

		// window.close() is a silent no-op when the browser refuses it (e.g. this route loaded in an
		// ordinary tab rather than the script-opened external-login popup). Fall back to landing on
		// the backoffice root instead of leaving a bare loader up indefinitely.
		setTimeout(() => {
			if (!window.closed) {
				window.location.href = this.#backofficePath;
			}
		}, 300);
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
