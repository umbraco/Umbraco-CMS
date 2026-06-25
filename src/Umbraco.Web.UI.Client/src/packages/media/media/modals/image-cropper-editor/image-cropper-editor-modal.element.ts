import { UMB_MEDIA_PICKER_MODAL } from '../media-picker/media-picker-modal.token.js';
import { UmbMediaUrlRepository } from '../../url/index.js';
import type { UmbCropModel, UmbMediaItemModel } from '../../types.js';
import type { UmbImageCropperPropertyEditorValue } from '../../components/index.js';
import type {
	UmbImageCropperEditorModalData,
	UmbImageCropperEditorModalValue,
} from './image-cropper-editor-modal.token.js';
import type { UmbImageCropperEditorFieldElement } from './components/image-cropper-editor-field.element.js';
import { css, customElement, html, nothing, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbTemporaryFileConfigRepository } from '@umbraco-cms/backoffice/temporary-file';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_MODAL_MANAGER_CONTEXT, UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import type { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';

import './components/index.js';
import '../../components/input-upload-field/file-upload-preview.element.js';

@customElement('umb-image-cropper-editor-modal')
export class UmbImageCropperEditorModalElement extends UmbModalBaseElement<
	UmbImageCropperEditorModalData<any>,
	UmbImageCropperEditorModalValue
> {
	#urlRepository = new UmbMediaUrlRepository(this);

	@state()
	private _imageCropperValue?: UmbImageCropperPropertyEditorValue;

	@state()
	private _key: string = '';

	@state()
	private _unique: string = '';

	@state()
	private _hideFocalPoint = false;

	@state()
	private _hideZoomCrop = false;

	@state()
	private _enableAltTextPerCrop = false;

	@state()
	private _altTextMode: 'off' | 'altText' | 'decorative' = 'off';

	@state()
	private _altText: string = '';

	@state()
	private _culture?: string;

	@state()
	private _readonlyMedia = false;

	@state()
	private _statusAnnouncement = '';

	@state()
	private _crops: Array<UmbCropModel> = [];

	@state()
	private _editMediaPath = '';

	@state()
	private _pickableFilter?: (item: UmbMediaItemModel) => boolean;

	@state()
	private _isCroppable = false;

	#config = new UmbTemporaryFileConfigRepository(this);

	#imageFileTypes?: Array<string>;

	#modalManager?: UmbModalManagerContext;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (context) => {
			this.#modalManager = context;
		});

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('media')
			.onSetup(() => {
				return { data: { entityType: 'media', preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editMediaPath = routeBuilder({});
			});
	}

	override connectedCallback(): void {
		super.connectedCallback();

		this._key = this.data?.key ?? '';
		this._unique = this.data?.unique ?? '';

		this._hideFocalPoint = this.data?.hideFocalPoint ?? false;
		this._hideZoomCrop = this.data?.hideZoomCrop ?? false;
		this._enableAltTextPerCrop = this.data?.enableAltTextPerCrop ?? false;
		this._altTextMode = this.data?.altTextMode ?? 'off';
		this._altText = this.value?.altText ?? '';
		this._culture = this.data?.culture;
		this._readonlyMedia = this.data?.readonlyMedia ?? false;
		this._crops = this.data?.cropOptions ?? [];
		this._pickableFilter = this.data?.pickableFilter;

		this.#observeAcceptedFileTypes();
		this.#getSrc();
	}

	async #observeAcceptedFileTypes() {
		await this.#config.initialized;
		this.observe(
			this.#config.part('imageFileTypes'),
			(imageFileTypes) => (this.#imageFileTypes = imageFileTypes),
			'_observeFileTypes',
		);
	}

	override firstUpdated(_changedProperties: Map<PropertyKey, unknown>) {
		super.firstUpdated(_changedProperties);
		if (this._readonlyMedia && this._culture) {
			this._statusAnnouncement = this.localize.term('mediaPicker_sharedPropertyCultureNotice', [this._culture]);
		}
	}

	async #getSrc() {
		const { data } = await this.#urlRepository.requestItems([this._unique]);
		const item = data?.[0];

		if (!item?.url) {
			this._isCroppable = false;
			this._imageCropperValue = undefined;
			this.#focusFirstInteractive();
			return;
		}

		if (item.extension && this.#imageFileTypes?.includes(item.extension)) {
			this._isCroppable = true;
		}

		/**
		 * Combine the crops from the property editor with the stored crops and ignore any invalid crops
		 * (e.g. crops that have been removed from the property editor)
		 * @remark If a crop is removed from the property editor, it will be ignored and not saved
		 */
		const crops: Array<UmbCropModel> = this._crops.map((crop) => {
			const existingCrop = this.value.crops?.find((c) => c.alias === crop.alias);
			return existingCrop ? { ...crop, ...existingCrop } : crop;
		});

		const value: UmbImageCropperPropertyEditorValue = {
			...this.value,
			src: item.url,
			crops,
			focalPoint: this.value.focalPoint ?? null,
		};
		this._imageCropperValue = value;

		await this.updateComplete;
		this.#focusFirstInteractive();
	}

	async #focusFirstInteractive() {
		await this.updateComplete;
		const field = this.shadowRoot?.querySelector<HTMLElement>('umb-image-cropper-editor-field');
		if (field) {
			field.focus();
			return;
		}
		const firstButton = this.shadowRoot?.querySelector<HTMLElement>('uui-button');
		firstButton?.focus();
	}

	async #openMediaPicker() {
		const modal = this.#modalManager?.open(this, UMB_MEDIA_PICKER_MODAL, {
			data: { multiple: false, pickableFilter: this._pickableFilter },
			value: { selection: [this._unique] },
		});
		const data = await modal?.onSubmit().catch(() => null);
		if (!data) return;

		const selected = data.selection[0];

		if (!selected) {
			throw new Error('No media selected');
		}

		this._unique = selected;
		this._altText = '';

		this.value = { ...this.value, unique: this._unique, altText: '' };

		this._isCroppable = false;

		this.#getSrc();
	}

	#onChange(e: CustomEvent & { target: UmbImageCropperEditorFieldElement }) {
		const value = e.target.value;
		if (!value) return;

		this._altText = e.target.altText;

		if (this._imageCropperValue) {
			this._imageCropperValue.crops = value.crops;
			this._imageCropperValue.focalPoint = value.focalPoint;
		}

		this.value = {
			key: this._key,
			unique: this._unique,
			crops: value.crops,
			focalPoint: value.focalPoint,
			altText: this._altText,
		};

		this._statusAnnouncement = this.localize.term('mediaPicker_cropSaved');
	}

	override render() {
		return html`
			<div class="sr-only" role="status" aria-live="polite" aria-atomic="true">${this._statusAnnouncement}</div>
			<umb-body-layout headline=${this.localize.term('defaultdialogs_selectMedia')}>
				<div id="content">
					<div id="layout">
						${when(
							this._isCroppable,
							() => this.#renderImageCropper(),
							() => this.#renderFilePreview(),
						)}
					</div>
				</div>
				<div slot="actions">
					<uui-button label=${this.localize.term('general_close')} @click=${this._rejectModal}></uui-button>
					<uui-button
						label=${this.localize.term('general_submit')}
						look="primary"
						color="positive"
						@click=${this._submitModal}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	#renderActions() {
		return html`
			<uui-button compact label=${this.localize.term('mediaPicker_changeMedia')} @click=${this.#openMediaPicker}>
				<uui-icon name="icon-search"></uui-icon>
				<umb-localize key="mediaPicker_changeMedia">Change Media Item</umb-localize>
			</uui-button>
			<uui-button
				compact
				label=${this.localize.term('mediaPicker_openMedia')}
				href=${this._editMediaPath + 'edit/' + this._unique}>
				<uui-icon name="icon-out"></uui-icon>
				<umb-localize key="mediaPicker_openMedia">Open in Media Library</umb-localize>
			</uui-button>
		`;
	}

	#renderImageCropper() {
		if (!this._imageCropperValue) return nothing;
		return html`
			${this._readonlyMedia && this._culture ? this.#renderCultureNotice() : nothing}
			<umb-image-cropper-editor-field
				.value=${this._imageCropperValue}
				.altText=${this._altText}
				.altTextMode=${this._altTextMode}
				.culture=${this._culture}
				?hideFocalPoint=${this._hideFocalPoint || this._readonlyMedia}
				?hideZoomCrop=${this._hideZoomCrop || this._readonlyMedia}
				?enable-alt-text-per-crop=${this._enableAltTextPerCrop}
				?readonly-media=${this._readonlyMedia}
				@change=${this.#onChange}>
				<div slot="actions">${this._readonlyMedia ? nothing : this.#renderActions()}</div>
			</umb-image-cropper-editor-field>
		`;
	}

	#renderCultureNotice() {
		return html`
			<div class="culture-notice" role="note">
				<uui-icon name="icon-info" aria-hidden="true"></uui-icon>
				<span>
					<umb-localize key="mediaPicker_sharedPropertyCultureNotice" .args=${[this._culture]}
						>Editing alternative text for the ${this._culture} language. Image selection and crop editing are only
						available on the default language tab.</umb-localize
					>
				</span>
			</div>
		`;
	}

	#renderFilePreview() {
		return html`
			<div id="main">
				${when(
					this._imageCropperValue?.src,
					(path) => html`<umb-file-upload-preview .path=${path}></umb-file-upload-preview>`,
					() => this.#renderFileNotFound(),
				)}
			</div>
			<div id="actions">${this.#renderActions()}</div>
		`;
	}

	#renderFileNotFound() {
		const args = [this.localize.term('general_media')];
		return html`
			<div class="uui-text">
				<h4>
					<umb-localize key="entityDetail_notFoundTitle" .args=${args}>Item not found</umb-localize>
				</h4>
				<umb-localize key="entityDetail_notFoundDescription">The requested item could not be found.</umb-localize>
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			.culture-notice {
				display: flex;
				align-items: flex-start;
				gap: var(--uui-size-space-3);
				padding: var(--uui-size-space-3) var(--uui-size-space-4);
				background: var(--uui-color-surface-emphasis);
				border-left: 3px solid var(--uui-color-interactive);
				font-size: var(--uui-type-small-size);
				color: var(--uui-color-text-alt);
				flex-shrink: 0;

				uui-icon {
					flex-shrink: 0;
					margin-top: 1px;
				}
			}

			#content {
				display: flex;
				flex-direction: column;
				height: 100%;
				min-height: 0;
			}

			#layout {
				flex: 1;
				min-height: 0;
				display: flex;
				flex-direction: column;
				justify-content: space-between;
			}

			umb-image-cropper-editor-field {
				flex: 1;
				min-height: 0;
			}

			#main {
				flex: 1;
				background-color: var(--uui-color-surface);
				outline: 1px solid var(--uui-color-border);
			}

			#actions {
				display: flex;
				margin-top: 0.5rem;

				uui-icon {
					padding-right: var(--uui-size-1);
				}
			}

			.uui-text {
				display: flex;
				flex-direction: column;
				justify-content: center;
				align-items: center;
				height: 100%;
			}

		`,
	];
}

export { UmbImageCropperEditorModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-image-cropper-editor-modal': UmbImageCropperEditorModalElement;
	}
}
