import type { UUIButtonState } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { PublishedCacheService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-dashboard-published-status')
export class UmbDashboardPublishedStatusElement extends UmbLitElement {
	@state()
	private _publishedStatusText = '';

	@state()
	private _buttonState: UUIButtonState = undefined;

	@state()
	private _buttonStateReload: UUIButtonState = undefined;

	@state()
	private _buttonStateRebuild: UUIButtonState = undefined;

	@state()
	private _buttonStateCollect: UUIButtonState = undefined;

	override connectedCallback() {
		super.connectedCallback();
		this._getPublishedStatus();
	}

	// Refresh
	private async _getPublishedStatus() {
		const { data } = await tryExecuteAndNotify(this, PublishedCacheService.getPublishedCacheStatus());
		if (data) {
			this._publishedStatusText = data;
		}
	}

	private async _onRefreshCacheHandler() {
		this._buttonState = 'waiting';
		await this._getPublishedStatus();
		this._buttonState = 'success';
	}

	//Reload
	private async _reloadMemoryCache() {
		this._buttonStateReload = 'waiting';
		this._buttonState = 'waiting';
		const { error } = await tryExecuteAndNotify(this, PublishedCacheService.postPublishedCacheReload());
		if (error) {
			this._buttonStateReload = 'failed';
			this._buttonState = 'failed';
		} else {
			this._buttonStateReload = 'success';
			this._getPublishedStatus();
			this._buttonState = 'success';
		}
	}
	private async _onReloadCacheHandler() {
		await umbConfirmModal(this, {
			headline: 'Reload',
			content: html` Trigger a in-memory and local file cache reload on all servers.`,
			color: 'danger',
			confirmLabel: 'Continue',
		});

		this._reloadMemoryCache();
	}

	// Rebuild
	private async _rebuildDatabaseCache() {
		this._buttonStateRebuild = 'waiting';
		const { error } = await tryExecuteAndNotify(this, PublishedCacheService.postPublishedCacheRebuild());
		if (error) {
			this._buttonStateRebuild = 'failed';
		} else {
			this._buttonStateRebuild = 'success';
		}
	}

	private async _onRebuildCacheHandler() {
		await umbConfirmModal(this, {
			headline: 'Rebuild',
			content: html` Rebuild content in cmsContentNu database table. Expensive.`,
			color: 'danger',
			confirmLabel: 'Continue',
		});

		this._rebuildDatabaseCache();
	}

	//Collect
	private async _cacheCollect() {
		this._buttonStateCollect = 'waiting';
		const { error } = await tryExecuteAndNotify(this, PublishedCacheService.postPublishedCacheCollect());
		if (error) {
			this._buttonStateCollect = 'failed';
		} else {
			this._buttonStateCollect = 'success';
		}
	}

	private async _onSnapshotCacheHandler() {
		await umbConfirmModal(this, {
			headline: 'Snapshot',
			content: html` Trigger a NuCache snapshots collection.`,
			color: 'danger',
			confirmLabel: 'Continue',
		});
		this._cacheCollect();
	}

	override render() {
		return html`
			<uui-box headline="Published Cache Status">
				<p>${this._publishedStatusText}</p>
				<uui-button
					.state=${this._buttonState}
					type="button"
					look="primary"
					color="danger"
					label="Refresh Status"
					@click=${this._onRefreshCacheHandler}>
					Refresh Status
				</uui-button>
			</uui-box>

			<uui-box headline="Memory Cache">
				<p>
					This button lets you reload the in-memory cache, by entirely reloading it from the database cache (but it does
					not rebuild that database cache). This is relatively fast. Use it when you think that the memory cache has not
					been properly refreshed, after some events triggered—which would indicate a minor Umbraco issue. (note:
					triggers the reload on all servers in an LB environment).
				</p>
				<uui-button
					type="button"
					look="primary"
					color="danger"
					label="Reload Memory Cache"
					@click=${this._onReloadCacheHandler}
					.state=${this._buttonStateReload}>
					Reload Memory Cache
				</uui-button>
			</uui-box>

			<uui-box headline="Database Cache">
				<p>
					This button lets you rebuild the database cache, ie the content of the cmsContentNu table. Rebuilding can be
					expensive. Use it when reloading is not enough, and you think that the database cache has not been properly
					generated—which would indicate some critical Umbraco issue.
				</p>
				<uui-button
					type="button"
					look="primary"
					color="danger"
					label="Rebuild Database Cache"
					@click=${this._onRebuildCacheHandler}
					.state=${this._buttonStateRebuild}>
					Rebuild Database Cache
				</uui-button>
			</uui-box>

			<uui-box headline="Internal Cache">
				<p>
					This button lets you trigger a NuCache snapshots collection (after running a fullCLR GC). Unless you know what
					that means, you probably do not need to use it.
				</p>
				<uui-button
					type="button"
					look="primary"
					color="danger"
					label="Snapshot Internal Cache"
					@click=${this._onSnapshotCacheHandler}
					.state=${this._buttonStateCollect}>
					Snapshot Internal Cache
				</uui-button>
			</uui-box>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				padding: var(--uui-size-layout-1);
			}

			uui-box + uui-box {
				margin-top: var(--uui-size-space-5);
			}
			uui-box p:first-child {
				margin-block-start: 0;
			}
		`,
	];
}

export default UmbDashboardPublishedStatusElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-published-status': UmbDashboardPublishedStatusElement;
	}
}
