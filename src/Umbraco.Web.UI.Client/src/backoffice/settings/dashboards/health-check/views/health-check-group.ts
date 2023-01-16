import { UUIButtonState } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';

import { UmbHealthCheckContext } from '../health-check.context';
import { UmbHealthCheckDashboardContext } from '../health-check-dashboard.context';
import {
	HealthCheckAction,
	HealthCheckGroupWithResult,
	HealthCheckWithResult,
	StatusResultType,
} from '@umbraco-cms/backend-api';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-dashboard-health-check-group')
export class UmbDashboardHealthCheckGroupElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			uui-box {
				margin-bottom: var(--uui-size-space-5);
			}

			uui-box p:first-child {
				margin-block-start: 0;
			}

			.group-wrapper {
				display: flex;
				flex-wrap: wrap;
				margin-inline: -5px;
			}

			.header {
				display: flex;
				justify-content: space-between;
				align-items: center;
			}

			uui-box:first-child p {
				margin-top: 0;
			}

			.group-box {
				flex-basis: 20%;
				min-width: 150px;
				max-width: calc(20% - 10px);
				margin: 5px;
				text-align: center;
				font-weight: bold;
				cursor: pointer;
			}

			p uui-icon {
				vertical-align: sub;
			}

			.data .result-wrapper:not(:first-child) {
				padding-top: var(--uui-size-space-5);
				margin-top: var(--uui-size-space-5);
				border-top: 1px solid var(--uui-color-divider-standalone);
			}

			.data p {
				margin: 0;
			}
			.data uui-button {
				margin-block-start: 1em;
			}

			.action-wrapper {
				margin-top: var(--uui-size-space-4);
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-4);
			}

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

	@property()
	groupName!: string;

	@state()
	private _buttonState: UUIButtonState;

	@state()
	private _group?: HealthCheckGroupWithResult;

	private _healthCheckContext?: UmbHealthCheckDashboardContext;

	@state()
	private _checks?: HealthCheckWithResult[] | null;

	@state()
	private _keyResults?: any;

	private _api?: UmbHealthCheckContext;

	constructor() {
		super();
		this.consumeContext('umbHealthCheckDashboard', (instance) => {
			this._healthCheckContext = instance;

			this._api = this._healthCheckContext?.apis.get(this.groupName);

			this._api?.getGroupChecks(this.groupName);

			this._api?.checks.subscribe((checks) => {
				this._checks = checks;
				this._group = { name: this.groupName, checks: this._checks };
			});

			this._api?.results.subscribe((results) => {
				this._keyResults = results;
			});
		});
	}

	private async _buttonHandler() {
		this._buttonState = 'waiting';
		this._api?.checkGroup(this.groupName);
		this._buttonState = 'success';
	}

	render() {
		if (this._group) {
			return html`
				<div class="header">
					<h2>${this._group.name}</h2>
					<uui-button
						label="Get checks"
						color="positive"
						look="primary"
						.state="${this._buttonState}"
						@click="${this._buttonHandler}">
						Get checks
					</uui-button>
				</div>
				<div class="checks-wrapper">
					${this._group.checks?.map((check) => {
						return html`<uui-box headline="${check.name || '?'}">
							<p>${check.description}</p>
							${check.key ? this.renderCheckResults(check.key) : nothing}
						</uui-box>`;
					})}
				</div>
			`;
		} else return nothing;
	}

	renderCheckResults(key: string) {
		const checkResults = this._keyResults?.find((result: any) => result.key === key);
		return html`<uui-icon-registry-essential>
			<div class="data">
				${checkResults?.results.map((result: any) => {
					return html`<div class="result-wrapper">
						<p>${this.renderIcon(result.resultType)} ${result.message}</p>
						${result.readMoreLink
							? html`<uui-button
									label="Read more"
									color="default"
									look="primary"
									target="_blank"
									href="${result.readMoreLink}">
									Read more
									<uui-icon name="umb:out"></uui-icon>
							  </uui-button>`
							: nothing}
					</div>`;
				})}
			</div>
		</uui-icon-registry-essential>`;
	}

	private renderIcon(type?: StatusResultType) {
		switch (type) {
			case 'Success':
				return html`<uui-icon style="color: var(--uui-color-positive);" name="check"></uui-icon>`;
			case 'Warning':
				return html`<uui-icon style="color: var(--uui-color-warning);" name="alert"></uui-icon>`;
			case 'Error':
				return html`<uui-icon style="color: var(--uui-color-danger);" name="remove"></uui-icon>`;
			case 'Info':
				return html`<uui-icon style="color:black;" name="info"></uui-icon>`;
			default:
				return nothing;
		}
	}

	private renderActions(actions: HealthCheckAction[]) {
		if (actions.length)
			return html` <div class="action-wrapper">
				${actions.map((action) => {
					return html` <div class="action">
						<uui-button look="primary" color="positive" label="${action.name || 'action'}">
							${action.name || 'Action'}
						</uui-button>
						<p>${action.description || html`<span class="no-description">This action has no description</span>`}</p>
					</div>`;
				})}
			</div>`;
		else return nothing;
	}
}

export default UmbDashboardHealthCheckGroupElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-health-check-group': UmbDashboardHealthCheckGroupElement;
	}
}
