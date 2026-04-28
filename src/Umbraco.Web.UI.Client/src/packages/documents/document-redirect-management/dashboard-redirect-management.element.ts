import {
	css,
	customElement,
	html,
	nothing,
	property,
	query,
	repeat,
	state,
	when,
} from '@umbraco-cms/backoffice/external/lit';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UmbDocumentRedirectManagementRepository } from '@umbraco-cms/backoffice/document';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbDocumentRedirectUrlModel } from '@umbraco-cms/backoffice/document';
import type { UUIButtonState, UUIPaginationElement, UUIPaginationEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-dashboard-redirect-management')
export class UmbDashboardRedirectManagementElement extends UmbLitElement {
	@property({ type: Number, attribute: 'items-per-page' })
	itemsPerPage = 20;

	@property({ type: Number })
	page = 1;

	@state()
	private _trackerEnabled = true;

	@state()
	private _total = 0;

	@state()
	private _redirectData?: Array<UmbDocumentRedirectUrlModel>;

	@state()
	private _buttonState: UUIButtonState;

	@state()
	private _filter?: string;

	@query('#search')
	private _search!: HTMLInputElement;

	@query('uui-pagination')
	private _pagination?: UUIPaginationElement;

	#repository = new UmbDocumentRedirectManagementRepository(this);

	override connectedCallback() {
		super.connectedCallback();
		this.#getTrackerStatus();
		this.#getRedirectData();
	}

	async #getTrackerStatus() {
		const { data } = await this.#repository.requestStatus();
		if (data) this._trackerEnabled = data.enabled;
	}

	// Fetch data
	async #getRedirectData(filter: string | undefined = undefined) {
		const skip = this.page * this.itemsPerPage - this.itemsPerPage;
		const { data } = await this.#repository.requestRedirects({ filter, take: this.itemsPerPage, skip });
		if (!data) return;

		this._total = data.total;
		this._redirectData = data.items;

		if (filter !== undefined) this._buttonState = 'success';
	}

	// Pagination
	#onPageChange(event: UUIPaginationEvent) {
		if (this.page === event.target.current) return;
		this.page = event.target.current;
		this.#getRedirectData();
	}

	// Delete Redirect Action
	async #onRequestDelete(data: UmbDocumentRedirectUrlModel) {
		if (!data.unique) return;

		await umbConfirmModal(this, {
			headline: '#general_delete',
			content: html`
				<p>${this.localize.term('redirectUrls_redirectRemoveWarning')}</p>
				<p>
					${this.localize.term('redirectUrls_originalUrl')}: <strong>${data.originalUrl}</strong><br />
					${this.localize.term('redirectUrls_redirectedTo')}: <strong>${data.destinationUrl}</strong>
				</p>
			`,
			color: 'danger',
			confirmLabel: '#general_delete',
		});

		this.#redirectDelete(data.unique);
	}
	async #redirectDelete(unique: string) {
		const { error } = await this.#repository.delete(unique);
		if (error) return;

		this._redirectData = this._redirectData?.filter((x) => x.unique !== unique);
	}

	// Search action
	#onKeypress(e: KeyboardEvent) {
		if (e.key === 'Enter') this.#onSearch();
	}
	#onSearch() {
		this._buttonState = 'waiting';
		this._filter = this._search?.value ?? '';
		if (this._pagination) this._pagination.current = 1;
		this.page = 1;

		this.#getRedirectData(this._search.value);
	}

	// Tracker disable/enable
	async #onRequestTrackerToggle() {
		if (!this._trackerEnabled) {
			this.#trackerToggle();
			return;
		}

		await umbConfirmModal(this, {
			headline: '#redirectUrls_disableUrlTracker',
			content: '#redirectUrls_confirmDisable',
			color: 'danger',
			confirmLabel: '#actions_disable',
		});

		this.#trackerToggle();
	}

	async #trackerToggle() {
		const { error } = await this.#repository.setStatus(!this._trackerEnabled);
		if (error) return;
		this._trackerEnabled = !this._trackerEnabled;
	}

	override render() {
		return html`
			<div id="redirect-actions">
				${when(
					this._trackerEnabled,
					() => html`
						<div id="search-wrapper">
							<uui-input
								id="search"
								label=${this.localize.term('redirectUrls_originalUrl')}
								placeholder=${this.localize.term('redirectUrls_originalUrl')}
								@keypress=${this.#onKeypress}></uui-input>
							<uui-button
								look="outline"
								label=${this.localize.term('general_search')}
								@click=${this.#onSearch}
								.state=${this._buttonState}></uui-button>
						</div>
						<uui-button
							look="outline"
							label=${this.localize.term('redirectUrls_disableUrlTracker')}
							@click=${this.#onRequestTrackerToggle}></uui-button>
					`,
					() => html`
						<div></div>
						<uui-button
							color="positive"
							look="outline"
							label=${this.localize.term('redirectUrls_enableUrlTracker')}
							@click=${this.#onRequestTrackerToggle}></uui-button>
					`,
				)}
			</div>
			${when(
				this._redirectData?.length,
				() => html`
					<uui-box id="redirect-wrapper">
						${when(!this._trackerEnabled, () => html`<div id="grey-out"></div>`)} ${this.#renderTable()}
					</uui-box>
				`,
				() => (this._filter !== undefined ? this.#renderZeroResults() : this.#renderNoRedirects()),
			)}
			${this.#renderPagination()}
		`;
	}

	#renderZeroResults() {
		return html`
			<uui-box>
				<strong>No redirects matching this search criteria</strong>
				<p>Double check your search for any error or spelling mistakes.</p>
			</uui-box>
		`;
	}

	#renderNoRedirects() {
		return html`
			<uui-box>
				<strong>${this.localize.term('redirectUrls_noRedirects')}</strong>
				<p>${this.localize.term('redirectUrls_noRedirectsDescription')}</p>
			</uui-box>
		`;
	}

	#renderTable() {
		return html`
			<uui-table>
				<uui-table-head>
					<uui-table-head-cell style="width:10%;">${this.localize.term('redirectUrls_culture')}</uui-table-head-cell>
					<uui-table-head-cell>${this.localize.term('redirectUrls_originalUrl')}</uui-table-head-cell>
					<uui-table-head-cell style="width:10%;"></uui-table-head-cell>
					<uui-table-head-cell>${this.localize.term('redirectUrls_redirectedTo')}</uui-table-head-cell>
					<uui-table-head-cell style="width:10%;">${this.localize.term('general_actions')}</uui-table-head-cell>
				</uui-table-head>
				${this.#renderTableData()}
			</uui-table>
		`;
	}

	#renderTableData() {
		if (!this._redirectData?.length) return nothing;
		return html`
			${repeat(
				this._redirectData,
				(data) => data.unique,
				(data) => html`
					<uui-table-row>
						<uui-table-cell>${data.culture || '*'}</uui-table-cell>
						<uui-table-cell>
							<a href=${data.originalUrl || '#'} target="_blank">
								<span>${data.originalUrl}</span>
							</a>
						</uui-table-cell>
						<uui-table-cell>
							<uui-icon name="icon-arrow-right"></uui-icon>
						</uui-table-cell>
						<uui-table-cell>
							<a href=${data.destinationUrl || '#'} target="_blank">
								<span>${data.destinationUrl}</span>
							</a>
						</uui-table-cell>
						<uui-table-cell>
							<uui-action-bar>
								<uui-button
									label=${this.localize.term('general_delete')}
									look="secondary"
									.disabled=${!this._trackerEnabled}
									@click=${() => this.#onRequestDelete(data)}>
									<uui-icon name="delete"></uui-icon>
								</uui-button>
							</uui-action-bar>
						</uui-table-cell>
					</uui-table-row>
				`,
			)}
		`;
	}

	#renderPagination() {
		if (!this._total) return nothing;

		const totalPages = Math.ceil(this._total / this.itemsPerPage);
		if (totalPages <= 1) return nothing;

		return html`
			<div class="pagination">
				<uui-pagination
					.total=${totalPages}
					firstlabel=${this.localize.term('general_first')}
					previouslabel=${this.localize.term('general_previous')}
					nextlabel=${this.localize.term('general_next')}
					lastlabel=${this.localize.term('general_last')}
					@change=${this.#onPageChange}></uui-pagination>
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-4);
				padding: var(--uui-size-layout-1);
			}

			#redirect-actions {
				display: flex;
				justify-content: space-between;
			}

			#search-wrapper {
				display: flex;
				gap: var(--uui-size-4);
			}

			#redirect-wrapper {
				--uui-box-default-padding: 0;

				position: relative;
				display: block;

				#grey-out {
					position: absolute;
					inset: 0;
					background-color: var(--uui-color-surface-alt);
					opacity: 0.7;
					z-index: 1;
				}
			}

			uui-pagination {
				display: inline-block;
			}

			.pagination {
				display: flex;
				justify-content: center;
				margin-top: var(--uui-size-space-5);
			}

			uui-table-cell a:has(span, uui-icon) {
				display: inline-flex;
				align-items: center;
				gap: var(--uui-size-2);
			}
		`,
	];
}

export default UmbDashboardRedirectManagementElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-redirect-management': UmbDashboardRedirectManagementElement;
	}
}
