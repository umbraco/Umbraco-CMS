import type { UmbMediaRecycleBinTreeItemModel } from '../../../types.js';
import { UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN } from '../../../../../paths.js';
import { css, customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbTableColumn, UmbTableColumnLayoutElement, UmbTableItem } from '@umbraco-cms/backoffice/components';

@customElement('umb-trashed-media-name-table-column')
export class UmbTrashedMediaNameTableColumnElement extends UmbLitElement implements UmbTableColumnLayoutElement {
	@state()
	private _name = '';

	@state()
	private _editPath = '';

	column!: UmbTableColumn;
	item!: UmbTableItem;

	@property({ attribute: false })
	public set value(value: UmbMediaRecycleBinTreeItemModel) {
		this.#value = value;

		if (value) {
			this._name = value.variants[0]?.name || '';

			this._editPath = UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN.generateAbsolute({
				unique: value.unique,
			});
		}
	}
	public get value(): UmbMediaRecycleBinTreeItemModel {
		return this.#value;
	}
	#value!: UmbMediaRecycleBinTreeItemModel;

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
		'umb-trashed-media-name-table-column': UmbTrashedMediaNameTableColumnElement;
	}
}
