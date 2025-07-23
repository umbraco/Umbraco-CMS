import type { UmbHealthCheckContext } from '../health-check.context.js';
import type { UmbHealthCheckDashboardContext } from '../health-check-dashboard.context.js';
import { UMB_HEALTHCHECK_DASHBOARD_CONTEXT } from '../health-check-dashboard.context.js';
import type { UUIButtonState } from '@umbraco-cms/backoffice/external/uui';
import { css, html, nothing, customElement, property, state, unsafeHTML } from '@umbraco-cms/backoffice/external/lit';
import type {
	HealthCheckActionRequestModel,
	HealthCheckGroupPresentationModel,
	HealthCheckWithResultPresentationModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { StatusResultTypeModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import './health-check-action.element.js';

@customElement('umb-dashboard-health-check-group')
export class UmbDashboardHealthCheckGroupElement extends UmbLitElement {
	@property()
	groupName!: string;

	@state()
	private _buttonState: UUIButtonState;

	@state()
	private _group?: HealthCheckGroupPresentationModel;

	private _healthCheckContext?: UmbHealthCheckDashboardContext;

	@state()
	private _idResults?: HealthCheckWithResultPresentationModel[];

	private _api?: UmbHealthCheckContext;

	constructor() {
		super();
		this.consumeContext(UMB_HEALTHCHECK_DASHBOARD_CONTEXT, (instance) => {
			this._healthCheckContext = instance;

			this._api = this._healthCheckContext?.apis.get(this.groupName);

			if (this._api) {
				this._api.getGroupChecks(this.groupName);

				this.observe(this._api.checks, (group) => {
					this._group = group;
				});

				this.observe(this._api.results, (results) => {
					this._idResults = results?.checks;
				});
			}
		});
	}

	private async _buttonHandler() {
		this._buttonState = 'waiting';
		await this._api?.checkGroup(this.groupName);
		this._buttonState = 'success';
	}

	override render() {
		return html` <a href="section/settings/dashboard/health-check"> &larr; Back to overview </a>
			${this._group ? this.#renderGroup() : nothing}`;
	}

	#renderGroup() {
		return html`
			<div class="header">
				<h2>${this._group?.name}</h2>
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
				${this._group?.checks?.map((check) => {
					return html`<uui-box headline="${check.name || '?'}">
						<p>${check.description}</p>
						${check.id ? this.renderCheckResults(check.id) : nothing}
					</uui-box>`;
				})}
			</div>
		`;
	}

	renderCheckResults(id: string) {
		if (!this._idResults) {
			return nothing;
		}
		const checkResults = this._idResults.find((x) => x.id === id);

		if (!checkResults) {
			return nothing;
		}

		return html`<uui-icon-registry-essential>
			<div class="check-results-wrapper">
				${checkResults.results?.map((result) => {
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
									<uui-icon name="icon-out"></uui-icon>
								</uui-button>`
							: nothing}
					</div>`;
				})}
			</div>
		</uui-icon-registry-essential>`;
	}

	private renderIcon(type?: StatusResultTypeModel) {
		switch (type) {
			case StatusResultTypeModel.SUCCESS:
				return html`<uui-icon style="color: var(--uui-color-positive);" name="check"></uui-icon>`;
			case StatusResultTypeModel.WARNING:
				return html`<uui-icon style="color: var(--uui-color-warning-standalone);" name="alert"></uui-icon>`;
			case StatusResultTypeModel.ERROR:
				return html`<uui-icon style="color: var(--uui-color-danger);" name="remove"></uui-icon>`;
			case StatusResultTypeModel.INFO:
				return html`<uui-icon style="color:black;" name="info"></uui-icon>`;
			default:
				return nothing;
		}
	}

	private renderActions(actions: HealthCheckActionRequestModel[]) {
		if (actions.length)
			return html` <div class="action-wrapper">
				${actions.map(
					(action) =>
						html`<umb-dashboard-health-check-action
							.action=${action}
							@action-executed=${() => this._buttonHandler()}></umb-dashboard-health-check-action>`,
				)}
			</div>`;
		else return nothing;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
			}

			uui-box + uui-box {
				margin-top: var(--uui-size-space-5);
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
}

export default UmbDashboardHealthCheckGroupElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-health-check-group': UmbDashboardHealthCheckGroupElement;
	}
}
