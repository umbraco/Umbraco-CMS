import type { UmbCropModel, UmbMediaPickerPropertyValue } from '../../property-editors/index.js';
import { UMB_IMAGE_CROPPER_EDITOR_MODAL, type UmbMediaCardItemModel } from '../../modals/index.js';
import { UmbRichMediaPickerContext } from './input-rich-media.context.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { customElement, html, ifDefined, property, state } from '@umbraco-cms/backoffice/external/lit';
import {
	UmbInputMediaElement,
	type UmbRichMediaItemModel,
	type UmbUploadableFileModel,
} from '@umbraco-cms/backoffice/media';
import { type UmbModalRouteBuilder, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

const elementName = 'umb-input-rich-media';

@customElement(elementName)
export class UmbInputRichMediaElement extends UmbInputMediaElement {
	#modal: UmbModalRouteRegistrationController;
	#pickerContext = new UmbRichMediaPickerContext(this);

	@state()
	private _items: Array<UmbRichMediaItemModel> = [];

	@property({ type: Array })
	public set richValue(value: Array<UmbMediaPickerPropertyValue>) {
		this.#pickerContext.setSelection(value);
	}
	public get richValue(): Array<UmbMediaPickerPropertyValue> {
		return this.#pickerContext.getSelection();
	}

	@property({ type: Array })
	public set preselectedCrops(crops: Array<UmbCropModel>) {
		this.#pickerContext.setPreselectedCrops(crops);
	}
	public get preselectedCrops(): Array<UmbCropModel> {
		return this.#pickerContext.getPreselectedCrops();
	}

	@property({ type: Boolean })
	public set focalPointEnabled(enabled: boolean) {
		this.#pickerContext.setFocalPointEnabled(enabled);
	}
	public get focalPointEnabled(): boolean {
		return this.#pickerContext.getFocalPointEnabled();
	}

	@property()
	public set alias(value: string | undefined) {
		this.#modal.setUniquePathValue('propertyAlias', value);
	}
	public get alias(): string | undefined {
		return this.#modal.getUniquePathValue('propertyAlias');
	}

	@property()
	public set variantId(value: string | UmbVariantId | undefined) {
		this.#modal.setUniquePathValue('variantId', value?.toString());
	}
	public get variantId(): string | undefined {
		return this.#modal.getUniquePathValue('variantId');
	}

	@state()
	private _modalRoute?: UmbModalRouteBuilder;
	constructor() {
		super();
		this.#modal = new UmbModalRouteRegistrationController(this, UMB_IMAGE_CROPPER_EDITOR_MODAL)
			.addAdditionalPath(`:index`)
			.addUniquePaths(['propertyAlias', 'variantId'])
			.onSetup((params) => {
				const indexParam = params.index;
				if (!indexParam) return false;
				const index: number | null = parseInt(params.index);
				if (Number.isNaN(index)) return false;

				// Use the index to find the item:
				const unique = this.selection[index];

				return {
					data: { cropOptions: this.preselectedCrops, focalPointEnabled: this.focalPointEnabled, unique },
					value: { crops: [], focalPoint: { left: 0.5, top: 0.5 }, src: '', unique: unique },
				};
			})
			.onSubmit((value) => {
				this.#pickerContext.updateFocalPointOf(value.unique, value.focalPoint);
				this.#pickerContext.updateCropsOf(value.unique, value.crops);
				this.dispatchEvent(new UmbChangeEvent());
			})
			.observeRouteBuilder((routeBuilder) => {
				this._modalRoute = routeBuilder;
			});

		this.pickerContextObservers();
		this.addValidators();
	}

	connectedCallback(): void {
		super.connectedCallback();
	}

	async #onUploadCompleted(e: CustomEvent) {
		const completed = e.detail?.completed as Array<UmbUploadableFileModel>;
		const uploaded = completed.map((file) => file.unique);

		this.selection = [...this.selection, ...uploaded];
		this.dispatchEvent(new UmbChangeEvent());
	}

	render() {
		return html`${this.#renderDropzone()} ${super.render()}`;
	}

	#renderDropzone() {
		if (this.items && this.items.length >= this.max) return;
		return html`<umb-dropzone @change=${this.#onUploadCompleted}></umb-dropzone>`;
	}

	protected renderItem(item: UmbMediaCardItemModel, index: number) {
		const href = this._modalRoute?.({ index });
		return html`
			<uui-card-media
				name=${ifDefined(item.name === null ? undefined : item.name)}
				detail=${ifDefined(item.unique)}
				.href=${href}>
				${item.src
					? html`<img src=${item.src} alt=${item.name} />`
					: html`<umb-icon name=${ifDefined(item.mediaType.icon)}></umb-icon>`}
				${this.renderIsTrashed(item)}
				<uui-action-bar slot="actions">
					<uui-button label="Copy media" look="secondary">
						<uui-icon name="icon-documents"></uui-icon>
					</uui-button>
					<uui-button
						label=${this.localize.term('general_remove')}
						look="secondary"
						@click=${() => this.onRemove(item)}>
						<uui-icon name="icon-trash"></uui-icon>
					</uui-button>
				</uui-action-bar>
			</uui-card-media>
		`;
	}
}

export { UmbInputRichMediaElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbInputRichMediaElement;
	}
}
