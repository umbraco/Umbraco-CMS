import type { UmbEditableDocumentCollectionItemModel } from '../../../types.js';
import { UmbDocumentItemDataResolver } from '../../../../item/index.js';
import { UmbDocumentVariantState } from '../../../../variant-state.js';
import { css, customElement, html, nothing, property, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbTableColumn, UmbTableColumnLayoutElement, UmbTableItem } from '@umbraco-cms/backoffice/components';

/**
 * Reads the text a node renders, through shadow roots and slotted content, which `innerText` does not see.
 * @param {Node} node - The node to read.
 * @returns {string} The rendered text, whitespace as authored.
 */
function getFlatTreeText(node: Node): string {
	if (node.nodeType === Node.TEXT_NODE) return node.textContent ?? '';
	if (node instanceof HTMLStyleElement) return '';
	const children =
		node instanceof HTMLSlotElement
			? node.assignedNodes({ flatten: true })
			: Array.from(((node as Element).shadowRoot ?? node).childNodes);
	return children.map(getFlatTreeText).join('');
}

@customElement('umb-document-table-column-property-value')
export class UmbDocumentTableColumnPropertyValueElement extends UmbLitElement implements UmbTableColumnLayoutElement {
	#resolver = new UmbDocumentItemDataResolver(this);

	@state()
	private _createDate?: Date;

	@state()
	private _state?: string;

	@state()
	private _updateDate?: Date;

	column!: UmbTableColumn;
	item!: UmbTableItem;

	@property({ attribute: false })
	public set value(value: UmbEditableDocumentCollectionItemModel) {
		this.#value = value;

		if (value.item) {
			this.#resolver.setData(value.item);
		}
	}
	public get value(): UmbEditableDocumentCollectionItemModel {
		return this.#value;
	}
	#value!: UmbEditableDocumentCollectionItemModel;

	constructor() {
		super();

		this.#resolver.observe(this.#resolver.state, (state) => (this._state = state || ''));
		this.#resolver.observe(this.#resolver.createDate, (createDate) => (this._createDate = createDate));
		this.#resolver.observe(this.#resolver.updateDate, (updateDate) => (this._updateDate = updateDate));
		this.addEventListener('pointerenter', this.#onPointerEnter);
	}

	// Value summaries and label templates render later, inside nested shadow roots, so the text is read on hover.
	#onPointerEnter = () => {
		const isClamped = this.scrollHeight > this.clientHeight || this.scrollWidth > this.clientWidth;
		const text = isClamped ? getFlatTreeText(this).replace(/\s+/g, ' ').trim() : '';
		if (text) {
			this.title = text;
		} else {
			this.removeAttribute('title');
		}
	};

	#getPropertyValueByAlias() {
		const alias = this.column.alias;
		const item = this.value.item;
		switch (alias) {
			case 'contentTypeAlias':
				return { value: item.documentType.alias };
			case 'createDate':
				return { value: this._createDate?.toLocaleString(this.localize.lang()) };
			case 'creator':
			case 'owner':
				return { value: item.creator };
			case 'published':
				return { value: this._state !== UmbDocumentVariantState.DRAFT ? 'True' : 'False' };
			case 'sortOrder':
				return { value: item.sortOrder };
			case 'updateDate':
				return { value: this._updateDate?.toLocaleString(this.localize.lang()) };
			case 'updater':
				return { value: item.updater };
			default: {
				const culture = this.#resolver.getCulture();
				const prop = item.values.find((x) => x.alias === alias && (!x.culture || x.culture === culture));
				return { value: prop?.value ?? '', editorAlias: prop?.editorAlias };
			}
		}
	}

	override render() {
		if (!this.value) return nothing;
		const { value, editorAlias } = this.#getPropertyValueByAlias();
		return when(
			this.column.labelTemplate,
			() => html`<umb-ufm-render inline .markdown=${this.column.labelTemplate} .value=${{ value }}></umb-ufm-render>`,
			() =>
				editorAlias
					? html`<umb-value-summary-extension .valueType=${editorAlias} .value=${value}></umb-value-summary-extension>`
					: value,
		);
	}

	static override styles = [
		css`
			/* Clamped here rather than with the cell's clip-text, so the column can still size to the value. */
			:host {
				display: -webkit-box;
				-webkit-box-orient: vertical;
				-webkit-line-clamp: 1;
				overflow: hidden;
				overflow-wrap: anywhere;
				max-inline-size: 25ch;
			}
		`,
	];
}

export default UmbDocumentTableColumnPropertyValueElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-table-column-property-value': UmbDocumentTableColumnPropertyValueElement;
	}
}
