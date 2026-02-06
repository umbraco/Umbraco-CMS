import { UMB_EDIT_ELEMENT_WORKSPACE_PATH_PATTERN } from '../../../paths.js';
import { UMB_EDIT_ELEMENT_FOLDER_WORKSPACE_PATH_PATTERN } from '../../../folder/workspace/constants.js';
import type { UmbElementRecycleBinTreeItemModel } from '../../tree/types.js';
//import { UmbElementItemDataResolver } from '../../../item/data-resolver/element-item-data-resolver.js';
import { css, customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbTableColumn, UmbTableColumnLayoutElement, UmbTableItem } from '@umbraco-cms/backoffice/components';

@customElement('umb-trashed-element-name-table-column')
export class UmbTrashedElementNameTableColumnElement extends UmbLitElement implements UmbTableColumnLayoutElement {
	//#resolver = new UmbElementItemDataResolver(this);

	@state()
	private _name = '';

	@state()
	private _editPath = '';

	column!: UmbTableColumn;
	item!: UmbTableItem;

	@property({ attribute: false })
	public set value(value: UmbElementRecycleBinTreeItemModel) {
		this.#value = value;

		if (value) {
			//this.#resolver.setData(value);
			this._name = value.name;

			const pathPattern = value.isFolder
				? UMB_EDIT_ELEMENT_FOLDER_WORKSPACE_PATH_PATTERN
				: UMB_EDIT_ELEMENT_WORKSPACE_PATH_PATTERN;

			this._editPath = pathPattern.generateAbsolute({
				unique: value.unique,
			});
		}
	}
	public get value(): UmbElementRecycleBinTreeItemModel {
		return this.#value;
	}
	#value!: UmbElementRecycleBinTreeItemModel;

	constructor() {
		super();
		//this.#resolver.observe(this.#resolver.name, (name) => (this._name = name || ''));
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
		'umb-trashed-element-name-table-column': UmbTrashedElementNameTableColumnElement;
	}
}
