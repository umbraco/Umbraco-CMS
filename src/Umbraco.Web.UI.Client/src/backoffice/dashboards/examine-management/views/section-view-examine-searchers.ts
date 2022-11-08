import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, state, query, property } from 'lit/decorators.js';

import { UmbModalService } from '../../../../core/services/modal';
import { UmbNotificationService } from '../../../../core/services/notification';
import { UmbNotificationDefaultData } from '../../../../core/services/notification/layouts/default';

import { FieldViewModel, SearchResultsModel } from '../examine-extension';

import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { getSearchResults } from '@umbraco-cms/backend-api';

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

			uui-radio-group {
				display: flex;
			}

			uui-radio {
				padding-right: var(--uui-size-space-5);
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
		`,
	];

	private _notificationService?: UmbNotificationService;
	private _modalService?: UmbModalService;

	@property()
	searcherName!: string;

	@state()
	private _searchResults?: SearchResultsModel[];

	@state()
	private _fields?: ExposedField[];

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
			const res = await getSearchResults({
				searcherName: this.searcherName,
				query: this._searchInput.value,
				take: 100,
			});
			this._searchResults = res.data as SearchResultsModel[];
			this._updateFieldFilter();
		} catch (e) {
			if (e instanceof getSearchResults.Error) {
				const error = e.getActualType();
				const data: UmbNotificationDefaultData = { message: error.data.detail ?? 'Could not fetch search results' };
				this._notificationService?.peek('danger', { data });
			}
		}
	}

	private _updateFieldFilter() {
		this._searchResults?.map((result) => {
			const fieldNames = result.fields.map((field) => {
				return { name: field.name, exposed: false };
			});

			this._fields = fieldNames.map((field, i) => {
				return this._fields
					? this._fields[i].name == field.name
						? { name: this._fields[i].name, exposed: this._fields[i].exposed }
						: field
					: field;
			});
		});
	}

	private _onFieldFilterClick() {
		const modalHandler = this._modalService?.open('umb-modal-layout-fields-settings', {
			type: 'sidebar',
			size: 'small',
			data: { ...this._fields },
		});
		modalHandler?.onClose().then(({ fields } = {}) => {
			if (!fields) return;
			this._fields = fields;
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
			return html` <uui-table class="search">
				<uui-table-head>
					<uui-table-head-cell style="width:0">Score</uui-table-head-cell>
					<uui-table-head-cell style="width:0">Id</uui-table-head-cell>
					<uui-table-head-cell>Name</uui-table-head-cell>
					<uui-table-head-cell style="width:200px;">Fields</uui-table-head-cell>
					${this.renderHeadCells()}
					<uui-table-head-cell class="field-adder" @click="${this._onFieldFilterClick}">
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
								${rowData.name}
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
								${Object.keys(rowData.fields).length} fields
							</uui-button>
						</uui-table-cell>
						${this.renderBodyCells(rowData.fields)}
						<uui-table-cell></uui-table-cell>
					</uui-table-row>`;
				})}
			</uui-table>`;
		}
		return;
	}

	renderHeadCells() {
		return html`${this._fields?.map((field) => {
			return field.exposed ? html`<uui-table-head-cell>${field.name}</uui-table-head-cell>` : html``;
		})}`;
	}

	renderBodyCells(cellData: FieldViewModel[]) {
		return html`${this._fields?.map((field) => {
			return cellData.map((option) => {
				return option.name == field.name && field.exposed
					? html`<uui-table-cell>${option.values}</uui-table-cell>`
					: ``;
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
