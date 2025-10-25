import { html, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_ICON_PICKER_MODAL } from '@umbraco-cms/backoffice/icon';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { extractUmbColorVariable } from '@umbraco-cms/backoffice/resources';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

/**
 * @element umb-property-editor-ui-icon-picker
 */
@customElement('umb-property-editor-ui-icon-picker')
export class UmbPropertyEditorUIIconPickerElement
	extends UmbFormControlMixin<string, typeof UmbLitElement, undefined>(UmbLitElement, undefined)
	implements UmbPropertyEditorUiElement
{
	@property({ type: Boolean })
	mandatory = false;

	protected override firstUpdated(): void {
		this.addValidator(
			'valueMissing',
			() => 'Icon is required',
			() => this.mandatory && !this._icon,
		);
	}

	@property()
	public override set value(v: string) {
		const val = v ?? '';
		super.value = val;

		const parts = val.split(' ');
		if (parts.length === 2) {
			this._icon = parts[0];
			this._color = parts[1].replace('color-', '');
		} else {
			this._icon = val;
			this._color = '';
		}
	}

	public override get value() {
		return (super.value as string) ?? '';
	}

	@state()
	private _icon = '';

	@state()
	private _color = '';

	@state()
	private _placeholderIcon = '';

	@state()
	private _hideColors = false;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;
		const placeholder = config.getValueByAlias('placeholder');
		this._placeholderIcon = typeof placeholder === 'string' ? placeholder : '';
		this._hideColors = config.getValueByAlias('hideColors') as boolean;
	}

	private async _openModal() {
		const data = await umbOpenModal(this, UMB_ICON_PICKER_MODAL, {
			value: {
				icon: this._icon,
				color: this._color,
			},
			data: { placeholder: this._placeholderIcon, showEmptyOption: !this.mandatory, hideColors: this._hideColors },
		}).catch(() => undefined);

		if (!data) return;

		if (!data.icon) {
			this.value = '';
		} else if (data.color) {
			this.value = `${data.icon} color-${data.color}`;
		} else {
			this.value = data.icon as string;
		}

		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		const isEmpty = !this._icon;

		return html`
			<uui-button
				compact
				look="outline"
				label=${this.localize.term('defaultdialogs_selectIcon')}
				@click=${this._openModal}>
				${isEmpty
					? html` <uui-icon name="${ifDefined(this._placeholderIcon)}" style="opacity:.35"></uui-icon> `
					: this._color
						? html`
								<uui-icon name="${this._icon}" style="color:var(${extractUmbColorVariable(this._color)})"> </uui-icon>
							`
						: html`<uui-icon name="${this._icon}"></uui-icon>`}
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
