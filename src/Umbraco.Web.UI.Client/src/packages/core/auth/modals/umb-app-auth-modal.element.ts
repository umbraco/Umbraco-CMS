import { UmbModalBaseElement } from '../../modal/index.js';
import type { UmbModalAppAuthConfig, UmbModalAppAuthValue } from './umb-app-auth-modal.token.js';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-app-auth-modal')
export class UmbAppAuthModalElement extends UmbModalBaseElement<UmbModalAppAuthConfig, UmbModalAppAuthValue> {
	get props() {
		return {
			userLoginState: this.data?.userLoginState ?? 'loggingIn',
			onSubmit: this.onSubmit,
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
			<div id="layout">
				<div id="graphics" aria-hidden="true">
					<img id="logo" alt="logo" src="/umbraco/backoffice/assets/umbraco_logo_white.svg" />
				</div>
				<umb-body-layout id="login-layout">
					<h1 id="greeting" slot="header">${this.headline}</h1>
					${this.data?.userLoginState === 'timedOut'
						? html`<p style="margin-top:0">${this.localize.term('login_timeout')}</p>`
						: ''}
					<umb-extension-slot
						id="providers"
						type="authProvider"
						default-element="umb-auth-provider-default"
						.props=${this.props}></umb-extension-slot>
				</umb-body-layout>
			</div>
		`;
	}

	private onSubmit = (providerName: string) => {
		this.value = { providerName };
		this._submitModal();
	};

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				padding: 20px;
				width: 800px;
				max-width: 80vw;

				--umb-body-layout-color-background: #fff;
				--umb-login-image: url('/umbraco/backoffice/assets/login.jpg') center center / cover no-repeat;
			}

			#layout {
				display: flex;
				flex-direction: row;
				height: 100%;
				gap: var(--uui-size-space-5);
				container-type: inline-size;
				container-name: umb-app-auth-modal;
			}

			#graphics {
				position: relative;
				align-self: center;
				text-align: center;
				background: var(--umb-login-image);
				width: 400px;
				height: 327px;
			}

			#logo {
				position: absolute;
				top: 20px;
				left: 20px;
				width: calc(100% / 3);
			}

			#login-layout {
				max-width: 380px;
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

			@container umb-app-auth-modal (width < 568px) {
				#graphics {
					display: none;
				}

				#login-layout {
					max-width: none;
					min-height: none;
				}

				#greeting {
					text-align: center;
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
