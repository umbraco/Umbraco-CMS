import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { healthGroups, HealthCheckGroup, HealthType } from '../../../core/mocks/data/health-check.data';

@customElement('umb-dashboard-health-check')
export class UmbDashboardHealthCheckElement extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			uui-box + uui-box {
				margin-top: var(--uui-size-space-5);
			}
			uui-box p:first-child {
				margin-block-start: 0;
			}
			.flex {
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

			.mini-box {
				flex-basis: 20%;
				min-width: 150px;
				max-width: calc(20% - 10px);
				margin: 5px;
				text-align: center;
				font-weight: bold;
				cursor: pointer;
			}

			uui-tag {
				margin-top: 5px;
			}

			uui-tag uui-icon {
				padding-right: 10px;
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
			.postive {
				color: var(--uui-color-positive);
			}
			button {
				background: none;
				border: none;
				text-decoration: underline;
				cursor: pointer;
				margin-bottom: var(--uui-size-space-5);
			}
		`,
	];

	@state()
	private healthGroup = healthGroups;

	@state()
	private currentView?: HealthCheckGroup;

	render() {
		return !this.currentView ? this.renderOverview() : this.renderGroup(this.currentView);
	}

	renderOverview() {
		return html`
			<uui-box>
				<div class="headline" slot="headline">
					Health Check
					<uui-button look="primary" color="positive" label="Check health on all groups">Check All Groups</uui-button>
				</div>
				<p>
					The health checker evaluates various areas of your site for best practice settings, configuration, potential
					problems, etc. You can easily fix problems by pressing a button. You can add your own health checks, have a
					look at
					<a href="https://our.umbraco.com/documentation/Extending/Health-Check/">
						the documentation for more information
					</a>
					about custom health checks.<br />
				</p>
				<div class="flex">
					${this.healthGroup.map((group) => {
						const res = { success: 0, warning: 0, error: 0 };
						const status = group.checks.map((item) => {
							return item.data?.resultType;
						});
						if (status.length) {
							res.success = status.filter((s) => s == 'Success').length;
							res.warning = status.filter((s) => s == 'Warning').length;
							res.error = status.filter((s) => s == 'Error').length;
						}
						return html`<uui-box class="mini-box" @click="${() => (this.currentView = group)}">
							${group.name}
							<br />
							<uui-icon-registry-essential>
								${res.success ? this.renderTags('Success', res.success) : ''}
								${res.warning ? this.renderTags('Warning', res.warning) : ''}
								${res.error ? this.renderTags('Error', res.error) : ''}
							</uui-icon-registry-essential></uui-box
						>`;
					})}
				</div>
			</uui-box>
		`;
	}

	private renderTags(type: HealthType, amount: number) {
		return html` <uui-tag
			color="${type == 'Success' ? 'positive' : type == 'Warning' ? 'warning' : 'danger'}"
			look="secondary">
			<uui-icon name="${type == 'Success' ? 'check' : type == 'Warning' ? 'warning' : 'wrong'}"></uui-icon>${amount}
		</uui-tag>`;
	}

	renderGroup(group: HealthCheckGroup) {
		return html`
			<button @click="${() => (this.currentView = undefined)}">&larr; Back to overview</button>
			<uui-box>
				<div class="headline" slot="headline">
					Configuration
					<uui-button color="positive" look="primary">Check group</uui-button>
				</div>
				${group.checks.map((item) => {
					if (item.data) {
						return html`<uui-box headline="${item.name}">
							<p>${item.description}</p>
							<div class="data">
								<uui-icon-registry-essential>
									<p>
										<uui-icon
											name="${item.data.resultType == 'Success'
												? 'check'
												: item.data.resultType == 'Warning'
												? 'warning'
												: 'wrong'}"
											class="${item.data.resultType == 'Success'
												? 'positive'
												: item.data.resultType == 'Warning'
												? 'warning'
												: 'danger'}"></uui-icon>
										${item.data.message}
									</p>
									${!item.data.readMoreLink
										? html`<uui-button color="default" look="primary">Read More</uui-button>`
										: ``}
								</uui-icon-registry-essential>
							</div>
						</uui-box>`;
					}
					return html`<uui-box headline="${item.name}">
						<p>${item.description}</p>
					</uui-box>`;
				})}
			</uui-box>
		`;
	}
}

export default UmbDashboardHealthCheckElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-health-check': UmbDashboardHealthCheckElement;
	}
}
