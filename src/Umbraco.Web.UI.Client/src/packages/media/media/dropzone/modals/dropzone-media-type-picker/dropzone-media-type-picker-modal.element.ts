import type {
	UmbDropzoneMediaTypePickerModalData,
	UmbDropzoneMediaTypePickerModalValue,
} from './dropzone-media-type-picker-modal.token.js';
import { css, customElement, html, query, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbAllowedMediaTypeModel } from '@umbraco-cms/backoffice/media-type';
import type { UUIButtonElement } from '@umbraco-cms/backoffice/external/uui';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';

@customElement('umb-dropzone-media-type-picker-modal')
export class UmbDropzoneMediaTypePickerModalElement extends UmbModalBaseElement<
	UmbDropzoneMediaTypePickerModalData,
	UmbDropzoneMediaTypePickerModalValue
> {
	@state()
	private _options: Array<UmbAllowedMediaTypeModel> = [];

	@query('#auto')
	private _buttonAuto!: UUIButtonElement;

	override connectedCallback() {
		super.connectedCallback();
		this._options = this.data?.options ?? [];
		requestAnimationFrame(() => this._buttonAuto.focus());
	}

	#onAutoPick() {
		this.value = { mediaTypeUnique: undefined };
		this._submitModal();
	}

	#onMediaTypePick(unique: UmbEntityUnique) {
		if (!unique) {
			throw new Error('Invalid media type unique');
		}
		this.value = { mediaTypeUnique: unique };
		this._submitModal();
	}

	override render() {
		return html` <div id="options">
			<uui-button id="auto" look="secondary" @click=${() => this.#onAutoPick()} label="Automatically" compact>
				<umb-icon name="icon-wand"></umb-icon> Auto pick
			</uui-button>
			${repeat(
				this._options,
				(option) => option.unique,
				(option) =>
					html`<uui-button
						look="secondary"
						@click=${() => this.#onMediaTypePick(option.unique)}
						label=${option.name}
						compact>
						<umb-icon .name=${option.icon ?? 'icon-circle-dotted'}></umb-icon>${option.name}
					</uui-button>`,
			)}
		</div>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#options {
				display: flex;
				margin: var(--uui-size-layout-1);
				gap: var(--uui-size-3);
			}
			uui-button {
				width: 150px;
				height: 150px;
			}
			umb-icon {
				font-size: var(--uui-size-10);
				margin-bottom: var(--uui-size-2);
			}
		`,
	];
}

export default UmbDropzoneMediaTypePickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dropzone-media-type-picker-modal': UmbDropzoneMediaTypePickerModalElement;
	}
}
