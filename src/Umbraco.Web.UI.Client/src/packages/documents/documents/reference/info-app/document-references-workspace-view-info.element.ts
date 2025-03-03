import { UmbDocumentReferenceRepository } from '../repository/index.js';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../../constants.js';
import { css, customElement, html, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbReferenceItemModel } from '@umbraco-cms/backoffice/relations';
import type { UUIPaginationEvent } from '@umbraco-cms/backoffice/external/uui';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';

@customElement('umb-document-references-workspace-info-app')
export class UmbDocumentReferencesWorkspaceInfoAppElement extends UmbLitElement {
	@state()
	private _currentPage = 1;

	@state()
	private _total = 0;

	@state()
	private _items?: Array<UmbReferenceItemModel> = [];

	#itemsPerPage = 10;
	#referenceRepository = new UmbDocumentReferenceRepository(this);
	#documentUnique?: UmbEntityUnique;
	#workspaceContext?: typeof UMB_DOCUMENT_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			this.#observeDocumentUnique();
		});
	}

	#observeDocumentUnique() {
		this.observe(
			this.#workspaceContext?.unique,
			(unique) => {
				if (!unique) {
					this.#documentUnique = undefined;
					this._items = [];
					return;
				}

				if (this.#documentUnique === unique) {
					return;
				}

				this.#documentUnique = unique;
				this.#getReferences();
			},
			'umbReferencesDocumentUniqueObserver',
		);
	}

	async #getReferences() {
		if (!this.#documentUnique) {
			throw new Error('Document unique is required');
		}

		const { data } = await this.#referenceRepository.requestReferencedBy(
			this.#documentUnique,
			(this._currentPage - 1) * this.#itemsPerPage,
			this.#itemsPerPage,
		);
		if (!data) return;

		this._total = data.total;
		this._items = data.items;
	}

	#onPageChange(event: UUIPaginationEvent) {
		if (this._currentPage === event.target.current) return;
		this._currentPage = event.target.current;

		this.#getReferences();
	}

	override render() {
		if (!this._items?.length) return nothing;
		return html`
			<umb-workspace-info-app-layout headline="#references_labelUsedByItems">
				${this.#renderItems()} ${this.#renderReferencePagination()}
			</umb-workspace-info-app-layout>
		`;
	}

	#renderItems() {
		if (!this._items) return;
		return html`
			<uui-ref-list>
				${repeat(
					this._items,
					(item) => item.unique,
					(item) => html`<umb-entity-item-ref .item=${item}></umb-entity-item-ref>`,
				)}
			</uui-ref-list>
		`;
	}

	#renderReferencePagination() {
		if (!this._total) return nothing;

		const totalPages = Math.ceil(this._total / this.#itemsPerPage);
		if (totalPages <= 1) return nothing;

		return html`
			<div class="pagination">
				<uui-pagination .total=${totalPages} @change="${this.#onPageChange}"></uui-pagination>
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: contents;
			}

			uui-table-cell:not(.link-cell) {
				color: var(--uui-color-text-alt);
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

export default UmbDocumentReferencesWorkspaceInfoAppElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-references-workspace-info-app': UmbDocumentReferencesWorkspaceInfoAppElement;
	}
}
