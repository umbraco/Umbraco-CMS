import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import {
	healthGroups,
	HealthCheckGroup,
	HealthType,
	HealthCheckData,
	HealthCheck,
} from '../../../core/mocks/data/health-check.data';

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

	@state()
	private _healthGroups: HealthCheckGroup[] = healthGroups;

	@state()
	private _currentView?: HealthCheckGroup;

	private getChecksOfGroup(group: HealthCheckGroup) {
		const res = { success: 0, warning: 0, error: 0 };
		group.checks.map((category) => {
			category.data?.map((data) => {
				data.resultType == 'Success'
					? (res.success += 1)
					: data.resultType == 'Warning'
					? (res.warning += 1)
					: (res.error += 1);
			});
		});
		return res;
	}

	render() {
		return !this._currentView ? this.renderOverview() : this.renderGroup(this._currentView);
	}

	renderOverview() {
		return html`
			<uui-box>
				<div class="headline" slot="headline">Health Check</div>
				<div class="group-wrapper">
					${this._healthGroups.map((group) => {
						const checksOfGroup = this.getChecksOfGroup(group);
						return html` <uui-box class="group-box" @click="${() => (this._currentView = group)}">
							${group.name}
							<br />
							<uui-icon-registry-essential>
								${checksOfGroup.success ? this.renderTags('Success', checksOfGroup.success) : ''}
								${checksOfGroup.warning ? this.renderTags('Warning', checksOfGroup.warning) : ''}
								${checksOfGroup.error ? this.renderTags('Error', checksOfGroup.error) : ''}
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
			${this.renderIcon(type)}${amount}
		</uui-tag>`;
	}

	private renderIcon(type: HealthType) {
		return html`<uui-icon
			class="${type == 'Success' ? 'positive' : type == 'Warning' ? 'warning' : 'danger'}"
			name="${type == 'Success' ? 'check' : type == 'Warning' ? 'alert' : 'remove'}"></uui-icon>`;
	}

	renderGroup(group: HealthCheckGroup) {
		return html`
			<button @click="${() => (this._currentView = undefined)}">&larr; Back to overview</button>
			<uui-box>
				<div class="headline" slot="headline">Configuration</div>
				${group.checks.map((item) => {
					if (item.data) {
						return html`<uui-box headline="${item.name}">
							<p>${item.description}</p>
							<uui-icon-registry-essential>
								<div class="data">
									${item.data.map((res) => {
										return html` <section>
											<p>${this.renderIcon(res.resultType)} ${res.message}</p>
											${res.readMoreLink ? html`<uui-button color="default" look="primary">Read more</uui-button>` : ''}
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
	}
}

export default UmbDashboardHealthCheckElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-health-check': UmbDashboardHealthCheckElement;
	}
}
