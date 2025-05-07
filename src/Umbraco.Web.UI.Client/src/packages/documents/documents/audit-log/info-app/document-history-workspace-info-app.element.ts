import { UmbDocumentAuditLogRepository } from '../repository/index.js';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../../workspace/constants.js';
import type { UmbDocumentAuditLogModel } from '../types.js';
import { TimeOptions } from '../../utils.js';
import { getDocumentHistoryTagStyleAndText } from './utils.js';
import { css, customElement, html, nothing, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPaginationManager } from '@umbraco-cms/backoffice/utils';
import { UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbUserItemRepository } from '@umbraco-cms/backoffice/user';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/entity-action';
import type { UmbUserItemModel } from '@umbraco-cms/backoffice/user';
import type { UUIPaginationEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-document-history-workspace-info-app')
export class UmbDocumentHistoryWorkspaceInfoAppElement extends UmbLitElement {
	#allowedActions = new Set(['Umb.EntityAction.Document.Rollback']);

	#auditLogRepository = new UmbDocumentAuditLogRepository(this);

	#pagination = new UmbPaginationManager();

	#userItemRepository = new UmbUserItemRepository(this);

	#userMap = new Map<string, UmbUserItemModel>();

	#workspaceContext?: typeof UMB_DOCUMENT_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _currentPageNumber = 1;

	@state()
	private _items: Array<UmbDocumentAuditLogModel> = [];

	@state()
	private _totalPages = 1;

	constructor() {
		super();

		this.#pagination.setPageSize(10);
		this.observe(this.#pagination.currentPage, (number) => (this._currentPageNumber = number));
		this.observe(this.#pagination.totalPages, (number) => (this._totalPages = number));

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (context) => {
			context?.addEventListener(UmbRequestReloadStructureForEntityEvent.TYPE, () => {
				this.#requestAuditLogs();
			});
		});

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#requestAuditLogs();
		});
	}

	#onPageChange(event: UUIPaginationEvent) {
		this.#pagination.setCurrentPageNumber(event.target?.current);
		this.#requestAuditLogs();
	}

	async #requestAuditLogs() {
		if (!this.#workspaceContext) return;
		const unique = this.#workspaceContext.getUnique();
		if (!unique) throw new Error('Document unique is required');

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
		return html`
			<umb-workspace-info-app-layout headline="#general_history">
				<umb-extension-with-api-slot
					slot="header-actions"
					type="entityAction"
					.filter=${(manifest: ManifestEntityAction) =>
						this.#allowedActions.has(manifest.alias)}></umb-extension-with-api-slot>

				<div id="content">
					${when(
						this._items,
						() => this.#renderHistory(),
						() => html`<div id="loader"><uui-loader></uui-loader></div>`,
					)}
					${this.#renderPagination()}
				</div>
			</umb-workspace-info-app-layout>
		`;
	}

	#renderHistory() {
		if (!this._items?.length) return html`${this.localize.term('content_noItemsToShow')}`;
		return html`
			<umb-history-list>
				${repeat(
					this._items,
					(item) => item.timestamp,
					(item) => {
						const { text, style } = getDocumentHistoryTagStyleAndText(item.logType);
						const user = this.#userMap.get(item.user.unique);

						return html`
							<umb-history-item
								.name=${user?.name ?? 'Unknown'}
								.detail=${this.localize.date(item.timestamp, TimeOptions)}>
								<umb-user-avatar
									slot="avatar"
									.name=${user?.name}
									.kind=${user?.kind}
									.imgUrls=${user?.avatarUrls ?? []}>
								</umb-user-avatar>
								<div class="log-type">
									<uui-tag look=${style.look} color=${style.color}>
										${this.localize.term(text.label, item.parameters)}
									</uui-tag>
									<span>${this.localize.term(text.desc, item.parameters)}</span>
								</div>
							</umb-history-item>
						`;
					},
				)}
			</umb-history-list>
		`;
	}

	#renderPagination() {
		if (this._totalPages <= 1) return nothing;
		return html`
			<uui-pagination
				.current=${this._currentPageNumber}
				.total=${this._totalPages}
				@change=${this.#onPageChange}></uui-pagination>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#content {
				display: block;
				padding: var(--uui-size-space-4) var(--uui-size-space-5);
			}

			#loader {
				display: flex;
				justify-content: center;
			}

			.log-type {
				display: grid;
				grid-template-columns: var(--uui-size-40) auto;
				gap: var(--uui-size-layout-1);
			}

			.log-type uui-tag {
				justify-self: center;
				height: fit-content;
				margin-top: auto;
				margin-bottom: auto;
			}

			uui-pagination {
				flex: 1;
				display: flex;
				justify-content: center;
				margin-top: var(--uui-size-layout-1);
			}
		`,
	];
}

export default UmbDocumentHistoryWorkspaceInfoAppElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-history-workspace-info-app': UmbDocumentHistoryWorkspaceInfoAppElement;
	}
}
