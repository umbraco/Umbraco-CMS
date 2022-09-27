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
	static styles = [UUITextStyles, css`
		uui-box + uui-box {
			margin-top: var(--uui-size-space-5);
		}
		uui-box p:first-child {
			margin-block-start: 0;
		}
	`];

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

	private async _onRefreshCacheHandler() {
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

	private async _onReloadCacheHandler() {
		await undefined;
	}

	private async _onRebuildCacheHandler() {
		await undefined;
	}

	private async _onSnapshotCacheHandler() {
		await undefined;
	}

	render() {
		return html`
			<uui-box headline="Published Cache Status">
				<p>${this._publishedStatusText}</p>
				<uui-button .state=${this._buttonState} type="button" look="primary" color="danger" @click=${this._onRefreshCacheHandler}>Refresh Status</uui-button>
			</uui-box>

			<uui-box headline="Memory Cache">
				<p>This button lets you reload the in-memory cache, by entirely reloading it from the database cache (but it does not rebuild that database cache). This is relatively fast. Use it when you think that the memory cache has not been properly refreshed, after some events triggered—which would indicate a minor Umbraco issue. (note: triggers the reload on all servers in an LB environment).</p>
				<uui-button type="button" look="primary" color="danger" @click=${this._onReloadCacheHandler}>Reload Memory Cache</uui-button>
			</uui-box>

			<uui-box headline="Database Cache">
				<p>This button lets you rebuild the database cache, ie the content of the cmsContentNu table. Rebuilding can be expensive. Use it when reloading is not enough, and you think that the database cache has not been properly generated—which would indicate some critical Umbraco issue.</p>
				<uui-button type="button" look="primary" color="danger" @click=${this._onRebuildCacheHandler}>Rebuild Database Cache</uui-button>
			</uui-box>

			<uui-box headline="Internal Cache">
				<p>This button lets you trigger a NuCache snapshots collection (after running a fullCLR GC). Unless you know what that means, you probably do not need to use it.</p>
				<uui-button type="button" look="primary" color="danger" @click=${this._onSnapshotCacheHandler}>Snapshot Internal Cache</uui-button>
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
