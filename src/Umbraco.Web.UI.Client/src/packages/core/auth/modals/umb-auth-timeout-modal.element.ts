import type { UmbModalAuthTimeoutConfig } from './umb-auth-timeout-modal.token.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-auth-timeout-modal')
export class UmbAuthTimeoutModalElement extends UmbModalBaseElement<UmbModalAuthTimeoutConfig, never> {
	@state()
	private _remainingTimeInSeconds = 60;

	#interval?: NodeJS.Timeout;

	override connectedCallback() {
		super.connectedCallback();
		this.#startCountdown();
	}

	override disconnectedCallback() {
		super.disconnectedCallback();
		if (this.#interval) {
			clearInterval(this.#interval);
		}
	}

	#startCountdown() {
		this._remainingTimeInSeconds = this.data?.remainingTimeInSeconds ?? 60;
		this.#interval = setInterval(() => {
			if (this._remainingTimeInSeconds > 0) {
				this._remainingTimeInSeconds--;
			} else {
				clearInterval(this.#interval);
				this._handleLogout();
			}
		}, 1000);
	}

	private _handleLogout() {
		this.data?.onLogout?.();
		this.modalContext?.submit();
	}

	private _handleConfirm() {
		this.data?.onContinue?.();
		this.modalContext?.submit();
	}

	override render() {
		return html`
			<uui-dialog-layout class="uui-text" .headline=${this.localize.term('Session Timeout')}>
				<umb-localize key="auth_timeoutMessage" .args=${[this._remainingTimeInSeconds]}>
					You have been inactive and will be logged out in ${this._remainingTimeInSeconds} seconds.
				</umb-localize>

				<uui-button slot="actions" label=${this.localize.term('Log out')} @click=${this._handleLogout}></uui-button>
				<uui-button
					slot="actions"
					id="confirm"
					color="positive"
					look="primary"
					label=${this.localize.term('Stay logged in')}
					@click=${this._handleConfirm}
					${umbFocus()}></uui-button>
			</uui-dialog-layout>
		`;
	}

	static override readonly styles = [
		UmbTextStyles,
		css`
			uui-dialog-layout {
				max-inline-size: 60ch;
			}
		`,
	];
}

export default UmbAuthTimeoutModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-auth-timeout-modal': UmbAuthTimeoutModalElement;
	}
}
