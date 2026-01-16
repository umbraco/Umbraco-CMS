import { UMB_IMAGE_CROPPER_EDITOR_MODAL } from '../../modals/index.js';
import type { UmbMediaItemModel, UmbCropModel, UmbMediaPickerPropertyValueEntry } from '../../types.js';
import { UMB_MEDIA_ITEM_REPOSITORY_ALIAS } from '../../repository/constants.js';
import { UmbMediaPickerInputContext } from '../input-media/input-media.context.js';
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

import '@umbraco-cms/backoffice/imaging';
import {
	UmbInteractionMemoriesChangeEvent,
	type UmbInteractionMemoryModel,
} from '@umbraco-cms/backoffice/interaction-memory';
import { jsonStringComparison } from '@umbraco-cms/backoffice/observable-api';

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
	allowedContentTypeIds?: string[] | undefined;

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
		return this.#pickerInputContext.interactionMemory.getAllMemories();
	}
	public set interactionMemories(value: Array<UmbInteractionMemoryModel> | undefined) {
		this.#interactionMemories = value;
		value?.forEach((memory) => this.#pickerInputContext.interactionMemory.setMemory(memory));
	}

	#interactionMemories?: Array<UmbInteractionMemoryModel> = [];

	@state()
	private _cards: Array<UmbRichMediaCardModel> = [];

	@state()
	private _routeBuilder?: UmbModalRouteBuilder;

	readonly #itemManager = new UmbRepositoryItemsManager<UmbMediaItemModel>(this, UMB_MEDIA_ITEM_REPOSITORY_ALIAS);

	readonly #pickerInputContext = new UmbMediaPickerInputContext(this);

	constructor() {
		super();

		this.observe(this.#itemManager.items, () => {
			this.#populateCards();
		});

		new UmbModalRouteRegistrationController(this, UMB_IMAGE_CROPPER_EDITOR_MODAL)
			.addAdditionalPath(':key')
			.onSetup((params) => {
				const key = params.key;
				if (!key) return false;

				const item = this.value?.find((item) => item.key === key);
				if (!item) return false;

				return {
					data: {
						cropOptions: this.preselectedCrops,
						hideFocalPoint: !this.focalPointEnabled,
						key,
						unique: item.mediaKey,
						pickableFilter: this.#pickableFilter,
					},
					value: {
						crops: item.crops ?? [],
						focalPoint: item.focalPoint ?? { left: 0.5, top: 0.5 },
						src: '',
						key,
						unique: item.mediaKey,
					},
				};
			})
			.onSubmit((value) => {
				this.value = this.value?.map((item) => {
					if (item.key !== value.key) return item;

					const focalPoint = this.focalPointEnabled ? value.focalPoint : null;
					const crops = value.crops;
					const mediaKey = value.unique;

					// Note: If the mediaKey changes we will change the key which causes cards to update
					const key = mediaKey === item.mediaKey ? item.key : UmbId.new();

					return { ...item, crops, mediaKey, focalPoint, key };
				});

				this.dispatchEvent(new UmbChangeEvent());
			})
			.observeRouteBuilder((routeBuilder) => {
				this._routeBuilder = routeBuilder;
			});

		this.observe(this.#pickerInputContext.selection, (selection) => {
			this.#addItems(selection);
		});

		this.observe(
			this.#pickerInputContext.interactionMemory.memories,
			(memories) => {
				// only dispatch the event if the interaction memories have actually changed
				const isIdentical = jsonStringComparison(memories, this.#interactionMemories);

				if (!isIdentical) {
					this.#interactionMemories = memories;
					this.dispatchEvent(new UmbInteractionMemoriesChangeEvent());
				}
			},
			'_observeMemories',
		);

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
		const href = this.readonly ? undefined : this._routeBuilder?.({ key: item.unique });

		return html`
			<uui-card-media id=${item.unique} name=${item.name} .href=${href} ?readonly=${this.readonly}>
				<umb-imaging-thumbnail
					.unique=${item.media}
					.alt=${item.name}
					.icon=${item.icon ?? 'icon-picture'}
					.externalLoading=${item.isLoading ?? false}></umb-imaging-thumbnail>

				${this.#renderIsTrashed(item)} ${this.#renderActions(item)}
			</uui-card-media>
		`;
	}

	#renderActions(item: UmbRichMediaCardModel) {
		if (this.readonly) return nothing;
		return html`
			<uui-action-bar slot="actions">
				<uui-button label=${this.localize.term('general_remove')} look="secondary" @click=${() => this.#onRemove(item)}>
					<uui-icon name="icon-trash"></uui-icon>
				</uui-button>
			</uui-action-bar>
		`;
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
		css`
			:host {
				position: relative;
				display: block;
				width: 100%;
			}
			.container {
				display: grid;
				gap: var(--uui-size-space-5);
				grid-template-columns: repeat(auto-fill, minmax(var(--umb-card-medium-min-width), 1fr));
				grid-auto-rows: var(--umb-card-medium-min-width);
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
