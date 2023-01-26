import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UUIButtonState } from '@umbraco-ui/uui';

import { UmbHealthCheckDashboardContext } from '../health-check-dashboard.context';
import { UmbLitElement } from '@umbraco-cms/element';

import { ManifestHealthCheck } from '@umbraco-cms/extensions-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-api';

import './health-check-group-box-overview.element';

@customElement('umb-dashboard-health-check-overview')
export class UmbDashboardHealthCheckOverviewElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			uui-box + uui-box {
				margin-top: var(--uui-size-space-5);
			}

			.flex {
				display: flex;
				justify-content: space-between;
			}
		`,
	];

	@state()
	private _buttonState: UUIButtonState;

	private _healthCheckManifests: ManifestHealthCheck[] = [];

	private _healthCheckDashboardContext?: UmbHealthCheckDashboardContext;

	constructor() {
		super();
		this.consumeContext('umbHealthCheckDashboard', (instance: UmbHealthCheckDashboardContext) => {
			this._healthCheckDashboardContext = instance;
		});
	}

	connectedCallback(): void {
		super.connectedCallback();
		umbExtensionsRegistry.extensionsOfType('healthCheck').subscribe((healthChecks) => {
			this._healthCheckManifests = healthChecks;
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
				<!--//TODO:  wrap extension container in a grid wrapper -->
				<umb-extension-slot
					type="healthCheck"
					default-element="umb-health-check-group-box-overview"></umb-extension-slot>
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
