import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';

import { UUIButtonState } from '@umbraco-ui/uui';
import { UmbModalService } from '../../../../../core/modal';

import { UmbLitElement } from '@umbraco-cms/element';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';

import { HealthCheckGroup, HealthCheckGroupWithResult, HealthCheckResource } from '@umbraco-cms/backend-api';

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
	private _groups?: HealthCheckGroup[];

	@state()
	private _groupWithResults: HealthCheckGroupWithResult[] = [];

	@state()
	private _nameOfGroups!: string[];

	private _modalService?: UmbModalService;

	constructor() {
		super();
		this.consumeAllContexts(['umbNotificationService', 'umbModalService'], (instances) => {
			this._modalService = instances['umbModalService'];
		});
	}

	connectedCallback() {
		super.connectedCallback();
		this._getAllGroups();
	}

	private async _getAllGroups() {
		const { data } = await tryExecuteAndNotify(this, HealthCheckResource.getHealthCheckGroup({ skip: 0, take: 9999 }));
		if (data) {
			this._groups = data.items;
		}
	}

	private async _onHealthCheckHandler() {
		this._groups?.forEach((group) => {
			group.name ? this._getGroup(group.name) : nothing;
		});
	}

	private async _getGroup(name: string) {
		this._buttonState = 'waiting';
		await new Promise((resolve) => setTimeout(resolve, (Math.random() + 1) * 1000)); // simulate a delay of 1-2 seconds
		const { data } = await tryExecuteAndNotify(this, HealthCheckResource.getHealthCheckGroupByName({ name }));
		console.log(data);
	}

	render() {
		return html`
			<uui-box>
				<div slot="headline" class="flex">
					Health Check
					<uui-button
						color="positive"
						look="primary"
						.state="${this._buttonState}"
						@click="${this._onHealthCheckHandler}">
						Check all groups
					</uui-button>
				</div>
				<div class="group-wrapper">
					${this._groups?.map((group) => {
						if (group.name)
							return html` <a href="${window.location.href + '/' + group.name}">
								<uui-box class="group-box"> ${group.name} ${this.renderChecks(group.name)} </uui-box>
							</a>`;
						else return nothing;
					})}
				</div>
			</uui-box>
		`;
	}

	private renderChecks(name: string) {
		if (this._groupWithResults.length) {
			const group = this._groupWithResults?.find((group) => group.name == name);
			console.log('found:', group);
			return html`${name}`;
		} else return nothing;
	}

	/**				<div class="group-wrapper">
					${this._healthGroups.map((group) => {
						if (group.name)
							return html` <a href="${this.urlGenerator(group.name)}">
								<uui-box class="group-box"> ${group.name} ${this.renderChecks(group)} </uui-box>
							</a>`;
						else return nothing;
					})}
				</div> */
	/*
	private renderChecks(group: HealthCheckGroup) {
		if (this._showChecks && group.checks) {
			const checksOfGroup = this.getChecksInGroup(group.checks);
			return html` <br />
				<uui-icon-registry-essential>
					${checksOfGroup.success ? this.renderTags('Success', checksOfGroup.success) : ''}
					${checksOfGroup.warning ? this.renderTags('Warning', checksOfGroup.warning) : ''}
					${checksOfGroup.error ? this.renderTags('Error', checksOfGroup.error) : ''}
				</uui-icon-registry-essential>`;
		} else return nothing;
	}*/

	/*
	private getChecksInGroup(groupChecks: HealthCheckGroupCheck[]) {
		const result = { success: 0, warning: 0, error: 0, info: 0 };

		groupChecks.forEach((data) => {
			data.results?.forEach((check) => {
				switch (check.resultType) {
					case 'Success':
						result.success += 1;
						break;
					case 'Warning':
						result.warning += 1;
						break;
					case 'Error':
						result.error += 1;
						break;
					case 'Info':
						result.info += 1;
						break;
					default:
						break;
				}
			});
		});
		return result;
	}
	*/
	/*
	private renderTags(type: HealthResult, amount: number) {
		switch (type) {
			case 'Success':
				return html`<uui-tag color="positive" look="secondary"><uui-icon name="check"></uui-icon>${amount}</uui-tag>`;
			case 'Warning':
				return html`<uui-tag color="warning" look="secondary"><uui-icon name="alert"></uui-icon>${amount}</uui-tag>`;
			case 'Error':
				return html`<uui-tag color="danger" look="secondary"><uui-icon name="remove"></uui-icon>${amount}</uui-tag>`;
			case 'Info':
				return html`<uui-tag color="default" look="secondary"><uui-icon name="info"></uui-icon>${amount}</uui-tag>`;
			default:
				return html``;
		}
	}
	*/
}

export default UmbDashboardHealthCheckOverviewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-health-check-overview': UmbDashboardHealthCheckOverviewElement;
	}
}
