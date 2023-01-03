import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';

import { UmbNotificationService } from '../../../../../core/notification';
import { UmbNotificationDefaultData } from '../../../../../core/notification/layouts/default';

import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { ApiError, ProblemDetails, Searcher, Index, IndexerResource, SearcherResource } from '@umbraco-cms/backend-api';

@customElement('umb-dashboard-examine-overview')
export class UmbDashboardExamineOverviewElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			uui-box + uui-box {
				margin-top: var(--uui-size-space-5);
			}

			uui-box p {
				margin-top: 0;
			}

			a {
				color: var(--uui-color-text);
				background: transparent;
				border: none;
				text-decoration: underline;
				cursor: pointer;
			}

			uui-table-cell {
				line-height: 0;
				vertical-align: middle;
			}

			uui-table-row:last-child uui-table-cell {
				padding-bottom: 0;
			}

			.positive {
				color: var(--uui-color-positive);
			}

			.danger {
				color: var(--uui-color-danger);
			}

			.not-found-message {
				font-style: italic;
				color: var(--uui-color-text-alt);
			}
		`,
	];

	@state()
	private _indexers?: Index[];

	@state()
	private _searchers?: Searcher[];

	@state()
	private _loadingIndexers = false;

	@state()
	private _loadingSearchers = false;

	private _notificationService?: UmbNotificationService;

	constructor() {
		super();

		this.consumeAllContexts(['umbNotificationService'], (instances) => {
			this._notificationService = instances['umbNotificationService'];
		});
	}

	connectedCallback() {
		super.connectedCallback();
		this._getIndexers();
		this._getSearchers();
	}

	private async _getIndexers() {
		this._loadingIndexers = true;
		try {
			const indexers = await IndexerResource.getIndexer({ take: 9999, skip: 0 });
			this._indexers = indexers.items;
		} catch (e) {
			if (e instanceof ApiError) {
				const error = e as ProblemDetails;
				const data: UmbNotificationDefaultData = { message: error.message ?? 'Could not fetch indexers' };
				this._notificationService?.peek('danger', { data });
			}
		}
		this._loadingIndexers = false;
	}

	private async _getSearchers() {
		this._loadingSearchers = true;
		try {
			const searchers = await SearcherResource.getSearcher({ take: 9999, skip: 0 });
			this._searchers = searchers.items;
		} catch (e) {
			if (e instanceof ApiError) {
				const error = e as ProblemDetails;
				const data: UmbNotificationDefaultData = { message: error.message ?? 'Could not fetch searchers' };
				this._notificationService?.peek('danger', { data });
			}
		}
		this._loadingSearchers = false;
	}

	render() {
		return html`
			<uui-box headline="Indexers" class="overview">
				<p>
					<strong>Manage Examine's indexes</strong><br />
					Allows you to view the details of each index and provides some tools for managing the indexes
				</p>
				${this.renderIndexersList()}
			</uui-box>
			<uui-box headline="Searchers">
				<p>
					<strong>Configured Searchers</strong><br />
					Shows properties and tools for any configured Searcher (i.e. such as a multi-index searcher)
				</p>
				${this.renderSearchersList()}
			</uui-box>
		`;
	}

	private renderIndexersList() {
		if (this._loadingIndexers) return html`<uui-loader></uui-loader>`;
		if (!this._indexers) return nothing;
		return html` <uui-table class="overview">
			${this._indexers.map((index) => {
				return html`
					<uui-table-row>
						<uui-table-cell style="width:0px">
							<uui-icon-essentials>
								<uui-icon
									style="vertical-align: top"
									name=${index.isHealthy ? `check` : `wrong`}
									class=${index.isHealthy ? 'positive' : 'danger'}>
								</uui-icon>
							</uui-icon-essentials>
						</uui-table-cell>
						<uui-table-cell>
							<a href="${window.location.href.replace(/\/+$/, '')}/index/${index.name}">${index.name}</a>
						</uui-table-cell>
					</uui-table-row>
				`;
			})}
		</uui-table>`;
	}

	private renderSearchersList() {
		if (this._loadingSearchers) return html`<uui-loader></uui-loader>`;
		if (!this._searchers) return nothing;
		return html`
			<uui-table class="overview2">
				${this._searchers.map((searcher) => {
					return html`<uui-table-row>
							<uui-table-cell style="width:0px">
								<uui-icon-essentials>
									<uui-icon
										style="vertical-align: top"
										name="info"></uui-icon>
									</uui-icon>
								</uui-icon-essentials>
							</uui-table-cell>
						<uui-table-cell><a href="${window.location.href.replace(/\/+$/, '')}/searcher/${searcher.name}">${searcher.name}</a>
					</uui-table-cell>
					</uui-table-row>`;
				})}
			</uui-table>
		`;
	}
}

export default UmbDashboardExamineOverviewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-examine-overview': UmbDashboardExamineOverviewElement;
	}
}
