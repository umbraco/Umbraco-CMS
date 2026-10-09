import { UMB_TREE_CONTEXT } from '../tree.context.token.js';
import type { UUIPaginationEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-tree-pagination')
export class UmbTreePaginationElement extends UmbLitElement {
	@state()
	private _totalPages = 1;

	@state()
	private _currentPage = 1;

	#treeContext?: typeof UMB_TREE_CONTEXT.TYPE;

	constructor() {
		super();
		this.consumeContext(UMB_TREE_CONTEXT, (context) => {
			this.#treeContext = context;
			this.observe(
				context?.pagination.currentPage,
				(currentPage) => (this._currentPage = currentPage ?? 1),
				'_observeCurrentPage',
			);
			this.observe(
				context?.pagination.totalPages,
				(totalPages) => (this._totalPages = totalPages ?? 1),
				'_observeTotalPages',
			);
		});
	}

	#onChange(event: UUIPaginationEvent) {
		this.#treeContext?.loadPage?.(event.target.current);
	}

	override render() {
		if (this._totalPages <= 1) return nothing;

		return html`<uui-pagination
			.current=${this._currentPage}
			.total=${this._totalPages}
			firstlabel=${this.localize.term('general_first')}
			previouslabel=${this.localize.term('general_previous')}
			nextlabel=${this.localize.term('general_next')}
			lastlabel=${this.localize.term('general_last')}
			@change=${this.#onChange}></uui-pagination>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				text-align: center;
			}

			uui-pagination {
				display: block;
				margin-top: var(--uui-size-layout-1);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-pagination': UmbTreePaginationElement;
	}
}
