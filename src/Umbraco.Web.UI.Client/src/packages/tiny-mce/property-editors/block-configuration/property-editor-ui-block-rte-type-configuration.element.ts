import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block-type';
import { UmbInputBlockTypeElement } from '@umbraco-cms/backoffice/block-type';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import {
	UmbPropertyValueChangeEvent,
	type UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * @element umb-property-editor-ui-block-rte-type-configuration
 */
@customElement('umb-property-editor-ui-block-rte-type-configuration')
export class UmbPropertyEditorUIBlockRteBlockConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	@property({ attribute: false })
	public set value(value: UmbBlockTypeBaseModel[]) {
		this._value = value ?? [];
	}
	public get value() {
		return this._value;
	}

	@state()
	private _value: UmbBlockTypeBaseModel[] = [];

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	#onCreate(e: CustomEvent) {
		const key = e.detail.contentElementTypeKey;
		this.value = [...this._value, { contentElementTypeKey: key, forceHideContentEditorInOverlay: false }];
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}
	#onChange(e: CustomEvent) {
		this.value = (e.target as UmbInputBlockTypeElement).value;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return UmbInputBlockTypeElement
			? html`<umb-input-block-type
					entity-type="block-rte-type"
					.value=${this.value}
					@create=${this.#onCreate}
					@change=${this.#onChange}
					@delete=${this.#onChange}></umb-input-block-type>`
			: '';
	}
}

export default UmbPropertyEditorUIBlockRteBlockConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-rte-type-configuration': UmbPropertyEditorUIBlockRteBlockConfigurationElement;
	}
}
