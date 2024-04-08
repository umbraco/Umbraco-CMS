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

	render() {
		return html`
			<div id="layout">
				<div id="graphics" aria-hidden="true">
					<img id="logo" alt="logo" src="umbraco_logo_white.svg" />
				</div>
				<umb-body-layout id="login-layout" .headline=${this.localize.term('login_instruction')}>
					${this.data?.userLoginState === 'timedOut' ? html`<p>${this.localize.term('login_timeout')}</p>` : ''}
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
				--umb-login-image: url('login.jpg') center center / cover no-repeat;
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
