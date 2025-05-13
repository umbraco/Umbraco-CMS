import { UmbDocumentReferenceRepository } from '../repository/index.js';
import { css, customElement, html, nothing, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import {
	type UmbReferenceModel,
	isDocumentReference,
	isMediaReference,
	isMemberReference,
	isDefaultReference,
} from '@umbraco-cms/backoffice/relations';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

/**
 * @deprecated Deprecated from 15.4. The element will be removed in v17.0.0. For modals use the <umb-confirm-action-modal-entity-references> or <umb-confirm-bulk-action-modal-entity-references> element instead
 * @class UmbDocumentReferenceTableElement
 */
@customElement('umb-document-reference-table')
export class UmbDocumentReferenceTableElement extends UmbLitElement {
	#documentReferenceRepository = new UmbDocumentReferenceRepository(this);
	#pageSize = 10;

	@property()
	unique = '';

	@state()
	_items: Array<UmbReferenceModel> = [];

	/**
	 * Indicates if there are more references to load, i.e. if the server has more references to return.
	 * This is used to determine if the "...and X more references" text should be displayed.
	 */
	@state()
	_hasMoreReferences = 0;

	@state()
	_errorMessage = '';

	override firstUpdated() {
		new UmbDeprecation({
			removeInVersion: '17',
			deprecated: '<umb-document-reference-table> element',
			solution:
				'For modals use the <umb-confirm-action-modal-entity-references> or <umb-confirm-bulk-action-modal-entity-references> element instead',
		}).warn();

		this.#getReferences();
	}

	async #getReferences() {
		// Get the first 10 references for the document:
		const { data, error } = await this.#documentReferenceRepository.requestReferencedBy(this.unique, 0, this.#pageSize);

		if (error) {
			this._errorMessage = error.message;
			return;
		}

		if (!data) return;
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		//@ts-ignore
		this._items = data.items;
		this._hasMoreReferences = data.total > this.#pageSize ? data.total - this.#pageSize : 0;
	}

	override render() {
		return html` ${this.#renderErrorMessage()} ${this.#renderTable()} `;
	}

	#getIcon(item: UmbReferenceModel) {
		if (isDocumentReference(item)) {
			return item.documentType.icon ?? 'icon-document';
		}
		if (isMediaReference(item)) {
			return item.mediaType.icon ?? 'icon-picture';
		}
		if (isMemberReference(item)) {
			return item.memberType.icon ?? 'icon-user';
		}
		if (isDefaultReference(item)) {
			return item.icon ?? 'icon-document';
		}
		return 'icon-document';
	}

	#getContentTypeName(item: UmbReferenceModel) {
		if (isDocumentReference(item)) {
			return item.documentType.name;
		}
		if (isMediaReference(item)) {
			return item.mediaType.name;
		}
		if (isDefaultReference(item)) {
			return item.type;
		}
		return '';
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
						(item) => item.id,
						(item) =>
							html`<uui-table-row>
								<uui-table-cell style="text-align:center;">
									<umb-icon name=${this.#getIcon(item)}></umb-icon>
								</uui-table-cell>
								<uui-table-cell class="link-cell"> ${item.name} </uui-table-cell>
								<uui-table-cell>${this.#getContentTypeName(item)}</uui-table-cell>
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

	static override styles = [
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
		'umb-document-reference-table': UmbDocumentReferenceTableElement;
	}
}
