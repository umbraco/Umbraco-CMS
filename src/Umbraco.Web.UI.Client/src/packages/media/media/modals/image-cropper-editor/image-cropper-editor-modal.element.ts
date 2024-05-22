import { UmbMediaUrlRepository } from '../../repository/index.js';
import type { UmbCropModel } from '../../property-editors/index.js';
import type { UmbInputImageCropperFieldElement } from '../../components/input-image-cropper/image-cropper-field.element.js';
import type { UmbImageCropperPropertyEditorValue } from '../../components/index.js';
import type {
	UmbImageCropperEditorModalData,
	UmbImageCropperEditorModalValue,
} from './image-cropper-editor-modal.token.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

/** TODO Make some of the components from property editor image cropper reuseable for this modal... */

@customElement('umb-image-cropper-editor-modal')
export class UmbImageCropperEditorModalElement extends UmbModalBaseElement<
	UmbImageCropperEditorModalData,
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
	private _focalPointEnabled = false;
	/** TODO  allow to enable/disable focalpoint */

	@state()
	private _crops: Array<UmbCropModel> = [];

	connectedCallback(): void {
		super.connectedCallback();

		this._key = this.data?.key ?? '';
		this._unique = this.data?.unique ?? '';

		this._focalPointEnabled = this.data?.focalPointEnabled ?? false;
		this._crops = this.data?.cropOptions ?? [];

		this.#getSrc();
	}

	async #getSrc() {
		console.log('getSrc', this.value);
		const { data } = await this.#urlRepository.requestItems([this._unique]);
		const item = data?.[0];

		if (!item?.url) return;
		const value: UmbImageCropperPropertyEditorValue = {
			...this.value,
			src: item.url,
			crops: this.value.crops?.length ? this.value.crops : this._crops,
			focalPoint: this.value.focalPoint ?? { left: 0.5, top: 0.5 },
		};
		this._imageCropperValue = value;
	}

	#onChange(e: CustomEvent) {
		const value = (e.target as UmbInputImageCropperFieldElement).value;
		if (!value) return;

		this.value = { key: this._key, unique: this._unique, crops: value.crops, focalPoint: value.focalPoint };
	}

	render() {
		return html`
			<umb-body-layout headline=${this.localize.term('defaultdialogs_selectMedia')}>
				<umb-image-cropper-field @change=${this.#onChange} .value=${this._imageCropperValue}></umb-image-cropper-field>
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

	static styles = [
		css`
			uui-tab {
				flex: 1;
				min-height: 100px;
				min-width: 100px;
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
