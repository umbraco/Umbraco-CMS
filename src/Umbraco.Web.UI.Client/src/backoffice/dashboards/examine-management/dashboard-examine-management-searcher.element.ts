import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, query, state, property } from 'lit/decorators.js';

import { UUIButtonState } from '@umbraco-ui/uui';
import { getSearchResultFromSearchers } from '../../../core/api/fetcher';

import { SearchResult } from '../../../mocks/data/examine.data';

import { UmbContextConsumerMixin } from '../../../core/context';
import { UmbModalService } from '../../../core/services/modal';
import { UmbNotificationService } from '../../../core/services/notification';
import { UmbNotificationDefaultData } from '../../../core/services/notification/layouts/default';
import '../../../core/services/modal/layouts/fields-viewer/fields-viewer.element';

@customElement('examine-management-searchers')
export class UmbDashboardExamineManagementSearcherElement extends UmbContextConsumerMixin(LitElement) {
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

			uui-table-head-cell {
				text-transform: capitalize;
			}

			uui-table-row:last-child uui-table-cell {
				padding-bottom: 0;
			}

			uui-radio-group {
				display: flex;
			}

			uui-radio {
				padding-right: var(--uui-size-space-5);
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
	searcherName!: string;

	@state()
	private _searchResults?: SearchResult[];

	@query('#search')
	private _searchInput!: HTMLInputElement;

	private _notificationService?: UmbNotificationService;
	private _modalService?: UmbModalService;

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
	}

	private _onKeyPress(e: KeyboardEvent) {
		if (e.key == 'Enter') this._onSearch();
	}

	private async _onSearch() {
		if (!this._searchInput.value.length) return;
		try {
			const res = await getSearchResultFromSearchers({
				searcherName: this.searcherName,
				searchQuery: this._searchInput.value,
			});
			this._searchResults = res.data as SearchResult[];
		} catch (e) {
			if (e instanceof getSearchResultFromSearchers.Error) {
				const error = e.getActualType();
				const data: UmbNotificationDefaultData = { message: error.data.detail ?? 'Could not fetch search results' };
				this._notificationService?.peek('danger', { data });
			}
		}
	}

	private _onNameClick() {
		const data: UmbNotificationDefaultData = { message: 'TODO: Open editor for this' }; //TODO
		this._notificationService?.peek('warning', { data });
	}

	render() {
		return html`<uui-box headline="${this.searcherName}">
			<p><strong>Search tools</strong></p>
			<div class="flex">
				<uui-input
					id="search"
					placeholder="Type to search"
					label="Type to search"
					@keydown="${this._onKeyPress}"></uui-input>
				<uui-button label="search" look="primary" color="positive" @click="${this._onSearch}">Search</uui-button>
			</div>
			${this.renderSearchResults()}
		</uui-box>`;
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
						<uui-table-cell><button @click="${this._onNameClick}">${rowData.name}</button></uui-table-cell>
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
}
declare global {
	interface HTMLElementTagNameMap {
		'examine-management-searchers': UmbDashboardExamineManagementSearcherElement;
	}
}
