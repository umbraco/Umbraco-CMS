import type { ManifestAuditLogAction } from '../audit-log-action/audit-log-action.extension.js';
import type { ManifestWorkspaceInfoAppAuditLogKind } from './types.js';
import { css, customElement, html, nothing, property, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { createExtensionApiByAlias, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { fromPascalCase, UmbPaginationManager, UMB_DATE_TIME_FORMAT_OPTIONS } from '@umbraco-cms/backoffice/utils';
import { UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbUserItemRepository } from '@umbraco-cms/backoffice/user';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import type { UmbAuditLogModel, UmbAuditLogRepository, ManifestAuditLogTypeStyle, MetaAuditLogTypeStyle, ManifestAuditLogTriggerStyle } from '@umbraco-cms/backoffice/audit-log';
import type { UmbUserItemModel } from '@umbraco-cms/backoffice/user';
import type { UUIPaginationEvent } from '@umbraco-cms/backoffice/external/uui';

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

	#typeStyleMap = new Map<string, MetaAuditLogTypeStyle>();

	// Keyed by "source:operation" for exact matches, or "source" for fallback
	#triggerLabelMap = new Map<string, string>();

	#workspaceContext?: typeof UMB_ENTITY_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.#pagination.setPageSize(10);
		this.observe(this.#pagination.currentPage, (number) => (this._currentPageNumber = number));
		this.observe(this.#pagination.totalPages, (number) => (this._totalPages = number));

		this.observe(
			umbExtensionsRegistry.byType('auditLogTypeStyle'),
			(manifests) => {
				this.#typeStyleMap.clear();
				for (const manifest of manifests as ManifestAuditLogTypeStyle[]) {
					for (const alias of manifest.forTypeAliases) {
						this.#typeStyleMap.set(alias, manifest.meta);
					}
				}
				this.requestUpdate('_items');
			},
		);

		this.observe(
			umbExtensionsRegistry.byType('auditLogTriggerStyle'),
			(manifests) => {
				this.#triggerLabelMap.clear();
				for (const manifest of manifests as ManifestAuditLogTriggerStyle[]) {
					const source = manifest.forTriggerSource;
					for (const mapping of manifest.meta.mappings) {
						this.#triggerLabelMap.set(`${source}:${mapping.operation}`, mapping.label);
					}
					if (manifest.meta.fallbackLabel) {
						this.#triggerLabelMap.set(source, manifest.meta.fallbackLabel);
					}
				}
				this.requestUpdate('_items');
			},
		);

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
			<table class="log-table">
				<thead>
					<tr>
						<th colspan="2">${this.localize.term('general_user')}</th>
						<th>${this.localize.term('general_action')}</th>
						<th>${this.localize.term('auditTrails_trigger')}</th>
						<th>${this.localize.term('general_comment')}</th>
					</tr>
				</thead>
				<tbody>
					${repeat(
						this._items,
						(item) => item.timestamp,
						(item) => this.#renderHistoryItem(item),
					)}
				</tbody>
			</table>
		`;
	}

	#renderHistoryItem(item: UmbAuditLogModel) {
		const tagData = this.#auditLogRepository?.getTagStyleAndText?.(item.logType);
		const typeStyle = item.typeAlias ? this.#typeStyleMap.get(item.typeAlias) : undefined;
		const user = this.#userMap.get(item.user.unique);

		const badgeLook = typeStyle?.look ?? tagData?.style.look ?? 'placeholder';
		const badgeColor = typeStyle?.color ?? tagData?.style.color ?? 'default';
		const badgeLabel = typeStyle?.label ?? this.localize.term(tagData?.text.label ?? item.logType, item.parameters);

		return html`
			<tr>
				<td class="cell-avatar">
					<umb-user-avatar .name=${user?.name} .kind=${user?.kind} .imgUrls=${user?.avatarUrls ?? []}>
					</umb-user-avatar>
				</td>
				<td class="cell-user">
					<span class="user-name">${user?.name ?? 'Unknown'}</span>
					<span class="user-detail">${this.localize.date(item.timestamp, UMB_DATE_TIME_FORMAT_OPTIONS)}</span>
				</td>
				<td class="cell-action">
					<uui-tag look=${badgeLook} color=${badgeColor}>${badgeLabel}</uui-tag>
				</td>
				<td class="cell-trigger">${this.#renderTrigger(item)}</td>
				<td class="cell-comment">${this.#renderComment(item, tagData?.text.desc)}</td>
			</tr>
		`;
	}

	#renderTrigger(item: UmbAuditLogModel) {
		if (!item.triggerSource || !item.triggerOperation) return nothing;
		// Try exact match (source:operation), then source fallback, then raw display
		const label =
			this.#triggerLabelMap.get(`${item.triggerSource}:${item.triggerOperation}`)
			?? this.#triggerLabelMap.get(item.triggerSource)
			?? `${item.triggerSource}: ${fromPascalCase(item.triggerOperation)}`;
		return label;
	}

	#renderComment(item: UmbAuditLogModel, descKey?: string) {
		// For non-custom types without parameters, the comment is redundant
		// (it just repeats the action badge label, e.g. "Content saved").
		// Only show comments for custom types or when parameters add detail
		// (e.g. "Content saved for languages: en-US, da-DK").
		const isCustom = item.logType === 'Custom';
		const hasParameters = !!item.parameters;

		if (!isCustom && !hasParameters) {
			return nothing;
		}

		const localized = descKey ? this.localize.term(descKey, item.parameters ?? '') : '';
		return localized || item.comment || '';
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

			.log-table {
				width: 100%;
				border-collapse: collapse;
				table-layout: auto;
			}

			.log-table thead th {
				text-align: left;
				font-size: var(--uui-type-small-size);
				font-weight: 600;
				color: var(--uui-color-text-alt);
				padding: 0 var(--uui-size-space-3) var(--uui-size-space-3);
				border-bottom: 1px solid var(--uui-color-border);
			}

			.log-table tbody td {
				padding: var(--uui-size-space-3);
				vertical-align: middle;
			}

			.cell-avatar {
				width: 2.5em;
			}

			.cell-user {
				white-space: nowrap;
			}

			.user-name {
				display: block;
				font-weight: 600;
			}

			.user-detail {
				display: block;
				font-size: var(--uui-size-4);
				color: var(--uui-color-text-alt);
				line-height: 1;
			}

			.cell-action uui-tag {
				white-space: nowrap;
			}

			.cell-trigger {
				font-size: var(--uui-type-small-size);
				color: var(--uui-color-text-alt);
				white-space: nowrap;
			}

			.cell-comment {
				font-size: var(--uui-type-small-size);
				color: var(--uui-color-text-alt);
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
