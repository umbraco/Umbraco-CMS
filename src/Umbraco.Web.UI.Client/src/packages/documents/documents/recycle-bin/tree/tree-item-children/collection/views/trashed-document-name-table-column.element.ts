import { UmbDocumentItemDataResolver } from '../../../../../item/index.js';
import type { UmbDocumentRecycleBinTreeItemModel } from '../../../types.js';
import { UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN } from '../../../../../paths.js';
import { css, customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbTableColumn, UmbTableColumnLayoutElement, UmbTableItem } from '@umbraco-cms/backoffice/components';

@customElement('umb-trashed-document-name-table-column')
export class UmbTrashedDocumentNameTableColumnElement extends UmbLitElement implements UmbTableColumnLayoutElement {
	#resolver = new UmbDocumentItemDataResolver(this);

	@state()
	private _name = '';

	@state()
	private _editPath = '';

	column!: UmbTableColumn;
	item!: UmbTableItem;

	@property({ attribute: false })
	public set value(value: UmbDocumentRecycleBinTreeItemModel) {
		this.#value = value;

		if (value) {
			this.#resolver.setData(value);

			this._editPath = UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN.generateAbsolute({
				unique: value.unique,
			});
		}
	}
	public get value(): UmbDocumentRecycleBinTreeItemModel {
		return this.#value;
	}
	#value!: UmbDocumentRecycleBinTreeItemModel;

	constructor() {
		super();
		this.#resolver.observe(this.#resolver.name, (name) => (this._name = name || ''));
	}

	override render() {
		if (!this.value) return nothing;
		if (!this._name) return nothing;
		return html`<uui-button compact href=${this._editPath} label=${this._name}></uui-button>`;
	}

	static override styles = [
		css`
			uui-button {
				text-align: left;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-trashed-document-name-table-column': UmbTrashedDocumentNameTableColumnElement;
	}
}
