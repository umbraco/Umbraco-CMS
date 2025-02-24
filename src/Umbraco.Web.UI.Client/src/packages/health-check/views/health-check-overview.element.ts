import type { UmbHealthCheckDashboardContext } from '../health-check-dashboard.context.js';
import { UMB_HEALTHCHECK_DASHBOARD_CONTEXT } from '../health-check-dashboard.context.js';
import type { UUIButtonState } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';

import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import './health-check-group-box-overview.element.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-dashboard-health-check-overview')
export class UmbDashboardHealthCheckOverviewElement extends UmbLitElement {
	@state()
	private _buttonState: UUIButtonState;

	private _healthCheckDashboardContext?: UmbHealthCheckDashboardContext;

	constructor() {
		super();
		this.consumeContext(UMB_HEALTHCHECK_DASHBOARD_CONTEXT, (instance) => {
			this._healthCheckDashboardContext = instance;
		});
	}

	private async _onHealthCheckHandler() {
		this._buttonState = 'waiting';
		await this._healthCheckDashboardContext?.checkAll();
		this._buttonState = 'success';
	}

	override render() {
		return html`
			<uui-box>
				<div id="header" slot="header">
					<h2>Health Check</h2>
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

					${
						// As well as the visual presentation, this amend to the rendering based on button state is necessary
						// in order to trigger an update after the checks are complete (this.requestUpdate() doesn't suffice).
						this._buttonState !== 'waiting'
							? html`<umb-extension-slot
									type="healthCheck"
									default-element="umb-health-check-group-box-overview"></umb-extension-slot>`
							: html`<uui-loader></uui-loader>`
					}
					</umb-extension-slot>
				</div>
			</uui-box>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				padding: var(--uui-size-layout-1);
			}

			uui-box + uui-box {
				margin-top: var(--uui-size-space-5);
			}

			#header {
				width: 100%;
				display: flex;
				justify-content: space-between;
				align-items: center;
			}

			#header h2 {
				font-size: var(--uui-type-h5-size);
				margin: 0;
			}

			.grid {
				display: grid;
				gap: var(--uui-size-space-4);
				grid-template-columns: repeat(auto-fit, minmax(250px, auto));
			}
		`,
	];
}

export default UmbDashboardHealthCheckOverviewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-health-check-overview': UmbDashboardHealthCheckOverviewElement;
	}
}
