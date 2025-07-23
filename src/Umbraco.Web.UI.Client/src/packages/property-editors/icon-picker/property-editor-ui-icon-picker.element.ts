import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_ICON_PICKER_MODAL } from '@umbraco-cms/backoffice/icon';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { extractUmbColorVariable } from '@umbraco-cms/backoffice/resources';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

/**
 * @element umb-property-editor-ui-icon-picker
 */
@customElement('umb-property-editor-ui-icon-picker')
export class UmbPropertyEditorUIIconPickerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	//
	@property()
	public set value(v: string) {
		this._value = v ?? '';
		const parts = this._value.split(' ');
		if (parts.length === 2) {
			this._icon = parts[0];
			this._color = parts[1].replace('color-', '');
		} else {
			this._icon = this._value;
			this._color = '';
		}
	}
	public get value() {
		return this._value;
	}
	private _value = '';

	@state()
	private _icon = '';

	@state()
	private _color = '';

	private async _openModal() {
		const data = await umbOpenModal(this, UMB_ICON_PICKER_MODAL).catch(() => undefined);
		if (!data) return;

		if (data.color) {
			this.value = `${data.icon} color-${data.color}`;
		} else {
			this.value = data.icon as string;
		}

		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<uui-button
				compact
				label=${this.localize.term('defaultdialogs_selectIcon')}
				look="outline"
				@click=${this._openModal}>
				${this._color
					? html` <uui-icon name="${this._icon}" style="color:var(${extractUmbColorVariable(this._color)})"></uui-icon>`
					: html` <uui-icon name="${this._icon}"></uui-icon>`}
			</uui-button>
		`;
	}
}

export default UmbPropertyEditorUIIconPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-icon-picker': UmbPropertyEditorUIIconPickerElement;
	}
}
