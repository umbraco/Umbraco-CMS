import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { repeat } from 'lit/directives/repeat.js';
import { customElement, state } from 'lit/decorators.js';
import { UUIPaginationEvent } from '@umbraco-ui/uui';
import { UmbWorkspaceEntityContextInterface } from '../../../workspace-context/workspace-entity-context.interface';
import type { DocumentModel } from '@umbraco-cms/backend-api';
import { UmbLitElement } from '@umbraco-cms/element';

interface HistoryNode {
	userId?: number;
	userAvatars?: [];
	userName?: string;
	timestamp?: string;
	comment?: string;
	entityType?: string;
	logType?: HistoryLogType;
	nodeId?: string;
	parameters?: string;
}

type HistoryLogType = 'Publish' | 'Save' | 'Unpublish' | 'ContentVersionEnableCleanup' | 'ContentVersionPreventCleanup';

@customElement('umb-workspace-view-content-info')
export class UmbWorkspaceViewContentInfoElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: grid;
				gap: var(--uui-size-layout-1);
				margin: var(--uui-size-layout-1);
				grid-template-columns: 1fr 350px;
			}

			div.container {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-layout-1);
			}

			//General section

			#general-section {
				display: flex;
				flex-direction: column;
			}

			.general-item {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-1);
			}

			.general-item:not(:last-child) {
				margin-bottom: var(--uui-size-space-6);
			}

			// Link section

			#link-section {
				display: flex;
				flex-direction: column;
				text-align: left;
			}

			.link-item {
				padding: var(--uui-size-space-4) var(--uui-size-space-6);
				display: grid;
				grid-template-columns: 75px 1fr;
				color: inherit;
				text-decoration: none;
			}

			.link-language {
				color: var(--uui-color-divider-emphasis);
			}

			.link-content.italic {
				font-style: italic;
			}

			.link-item uui-icon {
				margin-right: var(--uui-size-space-2);
				vertical-align: middle;
			}

			.link-item.with-href {
				cursor: pointer;
			}

			.link-item.with-href:hover {
				background: var(--uui-color-divider);
			}

			//History section

			uui-tag uui-icon {
				margin-right: var(--uui-size-space-1);
			}

			.log-type {
				display: flex;
				gap: var(--uui-size-space-2);
			}
			uui-pagination {
				display: inline-block;
			}
			.pagination {
				display: flex;
				justify-content: center;
				margin-top: var(--uui-size-space-4);
			}
		`,
	];

	@state()
	private _historyList: HistoryNode[] = [
		{
			userId: -1,
			userAvatars: [],
			userName: 'Lone Iversen',
			timestamp: 'December 5, 2022 2:59 PM',
			comment: undefined,
			entityType: 'Document',
			logType: 'Save',
			nodeId: '1058',
			parameters: undefined,
		},
		{
			userId: -1,
			userAvatars: [],
			userName: 'Lone Iversen',
			timestamp: 'December 5, 2022 2:59 PM',
			comment: undefined,
			entityType: 'Document',
			logType: 'Unpublish',
			nodeId: '1058',
			parameters: undefined,
		},
		{
			userId: -1,
			userAvatars: [],
			userName: 'Lone Iversen',
			timestamp: 'December 5, 2022 2:59 PM',
			comment: undefined,
			entityType: 'Document',
			logType: 'Publish',
			nodeId: '1058',
			parameters: undefined,
		},

		{
			userId: -1,
			userAvatars: [],
			userName: 'Lone Iversen',
			timestamp: 'December 5, 2022 2:59 PM',
			comment: undefined,
			entityType: 'Document',
			logType: 'Save',
			nodeId: '1058',
			parameters: undefined,
		},

		{
			userId: -1,
			userAvatars: [],
			userName: 'Lone Iversen',
			timestamp: 'December 5, 2022 2:59 PM',
			comment: undefined,
			entityType: 'Document',
			logType: 'Save',
			nodeId: '1058',
			parameters: undefined,
		},
	];

	@state()
	private _total?: number;

	@state()
	private _currentPage = 1;

	@state()
	private _nodeName = '';

	private _workspaceContext?: UmbWorkspaceEntityContextInterface<DocumentModel>;
	private itemsPerPage = 10;

	constructor() {
		super();

		// TODO: Figure out how to get the magic string for the workspace context.
		this.consumeContext<UmbWorkspaceEntityContextInterface<DocumentModel>>('umbWorkspaceContext', (nodeContext) => {
			this._workspaceContext = nodeContext;
			this._observeContent();
		});
	}

	private _observeContent() {
		if (!this._workspaceContext) return;

		this._nodeName = 'TBD, with variants this is not as simple.';

		/*
		this.observe(this._workspaceContext.name, (name) => {
			this._nodeName = name || '';
		});
		*/
	}

	#onPageChange(event: UUIPaginationEvent) {
		if (this._currentPage === event.target.current) return;
		this._currentPage = event.target.current;
		//TODO: Run endpoint to get next history parts
	}

	render() {
		return html`<div class="container">
				<uui-box headline="Links" style="--uui-box-default-padding: 0;"> ${this.#renderLinksSection()} </uui-box>
				<uui-box headline="History">
					<umb-history-list>
						${repeat(
							this._historyList,
							(item) => item.timestamp,
							(item) => this.#renderHistory(item)
						)}
					</umb-history-list>
					${this.#renderHistoryPagination()}
				</uui-box>
			</div>
			<div class="container">
				<uui-box headline="General" id="general-section">${this.#renderGeneralSection()}</uui-box>
			</div>`;
	}

	#renderLinksSection() {
		//repeat
		return html`<div id="link-section">
			<a href="http://google.com" target="_blank" class="link-item with-href">
				<span class="link-language">da-DK</span>
				<span class="link-content"> <uui-icon name="umb:out"></uui-icon>google.com </span>
			</a>
			<div class="link-item">
				<span class="link-language">en-EN</span>
				<span class="link-content italic"> This document is published but is not in the cache</span>
			</div>
		</div>`;
	}

	#renderGeneralSection() {
		return html`
			<div class="general-item">
				<strong>Status</strong>
				<span><uui-tag color="positive" look="primary" label="Published">Published</uui-tag></span>
			</div>
			<div class="general-item">
				<strong>Created Date</strong>
				<span>...</span>
			</div>
			<div class="general-item">
				<strong>Document Type</strong>
				<span>document type picker?</span>
			</div>
			<div class="general-item">
				<strong>Template</strong>
				<span>template picker?</span>
			</div>
			<div class="general-item">
				<strong>Id</strong>
				<span>...</span>
			</div>
		`;
	}

	#renderHistory(history: HistoryNode) {
		return html` <umb-history-item .name="${history.userName}" .detail="${history.timestamp}">
			<span class="log-type">${this.#renderTag(history.logType)} ${this.#renderTagDescription(history.logType)}</span>
			<uui-button label="Rollback" look="secondary" slot="actions">
				<uui-icon name="umb:undo"></uui-icon> Rollback
			</uui-button>
		</umb-history-item>`;
	}

	#renderHistoryPagination() {
		if (!this._total) return nothing;

		const totalPages = Math.ceil(this._total / this.itemsPerPage);

		if (totalPages <= 1) return nothing;

		return html`<div class="pagination">
			<uui-pagination .total=${totalPages} @change="${this.#onPageChange}"></uui-pagination>
		</div>`;
	}

	#renderTag(type?: HistoryLogType) {
		switch (type) {
			case 'Publish':
				return html`<uui-tag look="primary" color="positive" label="Publish">Publish</uui-tag>`;
			case 'Unpublish':
				return html`<uui-tag look="primary" color="warning" label="Unpublish">Unpublish</uui-tag>`;
			case 'Save':
				return html`<uui-tag look="primary" label="Save">Save</uui-tag>`;
			case 'ContentVersionEnableCleanup':
				return html`<uui-tag look="secondary" label="Content Version Enable Cleanup">Save</uui-tag>`;
			case 'ContentVersionPreventCleanup':
				return html`<uui-tag look="secondary" label="Content Version Prevent Cleanup">Save</uui-tag>`;
			default:
				return 'Could not detech log type';
		}
	}

	#renderTagDescription(type?: HistoryLogType, params?: string) {
		switch (type) {
			case 'Publish':
				return html`Content published`;
			case 'Unpublish':
				return html`Content unpublished`;
			case 'Save':
				return html`Content saved`;
			case 'ContentVersionEnableCleanup':
				return html`Cleanup enabled for version: ${params}`;
			case 'ContentVersionPreventCleanup':
				return html`Cleanup disabled for version: ${params}`;
			default:
				return 'Could not detech log type';
		}
	}
}

export default UmbWorkspaceViewContentInfoElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-view-content-info': UmbWorkspaceViewContentInfoElement;
	}
}
