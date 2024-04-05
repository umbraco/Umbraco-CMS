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
			<umb-body-layout id="layout" .headline=${this.localize.term('login_instruction')}>
				<umb-extension-slot
					id="providers"
					type="authProvider"
					default-element="umb-auth-provider-default"
					.props=${this.props}></umb-extension-slot>
			</umb-body-layout>
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
				width: 380px;
				max-width: 80vw;
			}

			#layout {
				--umb-body-layout-color-background: #fff;
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
