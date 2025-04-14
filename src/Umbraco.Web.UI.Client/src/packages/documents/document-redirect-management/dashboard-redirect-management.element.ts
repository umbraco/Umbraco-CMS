import type { UUIButtonState, UUIPaginationElement, UUIPaginationEvent } from '@umbraco-cms/backoffice/external/uui';
import { css, html, nothing, customElement, state, query, property } from '@umbraco-cms/backoffice/external/lit';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { RedirectUrlResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { RedirectManagementService, RedirectStatusModel } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

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
	private _redirectData?: RedirectUrlResponseModel[];

	@state()
	private _buttonState: UUIButtonState;

	@state()
	private _filter?: string;

	@query('#search')
	private _search!: HTMLInputElement;

	@query('uui-pagination')
	private _pagination?: UUIPaginationElement;

	override connectedCallback() {
		super.connectedCallback();
		this.#getTrackerStatus();
		this.#getRedirectData();
	}

	async #getTrackerStatus() {
		const { data } = await tryExecute(this, RedirectManagementService.getRedirectManagementStatus());
		if (data && data.status) this._trackerEnabled = data.status === RedirectStatusModel.ENABLED ? true : false;
	}

	// Fetch data
	async #getRedirectData(filter: string | undefined = undefined) {
		const skip = this.page * this.itemsPerPage - this.itemsPerPage;
		const { data } = await tryExecute(
			this,
			RedirectManagementService.getRedirectManagement({ filter, take: this.itemsPerPage, skip }),
		);
		if (!data) return;

		this._total = data?.total;
		this._redirectData = data?.items;

		if (filter !== undefined) this._buttonState = 'success';
	}

	// Pagination
	#onPageChange(event: UUIPaginationEvent) {
		if (this.page === event.target.current) return;
		this.page = event.target.current;
		this.#getRedirectData();
	}

	// Delete Redirect Action
	async #onRequestDelete(data: RedirectUrlResponseModel) {
		if (!data.id) return;

		await umbConfirmModal(this, {
			headline: 'Delete',
			content: html`
				<div style="width:300px">
					<p>${this.localize.term('redirectUrls_redirectRemoveWarning')}</p>
					${this.localize.term('redirectUrls_originalUrl')}: <strong>${data.originalUrl}</strong><br />
					${this.localize.term('redirectUrls_redirectedTo')}: <strong>${data.destinationUrl}</strong>
				</div>
			`,
			color: 'danger',
			confirmLabel: 'Delete',
		});

		this.#redirectDelete(data.id!);
	}
	async #redirectDelete(id: string) {
		const { error } = await tryExecute(this, RedirectManagementService.deleteRedirectManagementById({ id }));
		if (error) return;

		this._redirectData = this._redirectData?.filter((x) => x.id !== id);
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
			headline: `${this.localize.term('redirectUrls_disableUrlTracker')}`,
			content: `${this.localize.term('redirectUrls_confirmDisable')}`,
			color: 'danger',
			confirmLabel: 'Disable',
		});

		this.#trackerToggle();
	}

	async #trackerToggle() {
		const status = this._trackerEnabled ? RedirectStatusModel.DISABLED : RedirectStatusModel.ENABLED;
		const { error } = await tryExecute(this, RedirectManagementService.postRedirectManagementStatus({ status }));
		if (error) return;
		this._trackerEnabled = !this._trackerEnabled;
	}

	// Renders
	override render() {
		return html` <div id="redirect-actions">
				${this._trackerEnabled
					? html`<div id="search-wrapper">
								<uui-input
									id="search"
									placeholder="${this.localize.term('redirectUrls_originalUrl')}"
									label="${this.localize.term('redirectUrls_originalUrl')}"
									@keypress=${this.#onKeypress}></uui-input>
								<uui-button
									look="outline"
									label="${this.localize.term('general_search')}"
									@click=${this.#onSearch}
									.state=${this._buttonState}>
									${this.localize.term('general_search')}
								</uui-button>
							</div>
							<uui-button
								look="outline"
								label="${this.localize.term('redirectUrls_disableUrlTracker')}"
								@click=${this.#onRequestTrackerToggle}>
								${this.localize.term('redirectUrls_disableUrlTracker')}
							</uui-button>`
					: html`<div></div>
							<uui-button
								look="outline"
								color="positive"
								label="${this.localize.term('redirectUrls_enableUrlTracker')}"
								@click=${this.#onRequestTrackerToggle}>
								${this.localize.term('redirectUrls_enableUrlTracker')}
							</uui-button>`}
			</div>
			${this._redirectData?.length
				? html`<uui-box id="redirect-wrapper" style="--uui-box-default-padding:0">
						${this._trackerEnabled ? '' : html`<div id="grey-out"></div>`} ${this.#renderTable()}
					</uui-box>`
				: this._filter !== undefined
					? this.#renderZeroResults()
					: this.#renderNoRedirects()}
			${this.#renderPagination()}`;
	}

	#renderZeroResults() {
		return html`<uui-box>
			<strong>No redirects matching this search criteria</strong>
			<p>Double check your search for any error or spelling mistakes.</p>
		</uui-box>`;
	}

	#renderNoRedirects() {
		return html`<uui-box>
			<strong>${this.localize.term('redirectUrls_noRedirects')}</strong>
			<p>${this.localize.term('redirectUrls_noRedirectsDescription')}</p>
		</uui-box>`;
	}

	#renderTable() {
		return html`<uui-table>
			<uui-table-head>
				<uui-table-head-cell style="width:10%;">${this.localize.term('redirectUrls_culture')}</uui-table-head-cell>
				<uui-table-head-cell>${this.localize.term('redirectUrls_originalUrl')}</uui-table-head-cell>
				<uui-table-head-cell style="width:10%;"></uui-table-head-cell>
				<uui-table-head-cell>${this.localize.term('redirectUrls_redirectedTo')}</uui-table-head-cell>
				<uui-table-head-cell style="width:10%;">${this.localize.term('general_actions')}</uui-table-head-cell>
			</uui-table-head>
			${this.#renderTableData()}
		</uui-table>`;
	}

	#renderTableData() {
		return html`${this._redirectData?.map((data) => {
			return html` <uui-table-row>
				<uui-table-cell> ${data.culture || '*'} </uui-table-cell>
				<uui-table-cell>
					<a href="${data.originalUrl || '#'}" target="_blank">
						<span>${data.originalUrl}</span>
						<uui-icon name="icon-out"></uui-icon>
					</a>
				</uui-table-cell>
				<uui-table-cell>
					<uui-icon name="icon-arrow-right"></uui-icon>
				</uui-table-cell>
				<uui-table-cell>
					<a href="${data.destinationUrl || '#'}" target="_blank">
						<span>${data.destinationUrl}</span>
						<uui-icon name="icon-out"></uui-icon>
					</a>
				</uui-table-cell>
				<uui-table-cell>
					<uui-action-bar style="justify-self: left;">
						<uui-button
							label="Delete"
							look="secondary"
							.disabled=${!this._trackerEnabled}
							@click=${() => this.#onRequestDelete(data)}>
							<uui-icon name="delete"></uui-icon>
						</uui-button>
					</uui-action-bar>
				</uui-table-cell>
			</uui-table-row>`;
		})}`;
	}

	#renderPagination() {
		if (!this._total) return nothing;

		const totalPages = Math.ceil(this._total / this.itemsPerPage);

		if (totalPages <= 1) return nothing;

		return html`<div class="pagination">
			<uui-pagination .total=${totalPages} @change=${this.#onPageChange}></uui-pagination>
		</div>`;
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
				position: relative;
				display: block;
			}
			#redirect-wrapper #grey-out {
				position: absolute;
				inset: 0;
				background-color: var(--uui-color-surface-alt);
				opacity: 0.7;
				z-index: 1;
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
