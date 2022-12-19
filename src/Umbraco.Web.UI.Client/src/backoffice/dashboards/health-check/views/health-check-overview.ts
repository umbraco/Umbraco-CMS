import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import {
	healthGroups,
	HealthCheckGroup,
	HealthResult,
	HealthCheckGroupCheck,
} from '../../../../core/mocks/data/health-check.data';

import { UmbModalService } from '../../../../core/services/modal';
import { UmbNotificationService } from '../../../../core/services/notification';

import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { UUIButtonState } from '@umbraco-ui/uui';

@customElement('umb-dashboard-health-check-overview')
export class UmbDashboardHealthCheckOverviewElement extends UmbContextConsumerMixin(LitElement) {
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
	private _showChecks = false;

	@state()
	private _loaders?: string[];

	@state()
	private _healthGroups: HealthCheckGroup[] = healthGroups;

	private _notificationService?: UmbNotificationService;
	private _modalService?: UmbModalService;

	private urlGenerator(name: string) {
		return `${window.location.href.replace(/\/+$/, '')}/${name.replace(/\s+/g, '-')}`;
	}

	constructor() {
		super();
		this.consumeAllContexts(['umbNotificationService', 'umbModalService'], (instances) => {
			this._notificationService = instances['umbNotificationService'];
			this._modalService = instances['umbModalService'];
		});
	}

	connectedCallback() {
		super.connectedCallback();
	}

	private async _healthCheckHandler() {
		this._buttonState = 'waiting';
		this._getAllHealthGroups();
	}

	private async _getHealthGroup() {
		console.log('group');
	}

	private async _getAllHealthGroups() {
		try {
			await new Promise((resolve) => setTimeout(resolve, (Math.random() + 1) * 1000));
			this._showChecks = true;
			this._buttonState = 'success';
			/*const { data } = await getHealthGroups({});
			this._healthGroups = data as HealthCheckGroup[]; */
		} catch (e) {
			this._buttonState = 'failed';
			/*if (e instanceof getHealthGroups.Error) {
				const error = e.getActualType();
				const data: UmbNotificationDefaultData = { message: error.data.detail ?? 'Something went wrong' };
				this._notificationService?.peek('danger', { data });
			}*/
		}
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
						@click="${this._healthCheckHandler}">
						Check all groups
					</uui-button>
				</div>
				<div class="group-wrapper">
					${this._healthGroups.map((group) => {
						if (group.name)
							return html` <a href="${this.urlGenerator(group.name)}">
								<uui-box class="group-box"> ${group.name} ${this.renderChecks(group)} </uui-box>
							</a>`;
						else return nothing;
					})}
				</div>
			</uui-box>
		`;
	}

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
	}

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
}

export default UmbDashboardHealthCheckOverviewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-health-check-overview': UmbDashboardHealthCheckOverviewElement;
	}
}
