import { UUIButtonState } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { unsafeHTML } from 'lit/directives/unsafe-html.js';

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
import './health-check-action.element';

@customElement('umb-dashboard-health-check-group')
export class UmbDashboardHealthCheckGroupElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			uui-box {
				margin-bottom: var(--uui-size-space-5);
			}

			p {
				margin: 0;
			}

			.header {
				display: flex;
				justify-content: space-between;
				align-items: center;
			}

			.check-results-wrapper .check-result {
				padding-top: var(--uui-size-space-5);
			}

			.check-results-wrapper .check-result:not(:last-child) {
				border-bottom: 1px solid var(--uui-color-divider-standalone);
				padding-bottom: var(--uui-size-space-5);
			}

			.check-results-wrapper uui-button {
				margin-block-start: 1em;
			}

			.check-result-description {
				display: flex;
			}

			.check-result-description span {
				width: 36px;
			}

			uui-icon {
				vertical-align: sub;
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
		this.consumeContext<UmbHealthCheckDashboardContext>('umbHealthCheckDashboard', (instance) => {
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

	private _onActionClick(action: HealthCheckAction) {
		return tryExecuteAndNotify(this, HealthCheckResource.postHealthCheckExecuteAction({ requestBody: action }));
	}

	render() {
		if (this._group) {
			return html`
				<div class="header">
					<h2>${this._group.name}</h2>
					<uui-button
						label="Perform checks"
						color="positive"
						look="primary"
						.state="${this._buttonState}"
						@click="${this._buttonHandler}">
						Perform checks
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
			<div class="check-results-wrapper">
				${checkResults?.results.map((result: any) => {
					return html`<div class="check-result">
						<div class="check-result-description">
							<span>${this.renderIcon(result.resultType)}</span>
							<p>${unsafeHTML(result.message)}</p>
						</div>

						${result.actions ? this.renderActions(result.actions) : nothing}
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
			case StatusResultType.SUCCESS:
				return html`<uui-icon style="color: var(--uui-color-positive);" name="check"></uui-icon>`;
			case StatusResultType.WARNING:
				return html`<uui-icon style="color: var(--uui-color-warning);" name="alert"></uui-icon>`;
			case StatusResultType.ERROR:
				return html`<uui-icon style="color: var(--uui-color-danger);" name="remove"></uui-icon>`;
			case StatusResultType.INFO:
				return html`<uui-icon style="color:black;" name="info"></uui-icon>`;
			default:
				return nothing;
		}
	}

	private renderActions(actions: HealthCheckAction[]) {
		if (actions.length)
			return html` <div class="action-wrapper">
				${actions.map(
					(action) =>
						html`<umb-dashboard-health-check-action
							.action=${action}
							@action-executed=${() => this._buttonHandler()}></umb-dashboard-health-check-action>`
				)}
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
