import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, state, query } from 'lit/decorators.js';

import { Indexer, Searcher } from '../../../core/mocks/data/examine.data';

import { UmbModalService } from '../../../core/services/modal';
import { UmbNotificationService } from '../../../core/services/notification';
import { UmbNotificationDefaultData } from '../../../core/services/notification/layouts/default';
import { getIndexers, getSearchers } from '@umbraco-cms/backend-api';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';

import '../examine-management/dashboard-examine-management-index.element';
import '../examine-management/dashboard-examine-management-searcher.element';

type ExaminePageType = 'Index' | 'Searcher';

@customElement('umb-dashboard-examine-management')
export class UmbDashboardExamineManagementElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			uui-box + uui-box {
				margin-top: var(--uui-size-space-5);
			}

			uui-box p {
				margin-top: 0;
			}

			button {
				background: transparent;
				border: none;
				text-decoration: underline;
				cursor: pointer;
			}

			button.back {
				margin-bottom: var(--uui-size-space-5);
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
	private _indexers?: Indexer[];

	@state()
	private _searchers?: Searcher[];

	@state()
	private _page?: { type: ExaminePageType; name: string };

	@query('#pop')
	private _popover!: HTMLElement;

	private _notificationService?: UmbNotificationService;
	private _modalService?: UmbModalService;

	private async _getIndexers() {
		try {
			const indexers = await getIndexers({});
			this._indexers = indexers.data as Indexer[];
		} catch (e) {
			if (e instanceof getIndexers.Error) {
				const error = e.getActualType();
				const data: UmbNotificationDefaultData = { message: error.data.detail ?? 'Could not fetch indexers' };
				this._notificationService?.peek('danger', { data });
			}
		}
	}

	private async _getSearchers() {
		try {
			const searchers = await getSearchers({});
			this._searchers = searchers.data as Searcher[];
		} catch (e) {
			if (e instanceof getSearchers.Error) {
				const error = e.getActualType();
				const data: UmbNotificationDefaultData = { message: error.data.detail ?? 'Could not fetch searchers' };
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
		if (this._page?.type == 'Index') {
			return html` <button class="back" @click="${() => (this._page = undefined)}">&larr; Back to overview</button>
				<examine-management-index indexName="${this._page.name}"></examine-management-index>`;
		} else if (this._page?.type == 'Searcher') {
			return html`<button class="back" @click="${() => (this._page = undefined)}">&larr; Back to overview</button>
				<examine-management-searchers searcherName="${this._page.name}"></examine-management-searchers>`;
		} else {
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
							<button @click="${() => (this._page = { type: 'Index', name: index.name })}">${index.name}</button>
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
						<uui-table-cell><button @click="${() => (this._page = { type: 'Searcher', name: searcher.name })}">
						${searcher.name}</button>
					</uui-table-cell>
					</uui-table-row>`;
				})}
			</uui-table>
		`;
	}
}

export default UmbDashboardExamineManagementElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-examine-management': UmbDashboardExamineManagementElement;
	}
}
