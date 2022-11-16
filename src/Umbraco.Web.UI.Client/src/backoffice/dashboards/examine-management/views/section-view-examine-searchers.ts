import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, state, query, property } from 'lit/decorators.js';

import { UmbModalService } from '../../../../core/services/modal';
import { UmbNotificationService } from '../../../../core/services/notification';
import { UmbNotificationDefaultData } from '../../../../core/services/notification/layouts/default';

import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';

import {
	ApiError,
	ProblemDetails,
	SearchResult,
	SearchResource,
	Field,
	PagedSearchResult,
} from '@umbraco-cms/backend-api';

import '../../../../core/services/modal/layouts/fields-viewer/fields-viewer.element';
import '../../../../core/services/modal/layouts/fields-viewer/fields-settings.element';

interface ExposedField {
	name: string;
	exposed: boolean;
}

@customElement('umb-dashboard-examine-searcher')
export class UmbDashboardExamineSearcherElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
			}
			uui-box {
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

			uui-table-cell:not(:last-child) {
				min-width: 100px;
			}

			button.bright {
				font-style: italic;
				color: var(--uui-color-positive-emphasis);
			}

			.field-adder {
				width: 0;
				line-height: 0;
				cursor: pointer;
			}

			.table-container {
				padding-bottom: var(--uui-size-space-4);
				max-width: calc(-336px + 100vw);
				overflow-x: scroll;
			}
		`,
	];

	private _notificationService?: UmbNotificationService;
	private _modalService?: UmbModalService;

	@property()
	searcherName!: string;

	@state()
	private _searchResults?: SearchResult[];

	@state()
	private _exposedFields?: ExposedField[];

	@query('#search-input')
	private _searchInput!: HTMLInputElement;

	constructor() {
		super();
		this.consumeContext('umbNotificationService', (notificationService: UmbNotificationService) => {
			this._notificationService = notificationService;
		});
		this.consumeContext('umbModalService', (modalService: UmbModalService) => {
			this._modalService = modalService;
		});
	}

	private _onNameClick() {
		const data: UmbNotificationDefaultData = { message: 'TODO: Open editor for this' }; // TODO
		this._notificationService?.peek('warning', { data });
	}

	private _onKeyPress(e: KeyboardEvent) {
		e.key == 'Enter' ? this._onSearch() : undefined;
	}

	private async _onSearch() {
		if (!this._searchInput.value.length) return;
		try {
			const res = await SearchResource.getSearchSearcherBySearcherNameSearch({
				searcherName: this.searcherName,
				query: this._searchInput.value,
				take: 100,
			});
			const pagedSearchResults = res.items as PagedSearchResult[];
			this._searchResults = pagedSearchResults[0].items;
			this._updateFieldFilter();
		} catch (e) {
			if (e instanceof ApiError) {
				const error = e as ProblemDetails;
				const data: UmbNotificationDefaultData = { message: error.message ?? 'Could not fetch search results' };
				this._notificationService?.peek('danger', { data });
			}
		}
	}

	private _updateFieldFilter() {
		this._searchResults?.map((result) => {
			const fieldNames = result.fields?.map((field) => {
				if (field.name) return { name: field.name, exposed: false } as ExposedField;
				return { name: '', exposed: false };
			});

			this._exposedFields = fieldNames?.map((field, i) => {
				return this._exposedFields
					? this._exposedFields[i].name == field.name
						? { name: this._exposedFields[i].name, exposed: this._exposedFields[i].exposed }
						: field
					: field;
			});
		});
	}

	private _onFieldFilterClick() {
		const modalHandler = this._modalService?.open('umb-modal-layout-fields-settings', {
			type: 'sidebar',
			size: 'small',
			data: { ...this._exposedFields },
		});
		modalHandler?.onClose().then(({ fields } = {}) => {
			if (!fields) return;
			this._exposedFields = fields;
		});
	}

	render() {
		return html`
			<uui-box headline="Search">
				<p>Search the ${this.searcherName} and view the results</p>
				<div class="flex">
					<uui-input
						id="search-input"
						placeholder="Type to filter..."
						label="Type to filter"
						@keypress=${this._onKeyPress}>
					</uui-input>
					<uui-button color="positive" look="primary" label="Search" @click="${this._onSearch}"> Search </uui-button>
				</div>
				${this.renderSearchResults()}
			</uui-box>
		`;
	}

	private renderSearchResults() {
		if (this._searchResults?.length) {
			return html` <uui-scroll-container class="table-container">
				<uui-table class="search">
					<uui-table-head>
						<uui-table-head-cell style="width:0">Score</uui-table-head-cell>
						<uui-table-head-cell style="width:0">Id</uui-table-head-cell>
						<uui-table-head-cell>Name</uui-table-head-cell>
						<uui-table-head-cell>Fields</uui-table-head-cell>
						${this.renderHeadCells()}
						<uui-table-head-cell style="width:0;" class="field-adder" @click="${this._onFieldFilterClick}">
							<uui-icon-registry-essential>
								<uui-tag look="secondary">
									<uui-icon name="add"></uui-icon>
								</uui-tag>
							</uui-icon-registry-essential>
						</uui-table-head-cell>
					</uui-table-head>
					${this._searchResults?.map((rowData) => {
						return html`<uui-table-row>
							<uui-table-cell> ${rowData.score} </uui-table-cell>
							<uui-table-cell> ${rowData.id} </uui-table-cell>
							<uui-table-cell>
								<uui-button look="secondary" label="Open editor for this document" @click="${this._onNameClick}">
									Document
								</uui-button>
							</uui-table-cell>
							<uui-table-cell>
								<uui-button
									class="bright"
									look="secondary"
									label="Open sidebar to see all fields"
									@click="${() =>
										this._modalService?.open('umb-modal-layout-fields-viewer', {
											type: 'sidebar',
											size: 'medium',
											data: { ...rowData },
										})}">
									${rowData.fields ? Object.keys(rowData.fields).length : ''} fields
								</uui-button>
							</uui-table-cell>
							${rowData.fields ? this.renderBodyCells(rowData.fields) : ''}
							<uui-table-cell></uui-table-cell>
						</uui-table-row>`;
					})}
				</uui-table>
			</uui-scroll-container>`;
		}
		return;
	}

	renderHeadCells() {
		return html`${this._exposedFields?.map((field) => {
			return field.exposed ? html`<uui-table-head-cell>${field.name}</uui-table-head-cell>` : html``;
		})}`;
	}

	renderBodyCells(cellData: Field[]) {
		return html`${this._exposedFields?.map((slot) => {
			return cellData.map((field) => {
				return slot.exposed && field.name == slot.name
					? html`<uui-table-cell clip-text>${field.values}</uui-table-cell>`
					: html``;
			});
		})}`;
	}
}

export default UmbDashboardExamineSearcherElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-examine-searcher': UmbDashboardExamineSearcherElement;
	}
}
