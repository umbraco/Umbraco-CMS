import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';

import { UmbModalService } from '../../../../core/services/modal';
import { UmbNotificationService } from '../../../../core/services/notification';
import { UmbNotificationDefaultData } from '../../../../core/services/notification/layouts/default';

import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { ApiError, ProblemDetails, Searcher, Index, SearchResource } from '@umbraco-cms/backend-api';

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

	private _notificationService?: UmbNotificationService;
	private _modalService?: UmbModalService;

	private async _getIndexers() {
		try {
			const indexers = await SearchResource.getSearchIndex({ take: 9999, skip: 0 });
			this._indexers = indexers.items as Index[];
		} catch (e) {
			if (e instanceof ApiError) {
				const error = e as ProblemDetails;
				const data: UmbNotificationDefaultData = { message: error.message ?? 'Could not fetch indexers' };
				this._notificationService?.peek('danger', { data });
			}
		}
	}

	private async _getSearchers() {
		try {
			const searchers = await SearchResource.getSearchSearcher({ take: 9999, skip: 0 });
			this._searchers = searchers.items as Searcher[];
		} catch (e) {
			if (e instanceof ApiError) {
				const error = e as ProblemDetails;
				const data: UmbNotificationDefaultData = { message: error.message ?? 'Could not fetch searchers' };
				this._notificationService?.peek('danger', { data });
			}
		}
	}

	constructor() {
		super();
		this._getIndexers();
		this._getSearchers();

		this.consumeContext('umbNotificationService', (notificationService: UmbNotificationService) => {
			this._notificationService = notificationService;
		});
		this.consumeContext('umbModalService', (modalService: UmbModalService) => {
			this._modalService = modalService;
		});
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
		if (!this._indexers) return;
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
							<a href="/section/settings/dashboard/examine-management/index/${index.name}">${index.name}</a>
						</uui-table-cell>
					</uui-table-row>
				`;
			})}
		</uui-table>`;
	}

	private renderSearchersList() {
		if (!this._searchers) return html`<span class="not-found-message">No searchers were found</span>`;
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
						<uui-table-cell><a href="/section/settings/dashboard/examine-management/searcher/${searcher.name}">${searcher.name}</a>
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
