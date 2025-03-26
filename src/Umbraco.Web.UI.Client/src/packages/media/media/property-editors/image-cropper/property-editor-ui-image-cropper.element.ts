import type { UmbImageCropperPropertyEditorValue, UmbInputImageCropperElement } from '../../components/index.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import {
	type UmbPropertyEditorUiElement,
	type UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

import '../../components/input-image-cropper/input-image-cropper.element.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

/**
 * @element umb-property-editor-ui-image-cropper
 */
@customElement('umb-property-editor-ui-image-cropper')
export class UmbPropertyEditorUIImageCropperElement
	extends UmbFormControlMixin<UmbImageCropperPropertyEditorValue | undefined, typeof UmbLitElement, undefined>(
		UmbLitElement,
	)
	implements UmbPropertyEditorUiElement
{
	/**
	 * Sets the input to mandatory, meaning validation will fail if the value is empty.
	 * @type {boolean}
	 */
	@property({ type: Boolean })
	mandatory?: boolean;

	@property({ type: String })
	mandatoryMessage = UMB_VALIDATION_EMPTY_LOCALIZATION_KEY;

	@state()
	crops: UmbImageCropperPropertyEditorValue['crops'] = [];

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this.crops = config?.getValueByAlias<UmbImageCropperPropertyEditorValue['crops']>('crops') ?? [];
	}

	override firstUpdated() {
		this.addFormControlElement(this.shadowRoot!.querySelector('umb-input-image-cropper')!);
	}

	override focus() {
		return this.shadowRoot?.querySelector('umb-input-image-cropper')?.focus();
	}

	#onChange(e: Event) {
		this.value = (e.target as UmbInputImageCropperElement).value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`<umb-input-image-cropper
			@change=${this.#onChange}
			.value=${this.value}
			.crops=${this.crops}
			.required=${this.mandatory}
			.requiredMessage=${this.mandatoryMessage}></umb-input-image-cropper>`;
	}
}

export default UmbPropertyEditorUIImageCropperElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-image-cropper': UmbPropertyEditorUIImageCropperElement;
	}
}
