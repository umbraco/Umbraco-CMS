import { UmbMemberReferenceRepository } from '../repository/index.js';
import { UMB_MEMBER_WORKSPACE_CONTEXT } from '../../workspace/constants.js';
import { css, customElement, html, nothing, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbReferenceItemModel } from '@umbraco-cms/backoffice/relations';
import type { UUIPaginationEvent } from '@umbraco-cms/backoffice/external/uui';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';

@customElement('umb-member-references-workspace-info-app')
export class UmbMemberReferencesWorkspaceInfoAppElement extends UmbLitElement {
	#itemsPerPage = 10;

	#referenceRepository;

	@state()
	private _currentPage = 1;

	@state()
	private _total = 0;

	@state()
	private _items?: Array<UmbReferenceItemModel> = [];

	@state()
	private _loading = true;

	#workspaceContext?: typeof UMB_MEMBER_WORKSPACE_CONTEXT.TYPE;
	#memberUnique?: UmbEntityUnique;

	constructor() {
		super();
		this.#referenceRepository = new UmbMemberReferenceRepository(this);

		this.consumeContext(UMB_MEMBER_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			this.#observeMemberUnique();
		});
	}

	#observeMemberUnique() {
		this.observe(
			this.#workspaceContext?.unique,
			(unique) => {
				if (!unique) {
					this.#memberUnique = undefined;
					this._items = [];
					return;
				}

				if (this.#memberUnique === unique) {
					return;
				}

				this.#memberUnique = unique;
				this.#getReferences();
			},
			'umbReferencesDocumentUniqueObserver',
		);
	}

	async #getReferences() {
		if (!this.#memberUnique) {
			throw new Error('Member unique is required');
		}

		this._loading = true;

		const { data } = await this.#referenceRepository.requestReferencedBy(
			this.#memberUnique,
			(this._currentPage - 1) * this.#itemsPerPage,
			this.#itemsPerPage,
		);

		if (!data) return;

		this._total = data.total;
		this._items = data.items;

		this._loading = false;
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
				${when(
					this._loading,
					() => html`<uui-loader></uui-loader>`,
					() => html`${this.#renderItems()} ${this.#renderPagination()}`,
				)}
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

	#renderPagination() {
		if (!this._total) return nothing;

		const totalPages = Math.ceil(this._total / this.#itemsPerPage);

		if (totalPages <= 1) return nothing;

		return html`
			<div class="pagination">
				<uui-pagination .total=${totalPages} @change=${this.#onPageChange}></uui-pagination>
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: contents;
			}

			uui-table-cell {
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

export default UmbMemberReferencesWorkspaceInfoAppElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-member-references-workspace-info-app': UmbMemberReferencesWorkspaceInfoAppElement;
	}
}
