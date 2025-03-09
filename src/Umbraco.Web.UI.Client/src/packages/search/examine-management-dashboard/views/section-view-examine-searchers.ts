import { UMB_EXAMINE_FIELDS_SETTINGS_MODAL, UMB_EXAMINE_FIELDS_VIEWER_MODAL } from '../modal/constants.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, nothing, customElement, state, query, property } from '@umbraco-cms/backoffice/external/lit';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import type { SearchResultResponseModel, FieldPresentationModel } from '@umbraco-cms/backoffice/external/backend-api';
import { SearcherService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbLitElement, umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

interface ExposedSearchResultField {
	name: string;
	exposed: boolean;
}

@customElement('umb-dashboard-examine-searcher')
export class UmbDashboardExamineSearcherElement extends UmbLitElement {
	@property()
	searcherName!: string;

	@state()
	private _searchResults?: SearchResultResponseModel[];

	@state()
	private _exposedFields?: ExposedSearchResultField[];

	@state()
	private _searchLoading = false;

	@query('#search-input')
	private _searchInput!: HTMLInputElement;

	@state()
	private _workspacePath = 'aa';

	private _onKeyPress(e: KeyboardEvent) {
		if (e.key == 'Enter') {
			this._onSearch();
		}
	}

	#entityType = '';

	constructor() {
		super();
		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath(':entityType')
			.onSetup((routingInfo) => {
				return { data: { entityType: routingInfo.entityType, preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._workspacePath = routeBuilder({ entityType: this.#entityType });
			});
	}

	private async _onSearch() {
		if (!this._searchInput.value.length) return;
		this._searchLoading = true;

		const { data } = await tryExecuteAndNotify(
			this,
			SearcherService.getSearcherBySearcherNameQuery({
				searcherName: this.searcherName,
				term: this._searchInput.value,
				take: 100,
				skip: 0,
			}),
		);

		this._searchResults = data?.items ?? [];
		this._updateFieldFilter();
		this._searchLoading = false;
	}

	private _updateFieldFilter() {
		this._searchResults?.map((doc) => {
			const document = doc.fields?.filter((field) => {
				return field.name?.toUpperCase() !== 'NODENAME';
			});
			if (document) {
				const newFieldNames = document.map((field) => {
					return field.name ?? '';
				});

				// TODO: I don't get this code, not sure what the purpose is, it seems like a mistake: [NL]
				this._exposedFields = this._exposedFields
					? this._exposedFields.filter((field) => {
							return { name: field.name, exposed: field.exposed };
						})
					: newFieldNames?.map((name) => {
							return { name, exposed: false };
						});
			}
		});
	}

	async #onFieldFilterClick() {
		const value = await umbOpenModal(this, UMB_EXAMINE_FIELDS_SETTINGS_MODAL, {
			value: { fields: this._exposedFields ?? [] },
		}).catch(() => undefined);

		this._exposedFields = value?.fields;
	}

	async #onFieldViewClick(rowData: SearchResultResponseModel) {
		await umbOpenModal(this, UMB_EXAMINE_FIELDS_VIEWER_MODAL, {
			modal: {
				type: 'sidebar',
				size: 'medium',
			},
			data: { searchResult: rowData, name: this.getSearchResultNodeName(rowData) },
		}).catch(() => undefined);
	}

	override render() {
		return html`
			<uui-box headline=${this.localize.term('general_search')}>
				<p>
					<umb-localize key="examineManagement_searchDescription"
						>Search the ${this.searcherName} and view the results</umb-localize
					>
				</p>
				<div class="flex">
					<uui-input
						type="search"
						id="search-input"
						placeholder=${this.localize.term('placeholders_filter')}
						label=${this.localize.term('placeholders_filter')}
						@keypress=${this._onKeyPress}
						${umbFocus()}>
					</uui-input>
					<uui-button
						color="positive"
						look="primary"
						label=${this.localize.term('general_search')}
						@click="${this._onSearch}"></uui-button>
				</div>
				${this.renderSearchResults()}
			</uui-box>
		`;
	}

	// Find the field named 'nodeName' and return its value if it exists in the fields array
	private getSearchResultNodeName(searchResult: SearchResultResponseModel): string {
		const nodeNameField = searchResult.fields?.find((field) => field.name?.toUpperCase() === 'NODENAME');
		return nodeNameField?.values?.join(', ') ?? '';
	}

	#getEntityTypeFromIndexType(indexType: string) {
		switch (indexType) {
			case 'content':
				return 'document';
			default:
				return indexType;
		}
	}

	private renderSearchResults() {
		if (this._searchLoading) return html`<uui-loader></uui-loader>`;
		if (!this._searchResults) return nothing;
		if (!this._searchResults.length) {
			return html`<p>${this.localize.term('examineManagement_noResults')}</p>`;
		}
		return html`<div class="table-container">
			<uui-scroll-container>
				<uui-table class="search">
					<uui-table-head>
						<uui-table-head-cell style="width:0">Score</uui-table-head-cell>
						<uui-table-head-cell style="width:0">${this.localize.term('general_id')}</uui-table-head-cell>
						<uui-table-head-cell>${this.localize.term('general_name')}</uui-table-head-cell>
						<uui-table-head-cell>${this.localize.term('examineManagement_fields')}</uui-table-head-cell>
						${this.renderHeadCells()}
					</uui-table-head>
					${this._searchResults?.map((rowData) => {
						const indexType = rowData.fields?.find((field) => field.name === '__IndexType')?.values?.join(', ') ?? '';
						this.#entityType = this.#getEntityTypeFromIndexType(indexType);
						const unique = rowData.fields?.find((field) => field.name === '__Key')?.values?.join(', ') ?? '';

						return html`<uui-table-row>
							<uui-table-cell> ${rowData.score} </uui-table-cell>
							<uui-table-cell> ${rowData.id} </uui-table-cell>
							<uui-table-cell>
								<uui-button
									look="secondary"
									label=${this.localize.term('actions_editContent')}
									href=${this._workspacePath + this.#entityType + '/edit/' + unique}>
									${this.getSearchResultNodeName(rowData)}
								</uui-button>
							</uui-table-cell>
							<uui-table-cell>
								<uui-button
									class="bright"
									look="secondary"
									label=${this.localize.term('examineManagement_fieldValues')}
									@click=${() => this.#onFieldViewClick(rowData)}>
									${rowData.fields ? Object.keys(rowData.fields).length : ''}
									${this.localize.term('examineManagement_fields')}
								</uui-button>
							</uui-table-cell>
							${rowData.fields ? this.renderBodyCells(rowData.fields) : ''}
						</uui-table-row>`;
					})}
				</uui-table>
			</uui-scroll-container>
			<button class="field-adder" @click="${this.#onFieldFilterClick}">
				<uui-icon-registry-essential>
					<uui-tag look="secondary">
						<uui-icon name="add"></uui-icon>
					</uui-tag>
				</uui-icon-registry-essential>
			</button>
		</div>`;
	}

	renderHeadCells() {
		return html`${this._exposedFields?.map((field) => {
			return field.exposed
				? html`<uui-table-head-cell class="exposed-field">
						<div class="exposed-field-container">
							<span>${field.name}</span>
							<uui-button
								look="secondary"
								label="${this.localize.term('actions_remove')} ${field.name}"
								compact
								@click="${() => {
									this._exposedFields = this._exposedFields?.map((f) => {
										return f.name === field.name ? { name: f.name, exposed: false } : f;
									});
								}}"
								>x</uui-button
							>
						</div>
					</uui-table-head-cell>`
				: html``;
		})}`;
	}

	renderBodyCells(cellData: FieldPresentationModel[]) {
		return html`${this._exposedFields?.map((slot) => {
			return cellData.map((field) => {
				return slot.exposed && field.name == slot.name
					? html`<uui-table-cell clip-text>${field.values}</uui-table-cell>`
					: html``;
			});
		})}`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
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

			uui-table-cell {
				min-width: 100px;
			}

			button.bright {
				font-style: italic;
				color: var(--uui-color-positive-emphasis);
			}

			.field-adder {
				line-height: 0;
				cursor: pointer;
				vertical-align: top;
				background: transparent;
				border: none;
			}

			.table-container uui-scroll-container {
				padding-bottom: var(--uui-size-space-4);
				max-width: 100%;
				overflow-x: scroll;
				flex: 1;
			}

			.table-container {
				display: flex;
				align-items: flex-start;
			}
			uui-tag {
				margin-block: var(--uui-size-5, 15px);
			}

			.exposed-field uui-button {
				align-items: center;
				font-weight: normal;
				font-size: 10px;
				height: 10px;
				width: 10px;
				margin-top: -5px;
			}

			.exposed-field-container {
				display: flex;
				justify-content: space-between;
			}
		`,
	];
}

export default UmbDashboardExamineSearcherElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-examine-searcher': UmbDashboardExamineSearcherElement;
	}
}
