import { UMB_IMAGE_CROPPER_EDITOR_MODAL } from '../../modals/index.js';
import type { UmbMediaItemModel, UmbCropModel, UmbMediaPickerPropertyValueEntry } from '../../types.js';
import { UMB_MEDIA_ITEM_REPOSITORY_ALIAS } from '../../repository/constants.js';
import { UmbMediaUrlRepository } from '../../url/index.js';
import { UmbMediaPickerInputContext } from '../input-media/input-media.context.js';
import { UmbTemporaryFileConfigRepository } from '@umbraco-cms/backoffice/temporary-file';
import { UmbFileDropzoneItemStatus } from '@umbraco-cms/backoffice/dropzone';
import type { UmbDropzoneChangeEvent } from '@umbraco-cms/backoffice/dropzone';
import { css, customElement, html, nothing, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbSorterController, UmbSorterResolvePlacementAsGrid } from '@umbraco-cms/backoffice/sorter';
import type { UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';
import type { UmbTreeStartNode } from '@umbraco-cms/backoffice/tree';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import { UmbRepositoryItemsManager } from '@umbraco-cms/backoffice/repository';
import { UMB_MEDIA_TYPE_ENTITY_TYPE } from '@umbraco-cms/backoffice/media-type';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

import '@umbraco-cms/backoffice/imaging';
import { UmbEntityInputInteractionMemoryManager } from '@umbraco-cms/backoffice/entity';
import type { UmbInteractionMemoryModel } from '@umbraco-cms/backoffice/interaction-memory';

type UmbRichMediaCardModel = {
	unique: string;
	media: string;
	name: string;
	src?: string;
	icon?: string;
	isTrashed?: boolean;
	isLoading?: boolean;
};

@customElement('umb-input-rich-media')
export class UmbInputRichMediaElement extends UmbFormControlMixin<
	Array<UmbMediaPickerPropertyValueEntry>,
	typeof UmbLitElement,
	undefined
>(UmbLitElement, undefined) {
	#sorter = new UmbSorterController<UmbMediaPickerPropertyValueEntry>(this, {
		getUniqueOfElement: (element) => {
			return element.id;
		},
		getUniqueOfModel: (modelEntry) => {
			return modelEntry.key;
		},
		identifier: 'Umb.SorterIdentifier.InputRichMedia',
		itemSelector: 'uui-card-media',
		containerSelector: '.container',
		resolvePlacement: UmbSorterResolvePlacementAsGrid,
		onChange: ({ model }) => {
			this.value = model;
			this.dispatchEvent(new UmbChangeEvent());
		},
	});

	/**
	 * Sets the input to required, meaning validation will fail if the value is empty.
	 * @type {boolean}
	 */
	@property({ type: Boolean })
	required?: boolean;

	@property({ type: String })
	requiredMessage?: string;

	/**
	 * This is a minimum amount of selected items in this input.
	 * @type {number}
	 * @attr
	 * @default 0
	 */
	@property({ type: Number })
	public min = 0;

	/**
	 * Min validation message.
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: String, attribute: 'min-message' })
	minMessage = 'This field need more items';

	/**
	 * This is a maximum amount of selected items in this input.
	 * @type {number}
	 * @attr
	 * @default Infinity
	 */
	@property({ type: Number })
	public max = Infinity;

	/**
	 * Max validation message.
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: String, attribute: 'min-message' })
	maxMessage = 'This field exceeds the allowed amount of items';

	@property({ type: Array })
	public override set value(value: Array<UmbMediaPickerPropertyValueEntry> | undefined) {
		super.value = value;
		this.#sorter.setModel(value);
		this.#pickerInputContext.setSelection(value?.map((item) => item.mediaKey) ?? []);
		this.#itemManager.setUniques(value?.map((x) => x.mediaKey));
		// Maybe the new value is using an existing media, and there we need to update the cards despite no repository update.
		this.#populateCards();
	}
	public override get value(): Array<UmbMediaPickerPropertyValueEntry> | undefined {
		return super.value;
	}

	@property({ type: Array })
	allowedContentTypeIds?: string[];

	@property({ type: Object, attribute: false })
	startNode?: UmbTreeStartNode;

	@property({ type: Boolean })
	multiple = false;

	@property({ type: Array })
	public preselectedCrops?: Array<UmbCropModel>;

	@property({ type: Boolean })
	public set focalPointEnabled(value: boolean) {
		this.#focalPointEnabled = value;
	}
	public get focalPointEnabled(): boolean {
		return this.#focalPointEnabled;
	}
	#focalPointEnabled: boolean = false;

	@property({ type: String, attribute: 'alt-text-mode', reflect: true })
	public altTextMode: 'off' | 'altText' | 'decorative' = 'off';

	@property({ type: String })
	public variantId: string = 'invariant';

	/** The active workspace culture tab, provided by the property editor when the property is readonly (shared across cultures). */
	@property({ type: String })
	public activeCulture?: string;

	get #activeCulture(): string | null {
		// Only write per-culture alt text when readonly — shared property on a non-default culture tab.
		// For variant properties (readonly=false, each culture owns its full value), altText is the per-culture value.
		if (!this.readonly) return null;
		return this.activeCulture ?? null;
	}

	@property({ type: Boolean })
	public hideZoomCrop: boolean = false;

	@property({ type: Boolean })
	public enableAltTextPerCrop: boolean = false;

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: Boolean, reflect: true })
	public get readonly() {
		return this.#readonly;
	}
	public set readonly(value) {
		this.#readonly = value;

		if (this.#readonly) {
			this.#sorter.disable();
		} else {
			this.#sorter.enable();
		}
	}
	#readonly = false;

	@property({ type: Array, attribute: false })
	public get interactionMemories(): Array<UmbInteractionMemoryModel> | undefined {
		return this.#interactionMemoryManager.getMemories();
	}
	public set interactionMemories(value: Array<UmbInteractionMemoryModel> | undefined) {
		this.#interactionMemoryManager.setMemories(value);
	}

	@state()
	private _cards: Array<UmbRichMediaCardModel> = [];

	@state()
	private _announcement = '';

	@state()
	private _routeBuilder?: UmbModalRouteBuilder;

	readonly #itemManager = new UmbRepositoryItemsManager<UmbMediaItemModel>(this, UMB_MEDIA_ITEM_REPOSITORY_ALIAS);

	readonly #urlRepository = new UmbMediaUrlRepository(this);
	readonly #temporaryFileConfig = new UmbTemporaryFileConfigRepository(this);
	#imageFileTypes?: Array<string>;

	readonly #pickerInputContext = new UmbMediaPickerInputContext(this);
	readonly #interactionMemoryManager = new UmbEntityInputInteractionMemoryManager(
		this,
		this.#pickerInputContext.interactionMemory,
	);

	constructor() {
		super();

		this.observe(this.#itemManager.items, () => {
			this.#populateCards();
		});

		this.#observeImageFileTypes();

		new UmbModalRouteRegistrationController(this, UMB_IMAGE_CROPPER_EDITOR_MODAL)
			.addAdditionalPath(':key')
			.onSetup((params) => {
				const key = params.key;
				if (!key) return false;

				const item = this.value?.find((item) => item.key === key);
				if (!item) return false;

				const culture = this.#activeCulture;
				const resolvedAltText = (culture && item.altTextByCulture?.[culture]) ?? item.altText ?? '';

				// When editing a specific culture, resolve each crop's alt text from altTextByCulture[culture]
				// so the editor sees the culture-specific value rather than the default.
				const crops = culture
					? (item.crops ?? []).map((crop) => ({
							...crop,
							altText: crop.altTextByCulture?.[culture] ?? crop.altText ?? '',
						}))
					: (item.crops ?? []);

				return {
					data: {
						cropOptions: this.preselectedCrops,
						hideFocalPoint: !this.focalPointEnabled,
						hideZoomCrop: this.hideZoomCrop,
						enableAltTextPerCrop: this.enableAltTextPerCrop,
						altTextMode: this.altTextMode,
						key,
						unique: item.mediaKey,
						pickableFilter: this.#pickableFilter,
						culture: culture ?? undefined,
						readonlyMedia: this.readonly,
					},
					value: {
						crops,
						focalPoint: item.focalPoint ?? null,
						src: '',
						key,
						unique: item.mediaKey,
						altText: resolvedAltText,
					},
				};
			})
			.onSubmit((value) => {
				this.value = this.value?.map((item) => {
					if (item.key !== value.key) return item;

					const culture = this.#activeCulture;

					if (culture) {
						const altTextByCulture = { ...(item.altTextByCulture ?? {}), [culture]: value.altText ?? '' };

						// Write each crop's submitted altText into altTextByCulture[culture] on the stored crop,
						// preserving the invariant altText and all other cultures' entries.
						const crops = (item.crops ?? []).map((storedCrop) => {
							const submittedCrop = value.crops?.find((c) => c.alias === storedCrop.alias);
							if (!submittedCrop) return storedCrop;
							const cropAltByCulture = {
								...(storedCrop.altTextByCulture ?? {}),
								[culture]: submittedCrop.altText ?? '',
							};
							return { ...storedCrop, altTextByCulture: cropAltByCulture };
						});

						return { ...item, crops, altTextByCulture };
					}

					const focalPoint = this.focalPointEnabled ? value.focalPoint : null;
					const crops = value.crops;
					const mediaKey = value.unique;

					// Note: If the mediaKey changes we will change the key which causes cards to update
					const key = mediaKey === item.mediaKey ? item.key : UmbId.new();

					return { ...item, crops, mediaKey, focalPoint, key, altText: value.altText };
				});

				this.dispatchEvent(new UmbChangeEvent());
			})
			.observeRouteBuilder((routeBuilder) => {
				this._routeBuilder = routeBuilder;
			});

		this.observe(this.#pickerInputContext.selection, (selection) => {
			this.#addItems(selection);
		});

		this.addValidator(
			'valueMissing',
			() => this.requiredMessage ?? UMB_VALIDATION_EMPTY_LOCALIZATION_KEY,
			() => {
				return !this.readonly && !!this.required && (!this.value || this.value.length === 0);
			},
		);

		this.addValidator(
			'rangeUnderflow',
			() => this.minMessage,
			() =>
				!this.readonly &&
				// Only if min is set:
				!!this.min &&
				// if the value is empty and not required, we should not validate the min:
				!(this.value?.length === 0 && this.required == false) &&
				// Validate the min:
				(this.value?.length ?? 0) < this.min,
		);
		this.addValidator(
			'rangeOverflow',
			() => this.maxMessage,
			() => !this.readonly && !!this.value && !!this.max && this.value?.length > this.max,
		);
	}

	protected override getFormElement() {
		return undefined;
	}

	async #populateCards() {
		const mediaItems = this.#itemManager.getItems();

		this._cards =
			this.value?.map((item) => {
				const media = mediaItems.find((x) => x.unique === item.mediaKey);
				return {
					unique: item.key,
					media: item.mediaKey,
					name: media?.name ?? '',
					icon: media?.mediaType?.icon,
					isTrashed: media?.isTrashed ?? false,
					isLoading: !media,
				};
			}) ?? [];
	}

	#pickableFilter: (item: UmbMediaItemModel) => boolean = (item) => {
		if (this.allowedContentTypeIds && this.allowedContentTypeIds.length > 0) {
			return this.allowedContentTypeIds.includes(item.mediaType.unique);
		}
		return true;
	};

	#addItems(additionalMediaKeys: string[]) {
		// Check that the unique is not already added
		const uniques = additionalMediaKeys.filter((key) => !this.value?.some((item) => item.mediaKey === key));

		if (!uniques.length) return;

		const additions: Array<UmbMediaPickerPropertyValueEntry> = uniques.map((unique) => ({
			key: UmbId.new(),
			mediaKey: unique,
			mediaTypeAlias: '',
			crops: [],
			focalPoint: null,
		}));

		this.value = [...(this.value ?? []), ...additions];
		this.dispatchEvent(new UmbChangeEvent());

		if (uniques.length === 1 && (this.altTextMode === 'altText' || this.enableAltTextPerCrop)) {
			this.#autoOpenCropEditor(additions[0].key, additions[0].mediaKey);
		}
	}

	async #observeImageFileTypes() {
		await this.#temporaryFileConfig.initialized;
		this.observe(
			this.#temporaryFileConfig.part('imageFileTypes'),
			(imageFileTypes) => (this.#imageFileTypes = imageFileTypes),
			'_observeImageFileTypes',
		);
	}

	// The crop and alt text controls only apply to images, so non-image media (e.g. PDFs) must not auto-open the editor.
	async #isImage(mediaKey: string): Promise<boolean> {
		const { data } = await this.#urlRepository.requestItems([mediaKey]);
		const item = data?.[0];
		if (!item?.url || !item.extension) return false;

		await this.#temporaryFileConfig.initialized;
		return this.#imageFileTypes?.includes(item.extension) ?? false;
	}

	async #autoOpenCropEditor(key: string, mediaKey: string) {
		if (!(await this.#isImage(mediaKey))) return;

		await this.updateComplete;
		const href = this._routeBuilder?.({ key });
		if (!href) return;
		window.history.pushState({}, '', href);
	}

	#openPicker() {
		this.#pickerInputContext.openPicker(
			{
				multiple: this.multiple,
				startNode: this.startNode,
				pickableFilter: this.#pickableFilter,
			},
			{
				allowedContentTypes: this.allowedContentTypeIds?.map((id) => ({
					unique: id,
					entityType: UMB_MEDIA_TYPE_ENTITY_TYPE,
				})),
				includeTrashed: false,
			},
		);
	}

	async #onRemove(item: UmbRichMediaCardModel) {
		try {
			await this.#pickerInputContext.requestRemoveItem(item.media);
			this.value = this.value?.filter((x) => x.key !== item.unique);
			this.dispatchEvent(new UmbChangeEvent());
			this._announcement = this.localize.term('mediaPicker_itemRemoved', [item.name]);
		} catch {
			// User cancelled the action
		}
	}

	async #onUploadCompleted(e: UmbDropzoneChangeEvent) {
		if (this.readonly) return;

		// If there are any finished uploadable items, we need to add them to the value
		const uploaded = e.items
			.filter((file) => file.status === UmbFileDropzoneItemStatus.COMPLETE)
			.map((file) => file.unique);
		this.#addItems(uploaded);
	}

	override render() {
		return html`
			<div class="sr-only" role="status" aria-live="polite" aria-atomic="true">${this._announcement}</div>
			${this.#renderDropzone()}
			<div class="container">${this.#renderItems()} ${this.#renderAddButton()}</div>
		`;
	}

	// TODO: Consider removing the "progress element" from the dropzone and render that using a context instead. This would allow the media picker to show inline progress items instead [JOV]
	#renderDropzone() {
		if (this.readonly) return nothing;
		return html`<umb-dropzone-media
			id="dropzone"
			?multiple=${this.multiple}
			.parentUnique=${this.startNode?.unique ?? null}
			@change=${this.#onUploadCompleted}></umb-dropzone-media>`;
	}

	#renderItems() {
		if (!this._cards.length) return nothing;
		return html`
			${repeat(
				this._cards,
				(item) => item.unique,
				(item) => this.#renderItem(item),
			)}
		`;
	}

	#renderAddButton() {
		if (this.readonly) return nothing;
		if (this.max === 1 && this._cards.length > 0) return nothing;
		return html`
			<uui-button
				id="btn-add"
				look="placeholder"
				@blur=${() => {
					this.pristine = false;
					this.checkValidity();
				}}
				@click=${this.#openPicker}
				label=${this.localize.term('general_choose')}
				?disabled=${this.readonly}>
				<uui-icon name="icon-add"></uui-icon>
				${this.localize.term('general_choose')}
			</uui-button>
		`;
	}

	#renderItem(item: UmbRichMediaCardModel) {
		if (!item.unique) return nothing;
		const canOpenModal = !this.readonly || this.altTextMode !== 'off' || this.enableAltTextPerCrop;
		const href = canOpenModal ? this._routeBuilder?.({ key: item.unique }) : undefined;

		return html`
			<div class="media-item-wrapper">
				<uui-card-media id=${item.unique} title=${item.name} name=${item.name} .href=${href} ?readonly=${this.readonly}>
					<umb-media-thumbnail
						.unique=${item.media}
						.alt=${item.name}
						.icon=${item.icon ?? 'icon-picture'}
						.externalLoading=${item.isLoading ?? false}></umb-media-thumbnail>

					${this.#renderIsTrashed(item)} ${this.#renderActions(item)}
				</uui-card-media>
				${this.altTextMode === 'decorative' ? this.#renderDecorativeMessage() : nothing}
			</div>
		`;
	}

	#renderDecorativeMessage() {
		return html`
			<p class="decorative-message">
				<umb-localize key="mediaPicker_decorativeImageMessage">This image is decorative.</umb-localize>
			</p>
		`;
	}

	#renderActions(item: UmbRichMediaCardModel) {
		if (this.readonly) return nothing;
		const index = this._cards.findIndex((c) => c.unique === item.unique);
		const isFirst = index === 0;
		const isLast = index === this._cards.length - 1;
		const showMoveButtons = this._cards.length > 1;
		return html`
			<uui-action-bar slot="actions">
				${showMoveButtons
					? html`
							<uui-button
								label=${this.localize.term(
									'general_reorderMoveUpPosition',
									String(index + 1),
									String(this._cards.length),
								)}
								look="secondary"
								?disabled=${isFirst}
								@click=${() => this.#moveItem(index, -1)}>
								<uui-icon name="icon-arrow-up"></uui-icon>
							</uui-button>
							<uui-button
								label=${this.localize.term(
									'general_reorderMoveDownPosition',
									String(index + 1),
									String(this._cards.length),
								)}
								look="secondary"
								?disabled=${isLast}
								@click=${() => this.#moveItem(index, 1)}>
								<uui-icon name="icon-arrow-down"></uui-icon>
							</uui-button>
						`
					: nothing}
				<uui-button label=${this.localize.term('general_remove')} look="secondary" @click=${() => this.#onRemove(item)}>
					<uui-icon name="icon-trash"></uui-icon>
				</uui-button>
			</uui-action-bar>
		`;
	}

	async #moveItem(fromIndex: number, direction: -1 | 1) {
		if (!this.value) return;
		const toIndex = fromIndex + direction;
		if (toIndex < 0 || toIndex >= this.value.length) return;

		// Capture before mutation — value setter rebuilds _cards synchronously,
		// so reading _cards[fromIndex] after the swap gives the wrong item.
		const movedCard = this._cards[fromIndex];

		const updated = [...this.value];
		[updated[fromIndex], updated[toIndex]] = [updated[toIndex], updated[fromIndex]];
		this.value = updated;
		this.#sorter.setModel(updated);
		this.dispatchEvent(new UmbChangeEvent());
		this._announcement = this.localize.term(
			direction === -1 ? 'mediaPicker_movedUp' : 'mediaPicker_movedDown',
			movedCard.name,
			String(toIndex + 1),
			String(this.value?.length ?? 0),
		);

		await this.updateComplete;
		this.#restoreFocusAfterMove(movedCard.unique, toIndex, direction);
	}

	#restoreFocusAfterMove(movedItemKey: string, newIndex: number, direction: -1 | 1) {
		const card = this.shadowRoot?.querySelector(`[id="${movedItemKey}"]`);
		if (!card) return;

		// Buttons order within uui-action-bar: [0] move-up, [1] move-down, [2] remove
		const buttons = card.querySelectorAll<HTMLElement>('uui-action-bar uui-button');
		if (buttons.length < 2) return;

		const isNowFirst = newIndex === 0;
		const isNowLast = newIndex === (this.value?.length ?? 0) - 1;

		// Keep focus on the same direction button unless it is now disabled (edge position),
		// in which case move focus to the opposing direction button.
		const targetButton =
			direction === -1 ? (isNowFirst ? buttons[1] : buttons[0]) : isNowLast ? buttons[0] : buttons[1];

		targetButton?.focus();
	}

	#renderIsTrashed(item: UmbRichMediaCardModel) {
		if (!item.isTrashed) return;
		return html`
			<uui-tag size="s" slot="tag" color="danger">
				<umb-localize key="mediaPicker_trashed">Trashed</umb-localize>
			</uui-tag>
		`;
	}

	static override readonly styles = [
		UmbTextStyles,
		css`
			:host {
				position: relative;
				width: 100%;
				display: flex;
				flex-direction: column-reverse;
			}
			.container {
				display: grid;
				gap: var(--uui-size-space-5);
				grid-template-columns: repeat(auto-fill, minmax(var(--umb-card-medium-min-width), 1fr));
				grid-auto-rows: var(--umb-card-medium-min-width);
			}

			:host([alt-text-mode='decorative']) .container {
				grid-auto-rows: auto;
			}

			.media-item-wrapper {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-3);
			}

			.media-item-wrapper uui-card-media {
				min-height: var(--umb-card-medium-min-width);
			}

			.decorative-message {
				font-size: var(--uui-type-small-size);
				color: var(--uui-color-text-alt);
				margin: 0;
			}

			#dropzone {
				margin-bottom: var(--uui-size-space-5);
			}

			#btn-add {
				text-align: center;
				height: 100%;
			}

			uui-icon {
				display: block;
				margin: 0 auto;
			}

			uui-card-media umb-icon {
				font-size: var(--uui-size-8);
			}

			uui-card-media[drag-placeholder] {
				opacity: 0.2;
			}
			img {
				background-image: url('data:image/svg+xml;charset=utf-8,<svg xmlns="http://www.w3.org/2000/svg" width="100" height="100" fill-opacity=".1"><path d="M50 0h50v50H50zM0 50h50v50H0z"/></svg>');
				background-size: 10px 10px;
				background-repeat: repeat;
			}
		`,
	];
}

export default UmbInputRichMediaElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-rich-media': UmbInputRichMediaElement;
	}
}
