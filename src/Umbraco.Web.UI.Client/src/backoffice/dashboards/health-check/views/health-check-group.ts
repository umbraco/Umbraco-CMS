import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { healthGroups, HealthCheckGroup, HealthType } from '../../../../core/mocks/data/health-check.data';

@customElement('umb-dashboard-health-check-group')
export class UmbDashboardHealthCheckGroupElement extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			uui-box {
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

			.headline {
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

			.danger {
				color: var(--uui-color-danger);
			}
			.warning {
				color: var(--uui-color-warning);
			}
			.positive {
				color: var(--uui-color-positive);
			}

			button {
				background: none;
				border: none;
				text-decoration: underline;
				cursor: pointer;
				margin-bottom: var(--uui-size-space-5);
			}

			section:not(:first-child) {
				padding-top: var(--uui-size-space-5);
				margin-top: var(--uui-size-space-5);
				border-top: 1px solid var(--uui-color-divider-standalone);
			}

			section p {
				margin: 0;
			}
			uui-button {
				margin-block-start: 1em;
			}
		`,
	];

	@property()
	groupName!: string;

	@state()
	private _healthGroup?: HealthCheckGroup;

	constructor() {
		super();
		this._healthGroup = healthGroups[2];
	}

	protected firstUpdated() {
		console.log(this.groupName);
	}

	render() {
		if (this._healthGroup) {
			return html`
				<uui-box>
					<div class="headline" slot="headline">${this._healthGroup.name}</div>
					${this._healthGroup.checks.map((item) => {
						if (item.data) {
							return html`<uui-box headline="${item.name}">
								<p>${item.description}</p>
								<uui-icon-registry-essential>
									<div class="data">
										${item.data.map((res) => {
											return html` <section>
												<p>${this.renderIcon(res.resultType)} ${res.message}</p>
												${res.readMoreLink
													? html`<uui-button color="default" look="primary">Read more</uui-button>`
													: ''}
											</section>`;
										})}
									</div>
								</uui-icon-registry-essential>
							</uui-box>`;
						}
						return html`<uui-box headline="${item.name}">
							<p>${item.description}</p>
						</uui-box>`;
					})}
				</uui-box>
			`;
		} else return html``;
	}

	private renderIcon(type: HealthType) {
		return html`<uui-icon
			class="${type == 'Success' ? 'positive' : type == 'Warning' ? 'warning' : 'danger'}"
			name="${type == 'Success' ? 'check' : type == 'Warning' ? 'alert' : 'remove'}"></uui-icon>`;
	}
}

export default UmbDashboardHealthCheckGroupElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-health-check-group': UmbDashboardHealthCheckGroupElement;
	}
}
