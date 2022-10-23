import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, query, state, property } from 'lit/decorators.js';

import { UUIButtonState } from '@umbraco-ui/uui';
import { getIndex, postIndexRebuild, getSearchResultFromIndex } from '../../../core/api/fetcher';

import { Indexer, SearchResult } from '../../../mocks/data/examine.data';

import { UmbContextConsumerMixin } from '../../../core/context';
import { UmbModalService } from '../../../core/services/modal';
import { UmbNotificationService } from '../../../core/services/notification';
import { UmbNotificationDefaultData } from '../../../core/services/notification/layouts/default';
import { UmbModalLayoutElement } from '../../../core/services/modal/layouts/modal-layout.element';

@customElement('examine-management-index')
export class UmbDashboardExamineManagementIndexElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
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

			div.flex {
				display: flex;
			}
			div.flex > uui-button {
				padding-left: var(--uui-size-space-4);
				height: 0;
			}

			uui-input {
				width: 100%;
				margin-bottom: var(--uui-size-space-5);
			}

			uui-table.info uui-table-row:first-child uui-table-cell {
				border-top: none;
			}

			uui-table-head-cell {
				text-transform: capitalize;
			}

			uui-table-row:last-child uui-table-cell {
				padding-bottom: 0;
			}

			uui-icon {
				vertical-align: top;
				padding-right: var(--uui-size-space-5);
			}

			.positive {
				color: var(--uui-color-positive);
			}
			.danger {
				color: var(--uui-color-danger);
			}

			button {
				background: none;
				border: none;
				text-decoration: underline;
				cursor: pointer;
			}
			button.bright {
				font-style: italic;
				color: var(--uui-color-positive-emphasis);
			}
		`,
	];

	@property()
	indexName!: string;

	@state()
	private _searchResults?: SearchResult[];

	@state()
	private _buttonState?: UUIButtonState = undefined;

	@query('#search')
	private _searchInput!: HTMLInputElement;

	@state()
	private _indexData!: Indexer;

	private _notificationService?: UmbNotificationService;
	private _modalService?: UmbModalService;

	private async _getIndexData() {
		try {
			const index = await getIndex({ indexName: this.indexName });
			this._indexData = index.data as Indexer;
		} catch (e) {
			if (e instanceof getIndex.Error) {
				const error = e.getActualType();
				const data: UmbNotificationDefaultData = { message: error.data.detail ?? 'Could not fetch index' };
				this._notificationService?.peek('danger', { data });
			}
		}
	}

	constructor() {
		super();

		this.consumeContext('umbNotificationService', (notificationService: UmbNotificationService) => {
			this._notificationService = notificationService;
		});
		this.consumeContext('umbModalService', (modalService: UmbModalService) => {
			this._modalService = modalService;
		});
	}

	connectedCallback(): void {
		super.connectedCallback();
		this._getIndexData();
	}

	private _onKeyPress(e: KeyboardEvent) {
		if (e.key == 'Enter') this._onSearch();
	}

	private async _onSearch() {
		if (this._searchInput.value.length) {
			try {
				const res = await getSearchResultFromIndex({
					indexName: this._indexData.name,
					searchQuery: this._searchInput.value,
				});
				this._searchResults = res.data as SearchResult[];
				console.log(this._searchResults);
			} catch (e) {
				if (e instanceof getSearchResultFromIndex.Error) {
					const error = e.getActualType();
					const data: UmbNotificationDefaultData = { message: error.data.detail ?? 'Could not fetch search results' };
					this._notificationService?.peek('danger', { data });
				}
			}
		} else this._searchResults = undefined;
	}

	private async _onRebuildHandler() {
		const modalHandler = this._modalService?.confirm({
			headline: `Rebuild ${this.indexName}`,
			content: html`
				This will cause the index to be rebuilt.<br />
				Depending on how much content there is in your site this could take a while.<br />
				It is not recommended to rebuild an index during times of high website traffic or when editors are editing
				content.
			`,
			color: 'danger',
			confirmLabel: 'Rebuild',
		});
		modalHandler?.onClose().then(({ confirmed }) => {
			if (confirmed) this._rebuild();
		});
	}
	private async _rebuild() {
		this._buttonState = 'waiting';
		try {
			await postIndexRebuild({ indexName: this._indexData.name });
			this._buttonState = 'success';
		} catch (e) {
			this._buttonState = 'failed';
			if (e instanceof postIndexRebuild.Error) {
				const error = e.getActualType();
				const data: UmbNotificationDefaultData = { message: error.data.detail ?? 'Rebuild error' };
				this._notificationService?.peek('danger', { data });
			}
		}
	}

	render() {
		if (this._indexData) {
			return html`<uui-box headline="${this.indexName}">
					<p>
						<strong>Health Status</strong><br />
						The health status of the ${this._indexData.name} and if it can be read
					</p>
					<div>
						<uui-icon-essentials>
							<uui-icon
								name=${this._indexData.isHealthy ? `check` : `wrong`}
								class=${this._indexData.isHealthy ? 'positive' : 'danger'}>
							</uui-icon>
						</uui-icon-essentials>
						${this._indexData.healthStatus}
					</div>
				</uui-box>
				<uui-box headline="Search">
					<p>Search the ${this._indexData.name} and view the results</p>
					<div class="flex">
						<uui-input
							id="search"
							placeholder="Type to filter..."
							label="Type to filter"
							@keypress="${this._onKeyPress}">
						</uui-input>
						<uui-button color="positive" look="primary" label="Search" @click=${this._onSearch}> Search </uui-button>
					</div>
					${this.renderSearchResults()}
				</uui-box>
				${this.renderPropertyList()} ${this.renderTools()}`;
		} else return;
	}

	private renderSearchResults() {
		if (this._searchResults?.length) {
			return html` <uui-table class="search">
				<uui-table-head>
					<uui-table-head-cell style="width:0px">Id</uui-table-head-cell>
					<uui-table-head-cell>Name</uui-table-head-cell>
					<uui-table-head-cell style="width:0; min-width:100px;">Fields</uui-table-head-cell>
					<uui-table-head-cell style="width:0px">Score</uui-table-head-cell>
				</uui-table-head>
				${this._searchResults?.map((rowData) => {
					return html`<uui-table-row>
						<uui-table-cell> ${rowData.id} </uui-table-cell>
						<uui-table-cell><button>${rowData.name}</button></uui-table-cell>
						<uui-table-cell>
							<button
								class="bright"
								@click="${() =>
									this._modalService?.open('umb-modal-layout-fields-viewer', {
										type: 'sidebar',
										size: 'medium',
										data: { document: rowData, values: rowData.fields },
									})}">
								(${Object.keys(rowData.fields).length} fields)
							</button>
						</uui-table-cell>
						<uui-table-cell> ${rowData.score} </uui-table-cell>
					</uui-table-row>`;
				})}
			</uui-table>`;
		}
		return;
	}

	private renderPropertyList() {
		return html`<uui-box headline="Index info">
			<p>Lists the properties of the ${this._indexData.name}</p>
			<uui-table class="info">
				${Object.entries(this._indexData.providerProperties).map((entry) => {
					return html`<uui-table-row>
						<uui-table-cell style="width:0px; font-weight: bold;"> ${entry[0]} </uui-table-cell>
						<uui-table-cell> ${JSON.stringify(entry[1]).replace(/,/g, ', ')} </uui-table-cell>
					</uui-table-row>`;
				})}
			</uui-table>
		</uui-box>`;
	}

	private renderTools() {
		return html` <uui-box headline="Tools">
			<p>Tools to manage the ${this._indexData.name}</p>
			<uui-button
				color="danger"
				look="primary"
				.state="${this._buttonState}"
				@click="${this._onRebuildHandler}"
				.disabled="${!this._indexData?.canRebuild}"
				label="Rebuild index">
				Rebuild
			</uui-button>
		</uui-box>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'examine-management-index': UmbDashboardExamineManagementIndexElement;
	}
}
