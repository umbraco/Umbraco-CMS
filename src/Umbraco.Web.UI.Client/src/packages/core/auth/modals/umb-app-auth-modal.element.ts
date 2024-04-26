import type { ManifestAuthProvider } from '../../extension-registry/models/auth-provider.model.js';
import { UmbModalBaseElement } from '../../modal/index.js';
import { UmbTextStyles } from '../../style/text-style.style.js';
import { UMB_AUTH_CONTEXT } from '../auth.context.token.js';
import type { UmbAuthProviderDefaultProps } from '../types.js';
import type { UmbModalAppAuthConfig, UmbModalAppAuthValue } from './umb-app-auth-modal.token.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-app-auth-modal')
export class UmbAppAuthModalElement extends UmbModalBaseElement<UmbModalAppAuthConfig, UmbModalAppAuthValue> {
	@state()
	private _error?: string;

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

	render() {
		return html`
			<umb-body-layout id="login-layout">
				<h1 id="greeting" slot="header">${this.headline}</h1>
				${this.data?.userLoginState === 'timedOut'
					? html`<p style="margin-top:0">${this.localize.term('login_timeout')}</p>`
					: ''}
				${this._error ? html`<p style="margin-top:0;color:red">${this._error}</p>` : ''}
				<umb-extension-slot
					id="providers"
					type="authProvider"
					default-element="umb-auth-provider-default"
					.props=${this.props}></umb-extension-slot>
			</umb-body-layout>
		`;
	}

	private onSubmit = async (providerOrManifest: string | ManifestAuthProvider, loginHint?: string) => {
		const authContext = await this.getContext(UMB_AUTH_CONTEXT);
		const manifest = typeof providerOrManifest === 'string' ? undefined : providerOrManifest;
		const providerName =
			typeof providerOrManifest === 'string' ? providerOrManifest : providerOrManifest.forProviderName;
		await authContext.makeAuthorizationRequest(providerName, false, loginHint, manifest);

		const isAuthed = authContext.getIsAuthorized();
		this.value = { success: isAuthed };
		if (isAuthed) {
			this._submitModal();
		} else {
			this._error = 'Failed to authenticate';
		}
	};

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;

				--umb-body-layout-color-background: #fff;
			}

			#login-layout {
				width: 380px;
				max-width: 80vw;
				min-height: 327px;
			}

			#greeting {
				width: 100%;
				color: var(--umb-login-header-color, var(--uui-color-interactive));
				text-align: center;
				font-weight: 400;
				font-size: var(--umb-login-header-font-size, 2rem);
				line-height: 1.2;
				margin: 0;
			}

			#providers {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-5);
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
