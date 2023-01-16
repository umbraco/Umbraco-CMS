import { UUIButtonState } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';

import { UmbHealthCheckContext } from '../health-check.context';
import { UmbHealthCheckDashboardContext } from '../health-check-dashboard.context';
import {
	HealthCheckAction,
	HealthCheckGroupWithResult,
	HealthCheckResource,
	HealthCheckWithResult,
	StatusResultType,
} from '@umbraco-cms/backend-api';
import { UmbLitElement } from '@umbraco-cms/element';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';

@customElement('umb-dashboard-health-check-action')
export class UmbDashboardHealthCheckActionElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			.action {
				width: 100%;
				display: flex;
				gap: var(--uui-size-space-4);
				background-color: #eee;
				align-items: center;
			}
			.action uui-button {
				margin: 0;
				padding: 0;
				flex-shrink: 1;
			}

			.no-description {
				color: var(--uui-color-border-emphasis);
				font-style: italic;
			}
		`,
	];

	@property({ reflect: true })
	action!: HealthCheckAction;

	@state()
	private _buttonState?: UUIButtonState;

	private async _onActionClick(e: SubmitEvent) {
		e.preventDefault();
		this._buttonState = 'waiting';
		const { error } = await tryExecuteAndNotify(
			this,
			HealthCheckResource.postHealthCheckExecuteAction({ requestBody: this.action })
		);

		if (error) {
			this._buttonState = 'failed';
			return;
		}

		this._buttonState = 'success';
		this.dispatchEvent(new CustomEvent('action-executed'));
	}

	render() {
		return html` <div class="action">
			<uui-form>
				<form @submit=${(e: SubmitEvent) => this._onActionClick(e)}>
					${this._renderValueRequired()}
					<br />
					<uui-button
						type="submit"
						look="primary"
						color="positive"
						label="${this.action.name || 'Action'}"
						.state=${this._buttonState}>
						${this.action.name || 'Action'}
					</uui-button>
				</form>
			</uui-form>
			<p>${this.action.description || html`<span class="no-description">This action has no description</span>`}</p>
		</div>`;
	}

	private _renderValueRequired() {
		if (this.action.valueRequired) {
			switch (this.action.providedValueValidation) {
				case 'email':
					return html`<uui-input
						type="email"
						@input=${(e: any) => (this.action.providedValue = e.target.value)}
						placeholder="Value"
						.value=${this.action.providedValue ?? ''}
						required></uui-input>`;

				case 'regex':
					return html`<uui-input
						type="text"
						pattern="${ifDefined(this.action.providedValueValidationRegex ?? undefined)}"
						@input=${(e: any) => (this.action.providedValue = e.target.value)}
						placeholder="Value"
						.value=${this.action.providedValue ?? ''}
						required></uui-input>`;

				default:
					return html`<uui-input
						type="text"
						@input=${(e: any) => (this.action.providedValue = e.target.value)}
						placeholder="Value"
						.value=${this.action.providedValue ?? ''}
						required></uui-input>`;
			}
		}

		return nothing;
	}
}

export default UmbDashboardHealthCheckActionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-health-check-action': UmbDashboardHealthCheckActionElement;
	}
}
