import type {
	UmbDropzoneMediaTypePickerModalData,
	UmbDropzoneMediaTypePickerModalValue,
} from './dropzone-media-type-picker-modal.token.js';
import { css, customElement, html, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbAllowedMediaTypeModel } from '@umbraco-cms/backoffice/media-type';

@customElement('umb-dropzone-media-type-picker-modal')
export class UmbDropzoneMediaTypePickerModalElement extends UmbModalBaseElement<
	UmbDropzoneMediaTypePickerModalData,
	UmbDropzoneMediaTypePickerModalValue
> {
	@state()
	private _options: Array<UmbAllowedMediaTypeModel> = [];

	connectedCallback() {
		super.connectedCallback();
		this._options = this.data?.options ?? [];
	}

	#onMediaTypePick(unique?: string) {
		this.value = { mediaTypeUnique: unique };
		this._submitModal();
	}

	render() {
		return html`<uui-button look="secondary" @click=${() => this.#onMediaTypePick()} label="Automatically" compact>
				<umb-icon name="icon-wand"></umb-icon
			></uui-button>
			${repeat(
				this._options,
				(option) => option.unique,
				(option) =>
					html`<uui-button
						look="secondary"
						@click=${() => this.#onMediaTypePick(option.unique)}
						label=${option.name}
						compact>
						<umb-icon .name=${option.icon ?? 'icon-circle-dotted'}></umb-icon>
					</uui-button>`,
			)}`;
	}

	static styles = [UmbTextStyles, css``];
}

export default UmbDropzoneMediaTypePickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dropzone-media-type-picker-modal': UmbDropzoneMediaTypePickerModalElement;
	}
}
