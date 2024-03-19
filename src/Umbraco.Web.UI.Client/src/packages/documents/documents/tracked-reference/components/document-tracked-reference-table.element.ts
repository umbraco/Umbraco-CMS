import { UmbDocumentTrackedReferenceRepository } from '../repository/index.js';
import type { RelationItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { css, customElement, html, nothing, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-document-tracked-reference-table')
export class UmbDocumentTrackedReferenceTableElement extends UmbLitElement {
	#documentTrackedReferenceRepository = new UmbDocumentTrackedReferenceRepository(this);
	#pageSize = 10;

	@property()
	unique = '';

	@state()
	_items: Array<RelationItemResponseModel> = [];

	/**
	 * Indicates if there are more references to load, i.e. if the server has more references to return.
	 * This is used to determine if the "...and X more references" text should be displayed.
	 */
	@state()
	_hasMoreReferences = 0;

	@state()
	_errorMessage = '';

	firstUpdated() {
		this.#getTrackedReferences();
	}

	async #getTrackedReferences() {
		// Get the first 10 tracked references for the document:
		const { data, error } = await this.#documentTrackedReferenceRepository.requestTrackedReference(
			this.unique,
			0,
			this.#pageSize,
		);

		if (error) {
			this._errorMessage = error.message;
			return;
		}

		if (!data) return;

		this._items = data.items;
		this._hasMoreReferences = data.total > this.#pageSize ? data.total - this.#pageSize : 0;
	}

	render() {
		return html` ${this.#renderErrorMessage()} ${this.#renderTable()} `;
	}

	#renderTable() {
		if (this._items?.length === 0) return nothing;
		return html`
			<uui-box headline=${this.localize.term('references_labelDependsOnThis')} style="--uui-box-default-padding:0">
				<uui-table>
					<uui-table-head>
						<uui-table-head-cell></uui-table-head-cell>
						<uui-table-head-cell><umb-localize key="general_name">Name</umb-localize></uui-table-head-cell>
						<uui-table-head-cell><umb-localize key="general_typeName">Type Name</umb-localize></uui-table-head-cell>
					</uui-table-head>

					${repeat(
						this._items,
						(item) => item.nodeId,
						(item) =>
							html`<uui-table-row>
								<uui-table-cell style="text-align:center; vertical-align:revert;">
									<umb-icon name=${item.contentTypeIcon ?? 'icon-document'}></umb-icon>
								</uui-table-cell>
								<uui-table-cell class="link-cell"> ${item.nodeName} </uui-table-cell>
								<uui-table-cell>${item.contentTypeName}</uui-table-cell>
							</uui-table-row>`,
					)}
					${this._hasMoreReferences
						? html`<uui-table-row>
								<uui-table-cell></uui-table-cell>
								<uui-table-cell>
									<umb-localize key="references_labelMoreReferences" .args="${[this._hasMoreReferences]}">
										...and ${this._hasMoreReferences} more items
									</umb-localize>
								</uui-table-cell>
								<uui-table-cell></uui-table-cell>
							</uui-table-row>`
						: nothing}
				</uui-table>
			</uui-box>
		`;
	}

	#renderErrorMessage() {
		if (this._errorMessage) {
			return html`<div id="error"><strong>${this._errorMessage}</strong></div>`;
		}
		return nothing;
	}

	static styles = [
		UmbTextStyles,
		css`
			#error {
				color: var(--uui-color-negative);
				margin-bottom: 1rem;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-tracked-reference-table': UmbDocumentTrackedReferenceTableElement;
	}
}
