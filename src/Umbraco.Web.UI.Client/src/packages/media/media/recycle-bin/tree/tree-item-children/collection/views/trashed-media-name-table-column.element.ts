import type { UmbMediaRecycleBinTreeItemModel } from '../../../types.js';
import { UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN } from '../../../../../paths.js';
import { resolveVariantName } from '../../../../../item/index.js';
import { css, customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbTableColumn, UmbTableColumnLayoutElement, UmbTableItem } from '@umbraco-cms/backoffice/components';
import { UMB_VARIANT_CONTEXT } from '@umbraco-cms/backoffice/variant';

@customElement('umb-trashed-media-name-table-column')
export class UmbTrashedMediaNameTableColumnElement extends UmbLitElement implements UmbTableColumnLayoutElement {
	@state()
	private _name = '';

	@state()
	private _editPath = '';

	#displayCulture?: string | null;
	#fallbackCulture?: string | null;

	column!: UmbTableColumn;
	item!: UmbTableItem;

	@property({ attribute: false })
	public set value(value: UmbMediaRecycleBinTreeItemModel) {
		this.#value = value;
		this.#updateName();

		if (value) {
			this._editPath = UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN.generateAbsolute({
				unique: value.unique,
			});
		}
	}
	public get value(): UmbMediaRecycleBinTreeItemModel {
		return this.#value;
	}
	#value!: UmbMediaRecycleBinTreeItemModel;

	constructor() {
		super();
		this.consumeContext(UMB_VARIANT_CONTEXT, (context) => {
			this.observe(context?.displayCulture, (displayCulture) => {
				this.#displayCulture = displayCulture;
				this.#updateName();
			});
			this.observe(context?.fallbackCulture, (fallbackCulture) => {
				this.#fallbackCulture = fallbackCulture;
				this.#updateName();
			});
		});
	}

	#updateName() {
		this._name = this.#value
			? resolveVariantName(this.#value.variants, this.#displayCulture, this.#fallbackCulture)
			: '';
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
		'umb-trashed-media-name-table-column': UmbTrashedMediaNameTableColumnElement;
	}
}
