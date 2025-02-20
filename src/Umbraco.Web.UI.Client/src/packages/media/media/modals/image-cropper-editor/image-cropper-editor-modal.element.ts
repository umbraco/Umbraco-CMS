import { UMB_MEDIA_PICKER_MODAL } from '../media-picker/media-picker-modal.token.js';
import type { UmbCropModel } from '../../types.js';
import type { UmbInputImageCropperFieldElement } from '../../components/input-image-cropper/image-cropper-field.element.js';
import type { UmbImageCropperPropertyEditorValue } from '../../components/index.js';
import { UmbMediaUrlRepository } from '../../url/index.js';
import type {
	UmbImageCropperEditorModalData,
	UmbImageCropperEditorModalValue,
} from './image-cropper-editor-modal.token.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UMB_MODAL_MANAGER_CONTEXT, UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import type { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import './components/image-cropper-editor-field.element.js';

/** TODO Make some of the components from property editor image cropper reuseable for this modal... */

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
	private _crops: Array<UmbCropModel> = [];

	@state()
	private _editMediaPath = '';

	@state()
	private _pickableFilter?: (item: any) => boolean;

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
		this._crops = this.data?.cropOptions ?? [];
		this._pickableFilter = this.data?.pickableFilter;

		this.#getSrc();
	}

	async #getSrc() {
		const { data } = await this.#urlRepository.requestItems([this._unique]);
		const item = data?.[0];

		if (!item?.url) return;

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
			focalPoint: this.value.focalPoint ?? { left: 0.5, top: 0.5 },
		};
		this._imageCropperValue = value;
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

		this.value = { ...this.value, unique: this._unique };
		this.#getSrc();
	}

	#onChange(e: CustomEvent & { target: UmbInputImageCropperFieldElement }) {
		const value = e.target.value;
		if (!value) return;

		if (this._imageCropperValue) {
			this._imageCropperValue.crops = value.crops;
			this._imageCropperValue.focalPoint = value.focalPoint;
		}

		this.value = { key: this._key, unique: this._unique, crops: value.crops, focalPoint: value.focalPoint };
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('defaultdialogs_selectMedia')}>
				${this.#renderBody()}
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

	#renderBody() {
		return html`
			<div id="layout">
				<umb-image-cropper-editor-field
					.value=${this._imageCropperValue}
					?hideFocalPoint=${this._hideFocalPoint}
					@change=${this.#onChange}>
					<div id="actions" slot="actions">
						<uui-button compact @click=${this.#openMediaPicker} label=${this.localize.term('mediaPicker_changeMedia')}>
							<uui-icon name="icon-search"></uui-icon>${this.localize.term('mediaPicker_changeMedia')}
						</uui-button>
						<uui-button
							compact
							href=${this._editMediaPath + 'edit/' + this._unique}
							label=${this.localize.term('mediaPicker_openMedia')}>
							<uui-icon name="icon-out"></uui-icon>${this.localize.term('mediaPicker_openMedia')}
						</uui-button>
					</div>
				</umb-image-cropper-editor-field>
			</div>
		`;
	}

	static override styles = [
		css`
			#layout {
				height: 100%;
				display: flex;
				flex-direction: column;
				justify-content: space-between;
			}
			umb-image-cropper-editor-field {
				flex-grow: 1;
			}

			#actions {
				display: inline-flex;
				gap: var(--uui-size-space-3);
			}
			uui-icon {
				padding-right: var(--uui-size-3);
			}

			#options {
				display: flex;
				justify-content: center;
			}

			img {
				max-width: 80%;
				background-image: url('data:image/svg+xml;charset=utf-8,<svg xmlns="http://www.w3.org/2000/svg" width="100" height="100" fill-opacity=".1"><path d="M50 0h50v50H50zM0 50h50v50H0z"/></svg>');
				background-size: 10px 10px;
				background-repeat: repeat;
			}
		`,
	];
}

export default UmbImageCropperEditorModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-image-cropper-editor-modal': UmbImageCropperEditorModalElement;
	}
}
