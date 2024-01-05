import { HistoricTagAndDescription } from './history-utils.js';
import { css, html, customElement, state, property, nothing, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UUIPaginationEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbAuditLogRepository } from '@umbraco-cms/backoffice/audit-log';
import { AuditLogBaseModel, DirectionModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-document-info-history-workspace-view')
export class UmbDocumentInfoHistoryWorkspaceViewElement extends UmbLitElement {
	#logRepository: UmbAuditLogRepository;
	#itemsPerPage = 10;

	@property()
	documentUnique = '';

	@state()
	private _total?: number;

	@state()
	private _items?: Array<AuditLogBaseModel>;

	@state()
	private _currentPage = 1;

	constructor() {
		super();
		this.#logRepository = new UmbAuditLogRepository(this);
	}

	protected firstUpdated(): void {
		this.#getLogs();
	}

	async #getLogs() {
		this._items = undefined;
		if (!this.documentUnique) return;

		/*const { data } = await this.#logRepository.getAuditLogByUnique({
			id: this.documentUnique,
			orderDirection: DirectionModel.DESCENDING,
			skip: (this._currentPage - 1) * this.#itemsPerPage,
			take: this.#itemsPerPage,
		});

		if (!data) return;
		this._total = data.total;
		this._items = data.items;
		*/

		//TODO: I think there is an issue with the API (backend). Hacking for now.
		// Uncomment previous code and delete the following when issue fixed.

		const { data } = await this.#logRepository.getAuditLogByUnique({
			id: '',
			orderDirection: DirectionModel.DESCENDING,
			skip: 0,
			take: 99999,
		});

		if (!data) return;

		// Hack list to only get the items for the current document
		const list = data.items.filter((item) => item.entityId === this.documentUnique);
		this._total = list.length;

		// Hack list to only get the items for the current page
		this._items = list.slice(
			(this._currentPage - 1) * this.#itemsPerPage,
			(this._currentPage - 1) * this.#itemsPerPage + this.#itemsPerPage,
		);
	}

	#localizeTimestamp(timestamp: string) {
		//TODO In what format should we show the timestamps.. Server based? Localized? Other?
		//What about user's current culture? (American vs European date format) based on their user setting or windows setting?
		return new Date(timestamp).toLocaleString();
	}

	#onPageChange(event: UUIPaginationEvent) {
		if (this._currentPage === event.target.current) return;
		this._currentPage = event.target.current;

		this.#getLogs();
	}

	render() {
		return html`<uui-box headline=${this.localize.term('general_history')}>
			${this._items ? this.#renderHistory() : html`<uui-loader-circle></uui-loader-circle> `}
			${this.#renderHistoryPagination()}
		</uui-box>`;
	}

	#renderHistory() {
		if (this._items && this._items.length) {
			return html`
				<umb-history-list>
					${repeat(
						this._items,
						(item) => item.timestamp,
						(item) => {
							const { text, style } = HistoricTagAndDescription(item.logType);
							return html`<umb-history-item name="TODO Username" detail=${this.#localizeTimestamp(item.timestamp)}>
								<span class="log-type">
									<uui-tag look=${style.look} color=${style.color}> ${this.localize.term(text.label)} </uui-tag>
									${this.localize.term(text.desc, item.parameters)}
								</span>
								<uui-button label=${this.localize.term('actions_rollback')} look="secondary" slot="actions">
									<uui-icon name="icon-undo"></uui-icon>
									<umb-localize key="actions_rollback"></umb-localize>
								</uui-button>
							</umb-history-item>`;
						},
					)}
				</umb-history-list>
			`;
		} else {
			return html`No items found`;
		}
	}

	#renderHistoryPagination() {
		if (!this._total) return nothing;

		const totalPages = Math.ceil(this._total / this.#itemsPerPage);

		if (totalPages <= 1) return nothing;

		return html`<div class="pagination">
			<uui-pagination .total=${totalPages} @change="${this.#onPageChange}"></uui-pagination>
		</div>`;
	}

	static styles = [
		UmbTextStyles,
		css`
			uui-loader-circle {
				font-size: 2rem;
			}

			uui-tag uui-icon {
				margin-right: var(--uui-size-space-1);
			}

			.log-type {
				flex-grow: 1;
				display: flex;
				gap: var(--uui-size-space-2);
			}

			uui-pagination {
				flex: 1;
				display: inline-block;
			}
			.pagination {
				display: flex;
				justify-content: center;
				margin-top: var(--uui-size-space-4);
			}
		`,
	];
}

export default UmbDocumentInfoHistoryWorkspaceViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-info-history-workspace-view': UmbDocumentInfoHistoryWorkspaceViewElement;
	}
}
