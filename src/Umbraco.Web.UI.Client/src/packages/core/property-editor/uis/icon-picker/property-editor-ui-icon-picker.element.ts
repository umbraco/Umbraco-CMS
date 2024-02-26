import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_MANAGER_CONTEXT, UMB_ICON_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { extractUmbColorVariable } from '@umbraco-cms/backoffice/resources';

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

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;
		//console.log('config', config);
	}

	private _modalContext?: UmbModalManagerContext;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this._modalContext = instance;
		});
	}

	private async _openModal() {
		const modalContext = this._modalContext?.open(UMB_ICON_PICKER_MODAL);

		const data = await modalContext?.onSubmit();
		if (!data) return;

		if (data.color) {
			this.value = `${data.icon} color-${data.color}`;
		} else {
			this.value = data.icon as string;
		}

		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`
			<uui-button
				compact
				label=${this.localize.term('defaultdialogs_selectIcon')}
				look="secondary"
				@click=${this._openModal}
				style="margin-right: var(--uui-size-space-3)">
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
