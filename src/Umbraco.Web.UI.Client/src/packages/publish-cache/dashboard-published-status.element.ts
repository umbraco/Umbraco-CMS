import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { PublishedCacheService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UUIButtonState } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-dashboard-published-status')
export class UmbDashboardPublishedStatusElement extends UmbLitElement {
	@state()
	private _buttonStateReload: UUIButtonState = undefined;

	@state()
	private _buttonStateRebuild: UUIButtonState = undefined;

	#isFirstRebuildStatusPoll: boolean = true;

	//Reload
	private async _reloadMemoryCache() {
		this._buttonStateReload = 'waiting';
		const { error } = await tryExecuteAndNotify(this, PublishedCacheService.postPublishedCacheReload());
		if (error) {
			this._buttonStateReload = 'failed';
		} else {
			this._buttonStateReload = 'success';
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
			this.#isFirstRebuildStatusPoll = true;
			this._pollForRebuildDatabaseCacheStatus();
		}
	}

	private async _pollForRebuildDatabaseCacheStatus() {
		//Checking the server after 1 second and then every 5 seconds to see if the database cache is still rebuilding.
		while (this._buttonStateRebuild === 'waiting') {
			await new Promise((resolve) => setTimeout(resolve, this.#isFirstRebuildStatusPoll ? 1000 : 5000));
			this.#isFirstRebuildStatusPoll = false;
			const { data, error } = await tryExecuteAndNotify(this, PublishedCacheService.getPublishedCacheRebuildStatus());
			if (error || !data) {
				this._buttonStateRebuild = 'failed';
				return;
			}

			if (!data.isRebuilding) {
				this._buttonStateRebuild = 'success';
			}
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

	override render() {
		return html`
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
