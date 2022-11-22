import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { healthGroups, HealthCheckGroup, HealthType } from '../../../../core/mocks/data/health-check.data';

import { UmbModalService } from '../../../../core/services/modal';
import { UmbNotificationService } from '../../../../core/services/notification';

import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';

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
		`,
	];

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
		this._getHealthGroups();
	}

	private async _getHealthGroups() {
		try {
			/*const { data } = await getHealthGroups({});
			this._healthGroups = data as HealthCheckGroup[]; */
		} catch (e) {
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
				<div class="headline" slot="headline">Health Check</div>
				<div class="group-wrapper">
					${this._healthGroups.map((group) => {
						const checksOfGroup = this.getChecksOfGroup(group);
						return html` <a href="${this.urlGenerator(group.name)}">
							<uui-box class="group-box">
								${group.name}
								<br />
								<uui-icon-registry-essential>
									${checksOfGroup.success ? this.renderTags('Success', checksOfGroup.success) : ''}
									${checksOfGroup.warning ? this.renderTags('Warning', checksOfGroup.warning) : ''}
									${checksOfGroup.error ? this.renderTags('Error', checksOfGroup.error) : ''}
								</uui-icon-registry-essential>
							</uui-box>
						</a>`;
					})}
				</div>
			</uui-box>
		`;
	}

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

	private renderTags(type: HealthType, amount: number) {
		return html` <uui-tag
			color="${type == 'Success' ? 'positive' : type == 'Warning' ? 'warning' : 'danger'}"
			look="secondary">
			<uui-icon name="${type == 'Success' ? 'check' : type == 'Warning' ? 'alert' : 'remove'}"></uui-icon> ${amount}
		</uui-tag>`;
	}
}

export default UmbDashboardHealthCheckOverviewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-health-check-overview': UmbDashboardHealthCheckOverviewElement;
	}
}
