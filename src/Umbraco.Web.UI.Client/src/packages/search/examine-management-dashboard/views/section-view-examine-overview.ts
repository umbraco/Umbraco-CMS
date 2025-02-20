import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, nothing, customElement, state } from '@umbraco-cms/backoffice/external/lit';

import type { IndexResponseModel, SearcherResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { HealthStatusModel, IndexerService, SearcherService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

@customElement('umb-dashboard-examine-overview')
export class UmbDashboardExamineOverviewElement extends UmbLitElement {
	@state()
	private _indexers?: IndexResponseModel[];

	@state()
	private _searchers?: SearcherResponseModel[];

	@state()
	private _loadingIndexers = false;

	@state()
	private _loadingSearchers = false;

	override connectedCallback() {
		super.connectedCallback();
		this._getIndexers();
		this._getSearchers();
	}

	private async _getIndexers() {
		this._loadingIndexers = true;
		const { data } = await tryExecuteAndNotify(this, IndexerService.getIndexer({ take: 9999, skip: 0 }));
		this._indexers = data?.items ?? [];
		this._loadingIndexers = false;
	}

	private async _getSearchers() {
		this._loadingSearchers = true;
		const { data } = await tryExecuteAndNotify(this, SearcherService.getSearcher({ take: 9999, skip: 0 }));
		this._searchers = data?.items ?? [];
		this._loadingSearchers = false;
	}

	#renderStatus(status: HealthStatusModel) {
		switch (status) {
			case HealthStatusModel.HEALTHY:
				return html`<umb-icon name="icon-check color-green"></umb-icon>`;
			case HealthStatusModel.CORRUPT:
			case HealthStatusModel.UNHEALTHY:
				return html`<umb-icon name="icon-alert color-red"></umb-icon>`;
			case HealthStatusModel.REBUILDING:
				return html`<umb-icon name="icon-time color-yellow"></umb-icon>`;
			default:
				return;
		}
	}

	override render() {
		return html`
			<uui-box headline=${this.localize.term('examineManagement_indexers')} class="overview">
				<p>
					<strong><umb-localize key="examineManagement_manageIndexes">Manage Examine's indexes</umb-localize></strong
					><br />
					<umb-localize key="examineManagement_manageIndexesDescription"
						>Allows you to view the details of each index and provides some tools for managing the indexes</umb-localize
					>
				</p>
				${this.renderIndexersList()}
			</uui-box>
			<uui-box headline=${this.localize.term('examineManagement_searchers')}>
				<p>
					<strong><umb-localize key="examineManagement_configuredSearchers">Configured Searchers</umb-localize></strong
					><br />
					<umb-localize key="examineManagement_configuredSearchersDescription"
						>Shows properties and tools for any configured Searcher (i.e. such as a multi-index searcher)</umb-localize
					>
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
						<uui-table-cell style="width:0px"> ${this.#renderStatus(index.healthStatus.status)} </uui-table-cell>
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

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
			}

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
}

export default UmbDashboardExamineOverviewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-examine-overview': UmbDashboardExamineOverviewElement;
	}
}
