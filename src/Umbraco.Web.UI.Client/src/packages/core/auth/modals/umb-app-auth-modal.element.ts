import { UmbModalBaseElement } from '../../modal/index.js';
import { UmbTextStyles } from '../../style/text-style.style.js';
import type { UmbModalAppAuthConfig, UmbModalAppAuthValue } from './umb-app-auth-modal.token.js';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';

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
				<img
					id="logo-on-background"
					src="/umbraco/backoffice/assets/umbraco_logo_blue.svg"
					alt="Logo"
					aria-hidden="true"
					part="auth-logo-background" />
				<div id="graphic" aria-hidden="true">
					<img part="auth-logo" id="logo-on-image" src="/umbraco/backoffice/assets/umbraco_logo_white.svg" alt="Logo" />
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
						${this.data?.userLoginState === 'timedOut'
							? html`<p style="margin-top:0">${this.localize.term('login_timeout')}</p>`
							: ''}
						<umb-extension-slot
							id="providers"
							type="authProvider"
							default-element="umb-auth-provider-default"
							.props=${this.props}></umb-extension-slot>
					</div>
				</div>
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
				background: rgb(244, 244, 244);

				--image: url('https://picsum.photos/800/600') no-repeat center center/cover;
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
				top: 0px;
				right: 0px;
			}

			#curve-bottom {
				bottom: 0px;
				left: 0px;
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
