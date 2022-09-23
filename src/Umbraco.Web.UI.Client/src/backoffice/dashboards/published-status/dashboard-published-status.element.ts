import { UUIButtonState } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';

import { getPublishedCacheStatus, postPublishedCacheReload } from '../../../core/api/fetcher';
import { UmbContextConsumerMixin } from '../../../core/context';
import { UmbNotificationService } from '../../../core/services/notification';
import { UmbNotificationDefaultData } from '../../../core/services/notification/layouts/default';

@customElement('umb-dashboard-published-status')
export class UmbDashboardPublishedStatusElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	@state()
	private _publishedStatusText = '';

	@state()
	private _buttonState: UUIButtonState = undefined;

	private _notificationService?: UmbNotificationService;

	constructor() {
		super();

		this.consumeContext('umbNotificationService', (notificationService: UmbNotificationService) => {
			this._notificationService = notificationService;
		});
	}

	connectedCallback() {
		super.connectedCallback();

		this._getPublishedStatus();
	}

	private async _getPublishedStatus() {
		try {
			const { data } = await getPublishedCacheStatus({});
			this._publishedStatusText = data;
		} catch (e) {
			if (e instanceof getPublishedCacheStatus.Error) {
				const error = e.getActualType();
				const data: UmbNotificationDefaultData = { message: error.data.detail ?? 'Something went wrong' };
				this._notificationService?.peek('danger', { data });
			}
		}
	}

	private async _onReloadCacheHandler() {
		this._buttonState = 'waiting';
		try {
			await postPublishedCacheReload({});
			this._getPublishedStatus();
			this._buttonState = 'success';
		} catch (e) {
			this._buttonState = 'failed';
			if (e instanceof postPublishedCacheReload.Error) {
				const error = e.getActualType();
				const data: UmbNotificationDefaultData = { message: error.data.detail ?? 'Something went wrong' };
				this._notificationService?.peek('danger', { data });
			}
		}
	}

	render() {
		return html`
			<uui-box>
				<h1>Published Status</h1>
				<p>${this._publishedStatusText}</p>
			</uui-box>

			<uui-box>
				<uui-button .state=${this._buttonState} type="button" look="primary" @click=${this._onReloadCacheHandler}
					>Reload Cache</uui-button
				>
			</uui-box>
		`;
	}
}

export default UmbDashboardPublishedStatusElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-published-status': UmbDashboardPublishedStatusElement;
	}
}
