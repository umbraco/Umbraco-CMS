import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state, query } from 'lit/decorators.js';
import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '../../../../core/modal';
import { UmbLitElement } from '@umbraco-cms/element';
import { RedirectManagementResource, RedirectStatus, RedirectUrl, RedirectUrlStatus } from '@umbraco-cms/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';

@customElement('umb-dashboard-redirect-management')
export class UmbDashboardRedirectManagementElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			.actions {
				display: flex;
				gap: 4px;
				justify-content: space-between;
				margin-bottom: 12px;
			}

			uui-input uui-icon {
				padding-top: var(--uui-size-space-2);
				padding-left: var(--uui-size-space-3);
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

			.trackerDisabled {
				position: relative;
				-webkit-user-select: none;
				-ms-user-select: none;
				user-select: none;
			}
			.trackerDisabled::after {
				content: '';
				background-color: rgba(250, 250, 250, 0.7);
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

	@state()
	private _redirectData?: RedirectUrl[];

	@state()
	private _redirectDataFiltered?: RedirectUrl[];

	@state()
	private _trackerStatus = true;

	@query('#search')
	private _searchField!: HTMLInputElement;

	private _modalService?: UmbModalService;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_SERVICE_CONTEXT_TOKEN, (_instance) => {
			this._modalService = _instance;
		});
	}

	connectedCallback() {
		super.connectedCallback();
		this._getTrackerStatus();
		this._setup();
	}

	private async _getTrackerStatus() {
		const { data } = await tryExecuteAndNotify(this, RedirectManagementResource.getRedirectManagementStatus());
		if (data && data.status) data.status === RedirectStatus.ENABLED ? true : false;
	}

	private async _setup() {
		const { data } = await tryExecuteAndNotify(
			this,
			RedirectManagementResource.getRedirectManagement({ take: 9999, skip: 0 })
		);
		this._redirectData = data?.items;
		this._searchHandler();
	}

	private _removeRedirectHandler(data: RedirectUrl) {
		const modalHandler = this._modalService?.confirm({
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
		modalHandler?.onClose().then(({ confirmed }: any) => {
			if (confirmed) this._removeRedirect(data);
		});
	}

	private async _removeRedirect(r: RedirectUrl) {
		if (r.contentKey) {
			const { data } = await tryExecuteAndNotify(
				this,
				RedirectManagementResource.deleteRedirectManagementByKey({ key: r.contentKey })
			);
			this._setup();
		}
	}

	private _disableRedirectHandler() {
		const modalHandler = this._modalService?.confirm({
			headline: 'Disable URL tracker',
			content: html`Are you sure you want to disable the URL tracker?`,
			color: 'danger',
			confirmLabel: 'Disable',
		});
		modalHandler?.onClose().then(({ confirmed }: any) => {
			if (confirmed) this._toggleRedirect();
		});
	}
	private async _toggleRedirect() {
		/*//TODO: postRedirectManagementStatus returns 404 for whatever reason...
		const { data } = await tryExecuteAndNotify(
			this,
			RedirectManagementResource.postRedirectManagementStatus({ status: RedirectStatus.ENABLED })
		);*/

		this._trackerStatus = !this._trackerStatus;
	}

	private _searchHandler() {
		this._redirectDataFiltered = this._redirectData?.filter((data) => {
			return data.originalUrl?.includes(this._searchField.value);
		});
	}

	render() {
		return html`<div class="actions">
				${this._trackerStatus
					? html`
							<uui-input id="search" placeholder="Type to search..." label="Search" @keyup="${this._searchHandler}">
								<uui-icon slot="prepend" name="umb:search"></uui-icon>
							</uui-input>
							<uui-button
								label="Disable URL tracker"
								look="outline"
								color="danger"
								@click="${this._disableRedirectHandler}">
								Disable URL tracker
							</uui-button>
					  `
					: html`<uui-button
							label="Enable URL tracker"
							look="primary"
							color="positive"
							@click="${this._toggleRedirect}">
							Enable URL tracker
					  </uui-button>`}
			</div>

			${this._redirectDataFiltered?.length
				? html`<div class="${this._trackerStatus ? 'trackerEnabled' : 'trackerDisabled'}">${this.renderTable()}</div>`
				: this.renderNoRedirects()} `;
	}

	renderNoRedirects() {
		return html`<uui-box>
			<strong slot="header">No redirects have been made</strong>
			<p>When a published page gets renamed or moved, a redirect will automatically be made to the new page.</p>
		</uui-box>`;
	}

	renderTable() {
		return html`<uui-box style="--uui-box-default-padding: 0;">
				<uui-table>
					<uui-table-head>
						<uui-table-head-cell style="width:10%;">Culture</uui-table-head-cell>
						<uui-table-head-cell>Original URL</uui-table-head-cell>
						<uui-table-head-cell style="width:10%;"></uui-table-head-cell>
						<uui-table-head-cell>Redirected To</uui-table-head-cell>
						<uui-table-head-cell style="width:10%;">Actions</uui-table-head-cell>
					</uui-table-head>
					${this._redirectDataFiltered?.map((data) => {
						return html` <uui-table-row>
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
			</uui-box></uui-scroll-container
		>`;
	}
}

export default UmbDashboardRedirectManagementElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-redirect-management': UmbDashboardRedirectManagementElement;
	}
}
