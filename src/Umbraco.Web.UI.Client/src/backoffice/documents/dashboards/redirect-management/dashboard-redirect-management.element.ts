import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, nothing } from 'lit';
import { customElement, state, query, property } from 'lit/decorators.js';
import { UUIButtonState, UUIPaginationElement, UUIPaginationEvent } from '@umbraco-ui/uui';
import { UMB_CONFIRM_MODAL_TOKEN } from '../../../shared/modals/confirm';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/modal';
import { UmbLitElement } from '@umbraco-cms/element';
import { RedirectManagementResource, RedirectStatusModel, RedirectUrlModel } from '@umbraco-cms/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';

@customElement('umb-dashboard-redirect-management')
export class UmbDashboardRedirectManagementElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			.actions {
				display: flex;
				gap: var(--uui-size-space-1);
				justify-content: space-between;
				margin-bottom: var(--uui-size-space-4);
			}

			.actions uui-icon {
				transform: translateX(50%);
			}

			uui-table {
				table-layout: fixed;
			}

			uui-table-head-cell:nth-child(2*n) {
				width: 10%;
			}

			uui-table-head-cell:last-child,
			uui-table-cell:last-child {
				text-align: right;
			}

			uui-table uui-icon {
				vertical-align: sub;
			}
			uui-pagination {
				display: inline-block;
			}
			.pagination {
				display: flex;
				justify-content: center;
				margin-top: var(--uui-size-space-5);
			}

			.trackerDisabled {
				position: relative;
				-webkit-user-select: none;
				-ms-user-select: none;
				user-select: none;
			}
			.trackerDisabled::after {
				content: '';
				background-color: var(--uui-color-disabled);
				position: absolute;
				border-radius: 2px;
				left: 0;
				right: 0;
				top: 0;
				bottom: 0;
				-webkit-user-select: none;
				-ms-user-select: none;
				user-select: none;
			}

			a {
				color: var(--uui-color-interactive);
			}
			a:hover,
			a:focus {
				color: var(--uui-color-interactive-emphasis);
			}
		`,
	];

	@property({ type: Number, attribute: 'items-per-page' })
	itemsPerPage = 20;

	@state()
	private _redirectData?: RedirectUrlModel[];

	@state()
	private _trackerStatus = true;

	@state()
	private _currentPage = 1;

	@state()
	private _total?: number;

	@state()
	private _buttonState: UUIButtonState;

	@state()
	private _filter?: string;

	@query('#search-input')
	private _searchField!: HTMLInputElement;

	@query('uui-pagination')
	private _pagination?: UUIPaginationElement;

	private _modalContext?: UmbModalContext;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (_instance) => {
			this._modalContext = _instance;
		});
	}

	connectedCallback() {
		super.connectedCallback();
		this._getTrackerStatus();
		this._getRedirectData();
	}

	private async _getTrackerStatus() {
		const { data } = await tryExecuteAndNotify(this, RedirectManagementResource.getRedirectManagementStatus());
		if (data && data.status) this._trackerStatus = data.status === RedirectStatusModel.ENABLED ? true : false;
	}

	private _removeRedirectHandler(data: RedirectUrlModel) {
		const modalHandler = this._modalContext?.open(UMB_CONFIRM_MODAL_TOKEN, {
			headline: 'Delete',
			content: html`
				<div style="width:300px">
					<p>This will remove the redirect</p>
					Original URL: <strong>${data.originalUrl}</strong><br />
					Redirected To: <strong>${data.destinationUrl}</strong>
					<p>Are you sure you want to delete?</p>
				</div>
			`,
			color: 'danger',
			confirmLabel: 'Delete',
		});
		modalHandler?.onSubmit().then(() => {
			this._removeRedirect(data);
		});
	}

	private async _removeRedirect(r: RedirectUrlModel) {
		if (!r.key) return;
		const res = await tryExecuteAndNotify(
			this,
			RedirectManagementResource.deleteRedirectManagementByKey({ key: r.key })
		);
		if (!res.error) {
			// or just run a this._getRedirectData() again?
			this.shadowRoot?.getElementById(`redirect-key-${r.key}`)?.remove();
		}
	}

	private _disableRedirectHandler() {
		const modalHandler = this._modalContext?.open(UMB_CONFIRM_MODAL_TOKEN, {
			headline: 'Disable URL tracker',
			content: html`Are you sure you want to disable the URL tracker?`,
			color: 'danger',
			confirmLabel: 'Disable',
		});
		modalHandler?.onSubmit().then(() => {
			this._toggleRedirect();
		});
	}

	private async _toggleRedirect() {
		const { error } = await tryExecuteAndNotify(
			this,
			RedirectManagementResource.postRedirectManagementStatus({ status: RedirectStatusModel.ENABLED })
		);

		if (!error) {
			this._trackerStatus = !this._trackerStatus;
		}
	}

	private _inputHandler(pressed: KeyboardEvent) {
		if (pressed.key === 'Enter') this._searchHandler();
	}

	private async _searchHandler() {
		this._filter = this._searchField.value;
		if (this._pagination) this._pagination.current = 1;
		this._currentPage = 1;
		if (this._filter.length) {
			this._buttonState = 'waiting';
		}
		this._getRedirectData();
	}

	private _onPageChange(event: UUIPaginationEvent) {
		if (this._currentPage === event.target.current) return;
		this._currentPage = event.target.current;
		this._getRedirectData();
	}

	private async _getRedirectData() {
		const skip = this._currentPage * this.itemsPerPage - this.itemsPerPage;
		const { data } = await tryExecuteAndNotify(
			this,
			RedirectManagementResource.getRedirectManagement({ filter: this._filter, take: this.itemsPerPage, skip })
		);
		if (data) {
			this._total = data?.total;
			this._redirectData = data?.items;
			if (this._filter?.length) this._buttonState = 'success';
		}
	}

	render() {
		return html`<div class="actions">
				${this._trackerStatus
					? html`<div>
								<uui-input
									id="search-input"
									placeholder="Original URL"
									label="input for search"
									@keypress="${this._inputHandler}">
								</uui-input>
								<uui-button
									id="search-button"
									look="primary"
									color="positive"
									label="search"
									.state="${this._buttonState}"
									@click="${this._searchHandler}">
									Search<uui-icon name="umb:search"></uui-icon>
								</uui-button>
							</div>
							<uui-button
								label="Disable URL tracker"
								look="outline"
								color="danger"
								@click="${this._disableRedirectHandler}">
								Disable URL tracker
							</uui-button> `
					: html`<uui-button
							label="Enable URL tracker"
							look="primary"
							color="positive"
							@click="${this._toggleRedirect}">
							Enable URL tracker
					  </uui-button>`}
			</div>

			${this._total && this._total > 0
				? html`<div class="wrapper ${this._trackerStatus ? 'trackerEnabled' : 'trackerDisabled'}">
						${this.renderTable()}
				  </div>`
				: this._filter?.length
				? this._renderZeroResults()
				: this.renderNoRedirects()} `;
	}

	private _renderZeroResults() {
		return html`<uui-box>
			<strong>No redirects matching this search criteria</strong>
			<p>Double check your search for any error or spelling mistakes.</p>
		</uui-box>`;
	}

	private renderNoRedirects() {
		return html`<uui-box>
			<strong>No redirects have been made</strong>
			<p>When a published page gets renamed or moved, a redirect will automatically be made to the new page.</p>
		</uui-box>`;
	}

	private renderTable() {
		return html`<uui-box style="--uui-box-default-padding: 0;">
				<uui-table>
					<uui-table-head>
						<uui-table-head-cell style="width:10%;">Culture</uui-table-head-cell>
						<uui-table-head-cell>Original URL</uui-table-head-cell>
						<uui-table-head-cell style="width:10%;"></uui-table-head-cell>
						<uui-table-head-cell>Redirected To</uui-table-head-cell>
						<uui-table-head-cell style="width:10%;">Actions</uui-table-head-cell>
					</uui-table-head>
					${this._redirectData?.map((data) => {
						return html` <uui-table-row id="redirect-key-${data.key}">
							<uui-table-cell> ${data.culture || '*'} </uui-table-cell>
							<uui-table-cell>
								<a href="${data.originalUrl || '#'}" target="_blank"> ${data.originalUrl}</a>
								<uui-icon name="umb:out"></uui-icon>
							</uui-table-cell>
							<uui-table-cell>
								<uui-icon name="umb:arrow-right"></uui-icon>
							</uui-table-cell>
							<uui-table-cell>
								<a href="${data.destinationUrl || '#'}" target="_blank"> ${data.destinationUrl}</a>
								<uui-icon name="umb:out"></uui-icon>
							</uui-table-cell>
							<uui-table-cell>
								<uui-action-bar style="justify-self: left;">
									<uui-button
										label="Delete"
										look="secondary"
										.disabled=${!this._trackerStatus}
										@click="${() => this._removeRedirectHandler(data)}">
										<uui-icon name="delete"></uui-icon>
									</uui-button>
								</uui-action-bar>
							</uui-table-cell>
						</uui-table-row>`;
					})}
				</uui-table>
			</uui-box>
			${this._renderPagination()}
			</uui-scroll-container
		>`;
	}

	private _renderPagination() {
		if (!this._total) return nothing;

		const totalPages = Math.ceil(this._total / this.itemsPerPage);

		if (totalPages <= 1) return nothing;

		return html`<div class="pagination">
			<uui-pagination .total=${totalPages} @change="${this._onPageChange}"></uui-pagination>
		</div>`;
	}
}

export default UmbDashboardRedirectManagementElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-redirect-management': UmbDashboardRedirectManagementElement;
	}
}
