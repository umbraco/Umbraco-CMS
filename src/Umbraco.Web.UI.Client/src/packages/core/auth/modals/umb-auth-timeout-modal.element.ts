import type { UmbModalAuthTimeoutConfig } from './umb-auth-timeout-modal.token.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-auth-timeout-modal')
export class UmbAuthTimeoutModalElement extends UmbModalBaseElement<UmbModalAuthTimeoutConfig, never> {
	@state()
	private _remainingTimeInSeconds = 60;

	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	#interval?: any;

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
				this.#handleLogout();
			}
		}, 1000);
	}

	#handleLogout() {
		this.data?.onLogout?.();
		this.modalContext?.submit();
	}

	#handleConfirm() {
		this.data?.onContinue?.();
		this.modalContext?.submit();
	}

	override render() {
		return html`
			<uui-dialog-layout class="uui-text" .headline=${this.localize.term('timeout_warningHeadline')}>
				<umb-localize key="timeout_warningText" .args=${[this._remainingTimeInSeconds]}>
					Your session is about to expire and you will be logged out in
					<strong>${this._remainingTimeInSeconds} seconds</strong>.
				</umb-localize>

				<uui-button
					slot="actions"
					look="secondary"
					label=${this.localize.term('timeout_warningLogoutAction')}
					@click=${this.#handleLogout}></uui-button>
				<uui-button
					slot="actions"
					id="confirm"
					color="positive"
					look="primary"
					label=${this.localize.term('timeout_warningContinueAction')}
					@click=${this.#handleConfirm}
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
