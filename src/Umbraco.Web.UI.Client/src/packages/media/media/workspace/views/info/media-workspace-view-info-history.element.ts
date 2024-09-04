import type { UmbMediaAuditLogModel } from '../../../audit-log/types.js';
import { UmbMediaAuditLogRepository } from '../../../audit-log/index.js';
import { UMB_MEDIA_WORKSPACE_CONTEXT } from '../../media-workspace.context-token.js';
import { TimeOptions, getMediaHistoryTagStyleAndText } from './utils.js';
import { css, html, customElement, state, nothing, repeat, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbPaginationManager } from '@umbraco-cms/backoffice/utils';
import type { UUIPaginationEvent } from '@umbraco-cms/backoffice/external/uui';
import type { UmbUserItemModel } from '@umbraco-cms/backoffice/user';
import { UmbUserItemRepository } from '@umbraco-cms/backoffice/user';

@customElement('umb-media-workspace-view-info-history')
export class UmbMediaWorkspaceViewInfoHistoryElement extends UmbLitElement {
	@state()
	_currentPageNumber = 1;

	@state()
	_totalPages = 1;

	@state()
	private _items: Array<UmbMediaAuditLogModel> = [];

	#workspaceContext?: typeof UMB_MEDIA_WORKSPACE_CONTEXT.TYPE;
	#auditLogRepository = new UmbMediaAuditLogRepository(this);
	#pagination = new UmbPaginationManager();
	#userItemRepository = new UmbUserItemRepository(this);

	#userMap = new Map<string, UmbUserItemModel>();

	constructor() {
		super();

		this.#pagination.setPageSize(10);
		this.observe(this.#pagination.currentPage, (number) => (this._currentPageNumber = number));
		this.observe(this.#pagination.totalPages, (number) => (this._totalPages = number));

		this.consumeContext(UMB_MEDIA_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#requestAuditLogs();
		});
	}

	async #requestAuditLogs() {
		const unique = this.#workspaceContext?.getUnique();
		if (!unique) throw new Error('Media unique is required');

		const { data } = await this.#auditLogRepository.requestAuditLog({
			unique,
			skip: this.#pagination.getSkip(),
			take: this.#pagination.getPageSize(),
		});

		if (data) {
			this._items = data.items;
			this.#pagination.setTotalItems(data.total);
			this.#requestAndCacheUserItems();
		}
	}

	#onPageChange(event: UUIPaginationEvent) {
		this.#pagination.setCurrentPageNumber(event.target?.current);
		this.#requestAuditLogs();
	}

	async #requestAndCacheUserItems() {
		const allUsers = this._items?.map((item) => item.user.unique).filter(Boolean) as string[];
		const uniqueUsers = [...new Set(allUsers)];
		const uncachedUsers = uniqueUsers.filter((unique) => !this.#userMap.has(unique));

		// If there are no uncached user items, we don't need to make a request
		if (uncachedUsers.length === 0) return;

		const { data: items } = await this.#userItemRepository.requestItems(uncachedUsers);

		if (items) {
			items.forEach((item) => {
				// cache the user item
				this.#userMap.set(item.unique, item);
				this.requestUpdate('_items');
			});
		}
	}

	override render() {
		return html`<uui-box>
			<div id="rollback" slot="header">
				<h2><umb-localize key="general_history">History</umb-localize></h2>
			</div>
			${this._items ? this.#renderHistory() : html`<uui-loader-circle></uui-loader-circle> `}
			${this.#renderPagination()}
		</uui-box> `;
	}

	#renderHistory() {
		if (this._items && this._items.length) {
			return html`
				<umb-history-list>
					${repeat(
						this._items,
						(item) => item.timestamp,
						(item) => {
							const { text, style } = getMediaHistoryTagStyleAndText(item.logType);
							const user = this.#userMap.get(item.user.unique);

							return html`<umb-history-item
								.name=${user?.name ?? 'Unknown'}
								.detail=${this.localize.date(item.timestamp, TimeOptions)}>
								<umb-user-avatar
									slot="avatar"
									.name=${user?.name}
									.kind=${user?.kind}
									.imgUrls=${user?.avatarUrls ?? []}></umb-user-avatar>

								<span class="log-type">
									<uui-tag look=${style.look} color=${style.color}>
										${this.localize.term(text.label, item.parameters)}
									</uui-tag>
									${this.localize.term(text.desc, item.parameters)}
								</span>
							</umb-history-item>`;
						},
					)}
				</umb-history-list>
			`;
		} else {
			return html`${this.localize.term('content_noItemsToShow')}`;
		}
	}

	#renderPagination() {
		return html`
			${this._totalPages > 1
				? html`
						<uui-pagination
							class="pagination"
							.current=${this._currentPageNumber}
							.total=${this._totalPages}
							@change=${this.#onPageChange}></uui-pagination>
					`
				: nothing}
		`;
	}

	static override styles = [
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
				gap: var(--uui-size-space-2);
			}

			uui-pagination {
				flex: 1;
				display: inline-block;
			}

			.pagination {
				display: flex;
				justify-content: center;
				margin-top: var(--uui-size-layout-1);
			}
		`,
	];
}

export default UmbMediaWorkspaceViewInfoHistoryElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-workspace-view-info-history': UmbMediaWorkspaceViewInfoHistoryElement;
	}
}
