import type { UmbImageCropperPropertyEditorValue, UmbInputImageCropperElement } from '../../components/index.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type {
	UmbPropertyEditorUiElement,
	UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

import '../../components/input-image-cropper/input-image-cropper.element.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UMB_IS_TRASHED_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/recycle-bin';
import { UMB_SERVER_CONTEXT } from '@umbraco-cms/backoffice/server';

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

	#trashedEntityContext?: typeof UMB_IS_TRASHED_ENTITY_CONTEXT.TYPE;
	#serverContext?: typeof UMB_SERVER_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_IS_TRASHED_ENTITY_CONTEXT, (context) => {
			this.#trashedEntityContext = context;
		});

		this.consumeContext(UMB_SERVER_CONTEXT, (context) => {
			this.#serverContext = context;
		});
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

	#getValue() {
		// If the media item is in the recycle bin and media recycle bin protection is on, the media file will have been
		// renamed to have a .deleted suffix (e.g. media/xxx/test.png will be media/xxx/test.deleted.png on disk).
		const isTrashed = this.#trashedEntityContext?.getIsTrashed();
		const mediaRecycleBinProtectionEnabled = this.#serverContext?.getServerConnection().enableMediaRecycleBinProtection;
		return isTrashed && mediaRecycleBinProtectionEnabled
			? { ...this.value, src: this.#updateSrcToProtectedFile(this.value?.src) }
			: this.value;
	}

	#updateSrcToProtectedFile(src: string | undefined) {
		if (!src) {
			return undefined;
		}

		const lastDotIndex = src.lastIndexOf('.');

		if (lastDotIndex === -1) {
			return src + '.deleted';
		}

		return src.slice(0, lastDotIndex) + '.deleted' + src.slice(lastDotIndex);
	}

	override render() {
		return html`<umb-input-image-cropper
			@change=${this.#onChange}
			.value=${this.#getValue()}
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
