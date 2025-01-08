import { UmbMediaItemRepository } from '../../repository/index.js';
import { UMB_IMAGE_CROPPER_EDITOR_MODAL, UMB_MEDIA_PICKER_MODAL } from '../../modals/index.js';
import type { UmbMediaItemModel, UmbCropModel, UmbMediaPickerPropertyValue } from '../../types.js';
import type { UmbUploadableItem } from '../../dropzone/types.js';
import { css, customElement, html, nothing, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { umbConfirmModal, UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import type { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import type { UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

import '@umbraco-cms/backoffice/imaging';

type UmbRichMediaCardModel = {
	unique: string;
	media: string;
	name: string;
	src?: string;
	icon?: string;
	isTrashed?: boolean;
};

const elementName = 'umb-input-rich-media';

@customElement(elementName)
export class UmbInputRichMediaElement extends UUIFormControlMixin(UmbLitElement, '') {
	#sorter = new UmbSorterController<UmbMediaPickerPropertyValue>(this, {
		getUniqueOfElement: (element) => {
			return element.id;
		},
		getUniqueOfModel: (modelEntry) => {
			return modelEntry.key;
		},
		identifier: 'Umb.SorterIdentifier.InputRichMedia',
		itemSelector: 'uui-card-media',
		containerSelector: '.container',
		// TODO: This component probably needs some grid-like logic for resolve placement... [LI]
		// TODO: You can also use verticalDirection? [NL]
		resolvePlacement: () => false,
		onChange: ({ model }) => {
			this.#items = model;
			this.#sortCards(model);
			this.dispatchEvent(new UmbChangeEvent());
		},
	});

	#sortCards(model: Array<UmbMediaPickerPropertyValue>) {
		const idToIndexMap: { [unique: string]: number } = {};
		model.forEach((item, index) => {
			idToIndexMap[item.key] = index;
		});

		const cards = [...this._cards];
		this._cards = cards.sort((a, b) => idToIndexMap[a.unique] - idToIndexMap[b.unique]);
	}

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
	public set items(value: Array<UmbMediaPickerPropertyValue>) {
		this.#sorter.setModel(value);
		this.#items = value;
		this.#populateCards();
	}
	public get items(): Array<UmbMediaPickerPropertyValue> {
		return this.#items;
	}
	#items: Array<UmbMediaPickerPropertyValue> = [];

	@property({ type: Array })
	allowedContentTypeIds?: string[] | undefined;

	@property({ type: String })
	startNode = '';

	@property({ type: Boolean })
	multiple = false;

	@property()
	public override get value() {
		return this.items?.map((item) => item.mediaKey).join(',');
	}

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

	@property()
	public set alias(value: string | undefined) {
		this.#modalRouter.setUniquePathValue('propertyAlias', value);
	}
	public get alias(): string | undefined {
		return this.#modalRouter.getUniquePathValue('propertyAlias');
	}

	@property()
	public set variantId(value: string | UmbVariantId | undefined) {
		this.#modalRouter.setUniquePathValue('variantId', value?.toString());
	}
	public get variantId(): string | undefined {
		return this.#modalRouter.getUniquePathValue('variantId');
	}

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

	@state()
	private _cards: Array<UmbRichMediaCardModel> = [];

	@state()
	private _routeBuilder?: UmbModalRouteBuilder;

	#itemRepository = new UmbMediaItemRepository(this);

	#modalRouter;
	#modalManager?: UmbModalManagerContext;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalManager = instance;
		});

		this.#modalRouter = new UmbModalRouteRegistrationController(this, UMB_IMAGE_CROPPER_EDITOR_MODAL)
			.addAdditionalPath(':key')
			.addUniquePaths(['propertyAlias', 'variantId'])
			.onSetup((params) => {
				const key = params.key;
				if (!key) return false;

				const item = this.items.find((item) => item.key === key);
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
				this.items = this.items.map((item) => {
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

		this.addValidator(
			'rangeUnderflow',
			() => this.minMessage,
			() => !!this.min && this.items?.length < this.min,
		);
		this.addValidator(
			'rangeOverflow',
			() => this.maxMessage,
			() => !!this.max && this.items?.length > this.max,
		);
	}

	protected override getFormElement() {
		return undefined;
	}

	async #populateCards() {
		const missingCards = this.items.filter((item) => !this._cards.find((card) => card.unique === item.key));
		if (!missingCards.length) return;

		if (!this.items?.length) {
			this._cards = [];
			return;
		}

		const uniques = this.items.map((item) => item.mediaKey);

		const { data: items } = await this.#itemRepository.requestItems(uniques);

		this._cards = this.items.map((item) => {
			const media = items?.find((x) => x.unique === item.mediaKey);
			return {
				unique: item.key,
				media: item.mediaKey,
				name: media?.name ?? '',
				icon: media?.mediaType?.icon,
				isTrashed: media?.isTrashed ?? false,
			};
		});
	}

	#pickableFilter: (item: UmbMediaItemModel) => boolean = (item) => {
		if (this.allowedContentTypeIds && this.allowedContentTypeIds.length > 0) {
			return this.allowedContentTypeIds.includes(item.mediaType.unique);
		}
		return true;
	};

	#addItems(uniques: string[]) {
		if (!uniques.length) return;

		const additions: Array<UmbMediaPickerPropertyValue> = uniques.map((unique) => ({
			key: UmbId.new(),
			mediaKey: unique,
			mediaTypeAlias: '',
			crops: [],
			focalPoint: null,
		}));

		this.#items = [...this.#items, ...additions];
		this.dispatchEvent(new UmbChangeEvent());
	}

	async #openPicker() {
		const modalHandler = this.#modalManager?.open(this, UMB_MEDIA_PICKER_MODAL, {
			data: {
				multiple: this.multiple,
				startNode: this.startNode,
				pickableFilter: this.#pickableFilter,
			},
			value: { selection: [] },
		});

		const data = await modalHandler?.onSubmit().catch(() => null);
		if (!data) return;

		const selection = data.selection;
		this.#addItems(selection);
	}

	async #onRemove(item: UmbRichMediaCardModel) {
		await umbConfirmModal(this, {
			color: 'danger',
			headline: `${this.localize.term('actions_remove')} ${item.name}?`,
			content: `${this.localize.term('defaultdialogs_confirmremove')} ${item.name}?`,
			confirmLabel: this.localize.term('actions_remove'),
		});

		this.#items = this.#items.filter((x) => x.key !== item.unique);
		this._cards = this._cards.filter((x) => x.unique !== item.unique);

		this.dispatchEvent(new UmbChangeEvent());
	}

	async #onUploadCompleted(e: CustomEvent) {
		const completed = e.detail as Array<UmbUploadableItem>;
		const uploaded = completed.map((file) => file.unique);
		this.#addItems(uploaded);
	}

	override render() {
		return html`
			${this.#renderDropzone()}
			<div class="container">${this.#renderItems()} ${this.#renderAddButton()}</div>
		`;
	}

	#renderDropzone() {
		if (this.readonly) return nothing;
		if (this._cards && this._cards.length >= this.max) return;
		return html`<umb-dropzone ?multiple=${this.max > 1} @complete=${this.#onUploadCompleted}></umb-dropzone>`;
	}

	#renderItems() {
		if (!this._cards.length) return;
		return html`
			${repeat(
				this._cards,
				(item) => item.unique,
				(item) => this.#renderItem(item),
			)}
		`;
	}

	#renderAddButton() {
		// TODO: Stop preventing adding more, instead implement proper validation for user feedback. [NL]
		if ((this._cards && this.max && this._cards.length >= this.max) || (this._cards.length && !this.multiple)) return;
		if (this.readonly && this._cards.length > 0) {
			return nothing;
		} else {
			return html`
				<uui-button
					id="btn-add"
					look="placeholder"
					@click=${this.#openPicker}
					label=${this.localize.term('general_choose')}
					?disabled=${this.readonly}>
					<uui-icon name="icon-add"></uui-icon>
					${this.localize.term('general_choose')}
				</uui-button>
			`;
		}
	}

	#renderItem(item: UmbRichMediaCardModel) {
		if (!item.unique) return nothing;
		const href = this.readonly ? undefined : this._routeBuilder?.({ key: item.unique });
		return html`
			<uui-card-media id=${item.unique} name=${item.name} .href=${href} ?readonly=${this.readonly}>
				<umb-imaging-thumbnail
					unique=${item.media}
					alt=${item.name}
					icon=${item.icon ?? 'icon-picture'}></umb-imaging-thumbnail>
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

	static override styles = [
		css`
			:host {
				position: relative;
			}
			.container {
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
				grid-auto-rows: 150px;
				gap: var(--uui-size-space-5);
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
		[elementName]: UmbInputRichMediaElement;
	}
}
