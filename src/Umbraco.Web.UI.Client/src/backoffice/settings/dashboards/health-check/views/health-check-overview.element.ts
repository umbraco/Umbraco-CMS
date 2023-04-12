import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UUIButtonState } from '@umbraco-ui/uui';

import {
	UmbHealthCheckDashboardContext,
	UMB_HEALTHCHECK_DASHBOARD_CONTEXT_TOKEN,
} from '../health-check-dashboard.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './health-check-group-box-overview.element';

@customElement('umb-dashboard-health-check-overview')
export class UmbDashboardHealthCheckOverviewElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
			}

			uui-box + uui-box {
				margin-top: var(--uui-size-space-5);
			}

			.flex {
				display: flex;
				justify-content: space-between;
				align-items:center;
			}

			.grid {
				display: grid;
				gap: var(--uui-size-space-4);
				grid-template-columns: repeat(auto-fit, minmax(250px, auto));
			}
		`,
	];

	@state()
	private _buttonState: UUIButtonState;

	private _healthCheckDashboardContext?: UmbHealthCheckDashboardContext;

	constructor() {
		super();
		this.consumeContext(UMB_HEALTHCHECK_DASHBOARD_CONTEXT_TOKEN, (instance) => {
			this._healthCheckDashboardContext = instance;
		});
	}

	private async _onHealthCheckHandler() {
		this._healthCheckDashboardContext?.checkAll();
	}

	render() {
		return html`
			<uui-box>
				<div slot="headline" class="flex">
					Health Check
					<uui-button
						label="Perform all checks"
						color="positive"
						look="primary"
						.state="${this._buttonState}"
						@click="${this._onHealthCheckHandler}">
						Perform all checks
					</uui-button>
				</div>
				<div class="grid">
					<umb-extension-slot type="healthCheck" default-element="umb-health-check-group-box-overview">
					</umb-extension-slot>
				</div>
			</uui-box>
		`;
	}
}

export default UmbDashboardHealthCheckOverviewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-health-check-overview': UmbDashboardHealthCheckOverviewElement;
	}
}
