import type {
	UmbDropzoneMediaTypePickerModalData,
	UmbDropzoneMediaTypePickerModalValue,
} from './dropzone-media-type-picker-modal.token.js';
import { css, customElement, html, nothing, query, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbAllowedMediaTypeModel } from '@umbraco-cms/backoffice/media-type';
import type { UUIButtonElement } from '@umbraco-cms/backoffice/external/uui';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';

const VISIBLE_FILE_NAMES = 3;

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
		return html`<uui-dialog-layout headline=${this.localize.term('media_chooseMediaTypeHeadline')}>
			${this.#renderFiles()}
			<div id="options">
				<uui-button
					id="auto"
					look="secondary"
					@click=${() => this.#onAutoPick()}
					label=${this.localize.term('mediaType_autoPickMediaType')}
					compact>
					<umb-icon name="icon-wand"></umb-icon>${this.localize.term('mediaType_autoPickMediaType')}
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
			</div>
			<uui-button
				id="cancel"
				slot="actions"
				label=${this.localize.term('general_cancel')}
				@click=${() => this._rejectModal()}></uui-button>
		</uui-dialog-layout>`;
	}

	#renderFiles() {
		const files = this.data?.files ?? [];
		if (!files.length) return nothing;

		const names = files.slice(0, VISIBLE_FILE_NAMES).map((file) => file.name);
		const remaining = files.length - names.length;
		if (remaining > 0) names.push(this.localize.term('media_chooseMediaTypeMoreFiles', remaining));

		return html`<p id="files">${this.localize.list(names)}</p>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#files {
				margin-top: 0;
				word-break: break-word;
			}
			#options {
				display: flex;
				flex-wrap: wrap;
				gap: var(--uui-size-3);
			}
			#options uui-button {
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
