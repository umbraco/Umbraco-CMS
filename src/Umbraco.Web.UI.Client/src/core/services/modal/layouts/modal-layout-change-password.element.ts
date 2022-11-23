import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbModalHandler, UmbModalService } from '@umbraco-cms/services';
import type { UserDetails } from '@umbraco-cms/models';
import { UmbUserStore } from 'src/core/stores/user/user.store';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';

@customElement('umb-modal-layout-change-password')
export class UmbModalLayoutChangePasswordElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			:host {
				display: block;
			}
			:host,
			umb-editor-entity-layout {
				width: 100%;
				height: 100%;
			}
			#main {
				padding: var(--uui-size-space-5);
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-3);
			}
		`,
	];

	@property({ attribute: false })
	modalHandler?: UmbModalHandler;

	private _close() {
		this.modalHandler?.close();
	}

	private _handleSubmit(e: Event) {
		e.preventDefault();
		console.log('IMPLEMENT SUBMIT');
		this._close();
	}

	render() {
		return html`
			<uui-dialog-layout class="uui-text" .headline=${this.data?.headline || null}>
				<uui-form>
					<form id="LoginForm" name="login" @submit="${this._handleSubmit}">
						<uui-form-layout-item>
							<uui-label id="oldPasswordLabel" for="oldPpassword" slot="label" required>Old password</uui-label>
							<uui-input-password
								id="oldPassword"
								name="oldPassword"
								required
								required-message="Old password is required"></uui-input-password>
						</uui-form-layout-item>
						<br />
						<uui-form-layout-item>
							<uui-label id="newPasswordLabel" for="newPassword" slot="label" required>New password</uui-label>
							<uui-input-password
								id="newPassword"
								name="newPassword"
								required
								required-message="New password is required"></uui-input-password>
						</uui-form-layout-item>
						<uui-form-layout-item>
							<uui-label id="confirmPasswordLabel" for="confirmPassword" slot="label" required
								>Confirm password</uui-label
							>
							<uui-input-password
								id="confirmPassword"
								name="confirmPassword"
								required
								required-message="Confirm password is required"></uui-input-password>
						</uui-form-layout-item>

						<uui-button @click=${this._close} label="Cancel" look="secondary"></uui-button>
						<uui-button type="submit" label="Confirm" look="primary" color="positive"></uui-button>
					</form>
				</uui-form>
			</uui-dialog-layout>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-modal-layout-change-password': UmbModalLayoutChangePasswordElement;
	}
}
