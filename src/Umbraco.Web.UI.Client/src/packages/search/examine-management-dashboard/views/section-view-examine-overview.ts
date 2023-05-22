import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';

import {
	HealthStatusModel,
	IndexResponseModel,
	IndexerResource,
	SearcherResponseModel,
	SearcherResource,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
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

	connectedCallback() {
		super.connectedCallback();
		this._getIndexers();
		this._getSearchers();
	}

	private async _getIndexers() {
		this._loadingIndexers = true;
		const { data } = await tryExecuteAndNotify(this, IndexerResource.getIndexer({ take: 9999, skip: 0 }));
		this._indexers = data?.items ?? [];
		this._loadingIndexers = false;
	}

	private async _getSearchers() {
		this._loadingSearchers = true;
		const { data } = await tryExecuteAndNotify(this, SearcherResource.getSearcher({ take: 9999, skip: 0 }));
		this._searchers = data?.items ?? [];
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
							${
								index.healthStatus === HealthStatusModel.UNHEALTHY
									? html`<uui-icon name="wrong" class="danger"></uui-icon>`
									: html`<uui-icon name="check" class="positive"></uui-icon>`
							}
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

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
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
