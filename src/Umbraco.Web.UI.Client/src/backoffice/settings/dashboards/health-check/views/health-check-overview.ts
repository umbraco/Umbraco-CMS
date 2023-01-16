import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';

import { UUIButtonState } from '@umbraco-ui/uui';
import { UmbModalService } from '../../../../../core/modal';

import { UmbLitElement } from '@umbraco-cms/element';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';

import { ManifestHealthCheck, umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';

import { HealthCheckGroup, HealthCheckGroupWithResult, HealthCheckResource } from '@umbraco-cms/backend-api';

import '../health-check-overview-group.element';
import { UmbHealthCheckDashboardContext } from '../health-check-dashboard.context';

@customElement('umb-dashboard-health-check-overview')
export class UmbDashboardHealthCheckOverviewElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			uui-box + uui-box {
				margin-top: var(--uui-size-space-5);
			}

			.group-wrapper {
				display: grid;
				gap: var(--uui-size-space-4);
				grid-template-columns: repeat(auto-fit, minmax(250px, auto));
			}

			.group-box {
				position: relative;
			}

			.group-box:hover::after {
				content: '';
				width: 100%;
				height: 100%;
				position: absolute;
				top: 0;
				bottom: 0;
				left: 0;
				right: 0;
				border-radius: var(--uui-border-radius);
				transition: opacity 100ms ease-out 0s;
				opacity: 0.33;
				outline-color: var(--uui-color-selected);
				outline-width: 4px;
				outline-style: solid;
			}

			a {
				text-align: center;
				font-weight: bold;
				cursor: pointer;
				text-decoration: none;
				color: var(--uui-color-text);
			}

			uui-tag {
				margin-top: 5px;
			}

			uui-tag uui-icon {
				padding-right: 10px;
			}

			.flex {
				display: flex;
				justify-content: space-between;
			}
		`,
	];

	@state()
	private _buttonState: UUIButtonState;

	@state()
	private _groupWithResults: HealthCheckGroupWithResult[] = [];

	private _modalService?: UmbModalService;

	private _healthCheckManifests: ManifestHealthCheck[] = [];

	private _healthCheckDashboardContext?: UmbHealthCheckDashboardContext;

	constructor() {
		super();
		this.consumeAllContexts(['umbNotificationService', 'umbModalService', 'umbHealthCheckDashboard'], (instances) => {
			this._modalService = instances['umbModalService'];
			this._healthCheckDashboardContext = instances['umbHealthCheckDashboard'];
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
						label="Check all groups"
						color="positive"
						look="primary"
						.state="${this._buttonState}"
						@click="${this._onHealthCheckHandler}">
						Check all groups
					</uui-button>
				</div>
				<!--//TODO: i want to wrap my extension container in a grid wrapper -->
				<umb-extension-slot type="healthCheck" default-element="umb-health-check-overview-group"></umb-extension-slot>
			</uui-box>
		`;
	}

	private renderChecks(name: string) {
		if (this._groupWithResults.length) {
			const group = this._groupWithResults?.find((group) => group.name == name);
			//console.log('found:', group);
			return html`${name}`;
		} else return nothing;
	}
}

export default UmbDashboardHealthCheckOverviewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-health-check-overview': UmbDashboardHealthCheckOverviewElement;
	}
}
