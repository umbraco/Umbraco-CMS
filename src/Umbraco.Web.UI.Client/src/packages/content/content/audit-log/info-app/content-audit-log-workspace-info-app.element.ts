import type { ManifestAuditLogAction } from '../audit-log-action/audit-log-action.extension.js';
import type { ManifestWorkspaceInfoAppAuditLogKind } from './types.js';
import { css, customElement, html, nothing, property, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPaginationManager } from '@umbraco-cms/backoffice/utils';
import { UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbUserItemRepository } from '@umbraco-cms/backoffice/user';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import type { UmbAuditLogModel, UmbAuditLogRepository } from '@umbraco-cms/backoffice/audit-log';
import type { UmbUserItemModel } from '@umbraco-cms/backoffice/user';
import type { UUIPaginationEvent } from '@umbraco-cms/backoffice/external/uui';

const TimeOptions: Intl.DateTimeFormatOptions = {
	year: 'numeric',
	month: 'long',
	day: 'numeric',
	hour: 'numeric',
	minute: 'numeric',
	second: 'numeric',
};

@customElement('umb-content-audit-log-workspace-info-app')
export class UmbContentAuditLogWorkspaceInfoAppElement extends UmbLitElement {
	@property({ type: Object })
	private _manifest?: ManifestWorkspaceInfoAppAuditLogKind | undefined;
	public get manifest(): ManifestWorkspaceInfoAppAuditLogKind | undefined {
		return this._manifest;
	}
	public set manifest(value: ManifestWorkspaceInfoAppAuditLogKind | undefined) {
		this._manifest = value;
		this.#init();
	}

	@state()
	private _currentPageNumber = 1;

	@state()
	private _entityType?: string;

	@state()
	private _items: Array<UmbAuditLogModel> = [];

	@state()
	private _totalPages = 1;

	#auditLogRepository?: UmbAuditLogRepository;

	#pagination = new UmbPaginationManager();

	#userItemRepository = new UmbUserItemRepository(this);

	#userMap = new Map<string, UmbUserItemModel>();

	#workspaceContext?: typeof UMB_ENTITY_WORKSPACE_CONTEXT.TYPE;

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

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#requestAuditLogs();
		});
	}

	async #init() {
		if (!this._manifest) return;
		const auditLogRepositoryAlias = this._manifest.meta.auditLogRepositoryAlias;

		if (!auditLogRepositoryAlias) {
			throw new Error('Audit log repository alias is required');
		}

		this.#auditLogRepository = await createExtensionApiByAlias<UmbAuditLogRepository>(this, auditLogRepositoryAlias);

		this.#requestAuditLogs();
	}

	#onPageChange(event: UUIPaginationEvent) {
		this.#pagination.setCurrentPageNumber(event.target?.current);
		this.#requestAuditLogs();
	}

	async #requestAuditLogs() {
		if (!this.#workspaceContext) return;
		if (!this.#auditLogRepository) return;

		const unique = this.#workspaceContext.getUnique();
		if (!unique) {
			throw new Error('Workspace entity unique is required');
		}

		this._entityType = this.#workspaceContext.getEntityType();

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
				${when(
					this._entityType,
					(entityType) => html`
						<umb-extension-with-api-slot
							slot="header-actions"
							type="auditLogAction"
							.apiArgs=${(manifest: ManifestAuditLogAction) => [manifest]}
							.filter=${(manifest: ManifestAuditLogAction) => manifest.forEntityTypes.includes(entityType)}>
						</umb-extension-with-api-slot>
					`,
				)}
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
					(item) => this.#renderHistoryItem(item),
				)}
			</umb-history-list>
		`;
	}

	#renderHistoryItem(item: UmbAuditLogModel) {
		const tagData = this.#auditLogRepository?.getTagStyleAndText?.(item.logType);
		const user = this.#userMap.get(item.user.unique);

		return html`
			<umb-history-item .name=${user?.name ?? 'Unknown'} .detail=${this.localize.date(item.timestamp, TimeOptions)}>
				<umb-user-avatar slot="avatar" .name=${user?.name} .kind=${user?.kind} .imgUrls=${user?.avatarUrls ?? []}>
				</umb-user-avatar>
				<div class="log-type">
					<uui-tag look=${tagData?.style.look ?? 'placeholder'} color=${tagData?.style.color ?? 'default'}>
						${this.localize.term(tagData?.text.label ?? item.logType, item.parameters)}
					</uui-tag>
					<span>${this.localize.term(tagData?.text.desc ?? '', item.parameters)}</span>
				</div>
			</umb-history-item>
		`;
	}

	#renderPagination() {
		if (this._totalPages <= 1) return nothing;
		return html`
			<uui-pagination
				.current=${this._currentPageNumber}
				.total=${this._totalPages}
				firstlabel=${this.localize.term('general_first')}
				previouslabel=${this.localize.term('general_previous')}
				nextlabel=${this.localize.term('general_next')}
				lastlabel=${this.localize.term('general_last')}
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

export { UmbContentAuditLogWorkspaceInfoAppElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-content-audit-log-workspace-info-app': UmbContentAuditLogWorkspaceInfoAppElement;
	}
}
