import type { ManifestAuthProvider } from '../auth-provider.extension.js';
import { UmbModalBaseElement } from '../../modal/index.js';
import { UmbTextStyles } from '../../style/text-style.style.js';
import { UMB_AUTH_CONTEXT } from '../auth.context.token.js';
import type { UmbAuthProviderDefaultProps } from '../types.js';
import type { UmbModalAppAuthConfig, UmbModalAppAuthValue } from './umb-app-auth-modal.token.js';
import { css, customElement, html, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UMB_SERVER_CONTEXT } from '@umbraco-cms/backoffice/server';

@customElement('umb-app-auth-modal')
export class UmbAppAuthModalElement extends UmbModalBaseElement<UmbModalAppAuthConfig, UmbModalAppAuthValue> {
	@state()
	private _error?: string;

	@state()
	private _serverUrl = '';

	@state()
	private _loading = true;

	@state()
	private _allowLocalLogin = false;

	get props(): UmbAuthProviderDefaultProps {
		return {
			userLoginState: this.data?.userLoginState ?? 'loggingIn',
			onSubmit: this.onSubmit.bind(this),
		};
	}

	get headline() {
		return this.data?.userLoginState === 'timedOut'
			? this.localize.term('login_instruction')
			: this.localize.term(
					[
						'login_greeting0',
						'login_greeting1',
						'login_greeting2',
						'login_greeting3',
						'login_greeting4',
						'login_greeting5',
						'login_greeting6',
					][new Date().getDay()],
				);
	}

	override firstUpdated(): void {
		this.consumeContext(UMB_SERVER_CONTEXT, (context) => {
			this._serverUrl = context?.getServerUrl() ?? '';
			this.style.setProperty(
				'--image',
				`url('${this._serverUrl}/umbraco/management/api/v1/security/back-office/graphics/login-background') no-repeat center center/cover`,
			);

			const serverConnection = context?.getServerConnection();

			this.observe(serverConnection?.allowLocalLogin, (allowLocalLogin) => {
				this._allowLocalLogin = allowLocalLogin ?? false;
			});

			this.observe(serverConnection?.isConnected, (isConnected) => {
				this._loading = !isConnected;
			});
		});
	}

	override render() {
		return html`
			<div id="layout">
				<img
					id="logo-on-background"
					src="${this._serverUrl}/umbraco/management/api/v1/security/back-office/graphics/login-logo-alternative"
					alt="Logo"
					aria-hidden="true"
					part="auth-logo-background" />
				<div id="graphic" aria-hidden="true">
					<img
						part="auth-logo"
						id="logo-on-image"
						src="${this._serverUrl}/umbraco/management/api/v1/security/back-office/graphics/login-logo"
						alt="Logo" />
					<svg
						id="curve-top"
						width="1746"
						height="1374"
						viewBox="0 0 1746 1374"
						fill="none"
						xmlns="http://www.w3.org/2000/svg">
						<path d="M8 1C61.5 722.5 206.5 1366.5 1745.5 1366.5" stroke="currentColor" stroke-width="15"></path>
					</svg>
					<svg
						id="curve-bottom"
						width="1364"
						height="552"
						viewBox="0 0 1364 552"
						fill="none"
						xmlns="http://www.w3.org/2000/svg">
						<path d="M1 8C387 24 1109 11 1357 548" stroke="currentColor" stroke-width="15"></path>
					</svg>
				</div>
				<div id="content-container">
					<div id="content">
						<header id="header">
							<h1 id="greeting">${this.headline}</h1>
						</header>
						${this._error ? html`<p style="margin-top:0;color:red">${this._error}</p>` : ''}
						${this.data?.userLoginState === 'timedOut'
							? html`<p style="margin-top:0">${this.localize.term('login_timeout')}</p>`
							: ''}
						${when(
							this._loading,
							() => html`
								<div id="loader">
									<uui-loader></uui-loader>
								</div>
							`,
							() =>
								html` <umb-extension-slot
									id="providers"
									type="authProvider"
									default-element="umb-auth-provider-default"
									.props=${this.props}
									.filter=${this.#filterProvider}></umb-extension-slot>`,
						)}
					</div>
				</div>
			</div>
		`;
	}

	#filterProvider = (provider: ManifestAuthProvider) => {
		if (this._allowLocalLogin) {
			return true;
		}

		// Do not show any Umbraco auth provider if local login is disabled
		return provider.forProviderName.toLowerCase() !== 'umbraco';
	};

	private onSubmit = async (providerOrManifest: string | ManifestAuthProvider, loginHint?: string) => {
		try {
			const authContext = await this.getContext(UMB_AUTH_CONTEXT);
			if (!authContext) {
				throw new Error('Auth context not available');
			}

			const manifest = typeof providerOrManifest === 'string' ? undefined : providerOrManifest;
			const providerName =
				typeof providerOrManifest === 'string' ? providerOrManifest : providerOrManifest.forProviderName;

			// If the user is timed out, we do not want to lose the state, so avoid redirecting to the provider
			// and instead just make the authorization request. In all other cases, we want to redirect to the provider.
			const isTimedOut = this.data?.userLoginState === 'timedOut';

			await authContext.makeAuthorizationRequest(providerName, isTimedOut ? false : true, loginHint, manifest);

			const isAuthed = authContext.getIsAuthorized();
			this.value = { success: isAuthed };
			if (isAuthed) {
				this._submitModal();
			}
		} catch (error) {
			console.error('[AuthModal] Error submitting auth request', error);
			this._error = error instanceof Error ? error.message : 'Unknown error (see console)';
		}
	};

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				background: var(--uui-color-surface, #f4f4f4);

				--curves-color: var(--umb-login-curves-color, #f5c1bc);
				--curves-display: var(--umb-login-curves-display, inline);
			}

			#layout {
				display: flex;
				justify-content: center;
				padding: 32px 0 32px 32px;
				width: 100vw;
				max-width: 1920px;
				height: calc(100vh - 64px);
			}

			#graphic {
				position: relative;
				width: 100%;
				height: 100%;
				background: var(--umb-login-image, var(--image));
				border-radius: var(--umb-login-image-border-radius, 38px);
				position: relative;
				overflow: hidden;
				color: var(--curves-color);
			}

			#graphic svg {
				position: absolute;
				width: 45%;
				height: fit-content;
				display: var(--curves-display);
			}

			#curve-top {
				top: -9%;
				right: -9%;
			}

			#curve-bottom {
				bottom: -1px;
				left: -1px;
			}

			#content-container {
				background: var(--umb-login-content-background, none);
				display: var(--umb-login-content-display, flex);
				width: var(--umb-login-content-width, 100%);
				height: var(--umb-login-content-height, 100%);
				overflow: auto;
				border-radius: var(--umb-login-content-border-radius, 0);
				padding: 16px;
				margin: auto;
			}

			#content {
				max-width: 360px;
				margin: auto;
				width: 100%;
			}

			#logo-on-background {
				display: none;
			}

			#logo-on-image,
			#logo-on-background {
				position: absolute;
				top: 24px;
				left: 24px;
				height: 55px;
			}

			#header {
				text-align: center;
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-5);
			}

			#greeting {
				color: var(--umb-login-greeting-color, var(--uui-color-interactive-emphasis));
				text-align: center;
				font-weight: 400;
				font-size: var(--umb-login-header-font-size-large, 4rem);
				margin: 0 0 var(--uui-size-layout-1);
				line-height: 1.2;
			}

			#providers {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-5);
			}

			#loader {
				display: flex;
				justify-content: center;
				align-items: center;
			}

			@media (max-width: 900px) {
				#graphic {
					display: none;
				}
				#logo-on-background {
					display: block;
				}
			}
		`,
	];
}

export default UmbAppAuthModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-app-auth-modal': UmbAppAuthModalElement;
	}
}
