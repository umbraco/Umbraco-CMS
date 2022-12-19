import { UUIButtonState } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import {
	healthGroups,
	HealthCheckGroup,
	HealthResult,
	healthGroups2,
	HealthCheckGroupCheck,
	CheckResult,
} from '../../../../core/mocks/data/health-check.data';

@customElement('umb-dashboard-health-check-group')
export class UmbDashboardHealthCheckGroupElement extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			uui-box + uui-box {
				margin-top: var(--uui-size-space-5);
			}

			uui-box p:first-child {
				margin-block-start: 0;
			}

			.group-wrapper {
				display: flex;
				flex-wrap: wrap;
				margin-inline: -5px;
			}

			.flex {
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
				vertical-align: middle;
			}

			.data div:not(:first-child) {
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
		`,
	];

	@property()
	groupName!: string;

	@state()
	private _buttonState: UUIButtonState;

	@state()
	private _healthGroup?: HealthCheckGroup;

	constructor() {
		super();
		this._healthGroup = healthGroups[4];
	}

	protected firstUpdated() {
		console.log(this.groupName);
	}

	private async _buttonHandler() {
		this._buttonState = 'waiting';
		this._getChecks();
	}
	private async _getChecks() {
		await new Promise((resolve) => setTimeout(resolve, (Math.random() + 1) * 1000));
		this._buttonState = 'success';
	}

	render() {
		if (this._healthGroup) {
			return html`
				<uui-box>
					<div slot="headline" class="flex">
						${this._healthGroup.name}
						<uui-button color="positive" look="primary" .state="${this._buttonState}" @click="${this._buttonHandler}">
							Get checks
						</uui-button>
					</div>
					${this._healthGroup.checks?.map((check) => {
						return this.renderCheckDetails(check);
					})}
				</uui-box>
			`;
		} else return nothing;
	}

	renderCheckDetails(check: HealthCheckGroupCheck) {
		return html`<uui-box headline="${check.name || '?'}">
			<p>${check.description}</p>
			${check.results ? this.renderCheckResults(check.results) : nothing}
		</uui-box>`;
	}

	renderCheckResults(results: CheckResult[]) {
		return html`<uui-icon-registry-essential>
			<div class="data">
				${results.map((result) => {
					return html`<div>
						<p>${this.renderIcon(result.resultType)} ${result.message}</p>
						${result.readMoreLink ? html`<uui-button color="default" look="primary">Read more</uui-button>` : nothing}
					</div>`;
				})}
			</div></uui-icon-registry-essential
		>`;
	}

	private renderIcon(type?: HealthResult) {
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
}

export default UmbDashboardHealthCheckGroupElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-health-check-group': UmbDashboardHealthCheckGroupElement;
	}
}
