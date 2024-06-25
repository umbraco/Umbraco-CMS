import type { UmbMediaCardItemModel } from '../../modals/index.js';
import type { UmbMediaItemModel } from '../../repository/index.js';
import { UmbMediaPickerContext } from './input-media.context.js';
import { UmbImagingRepository } from '@umbraco-cms/backoffice/imaging';
import { css, customElement, html, ifDefined, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/modal';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';

const elementName = 'umb-input-media';

@customElement(elementName)
export class UmbInputMediaElement extends UUIFormControlMixin(UmbLitElement, '') {
	#sorter = new UmbSorterController<string>(this, {
		getUniqueOfElement: (element) => {
			return element.getAttribute('detail');
		},
		getUniqueOfModel: (modelEntry) => {
			return modelEntry;
		},
		identifier: 'Umb.SorterIdentifier.InputMedia',
		itemSelector: 'uui-card-media',
		containerSelector: '.container',
		/** TODO: This component probably needs some grid-like logic for resolve placement... [LI] */
		resolvePlacement: () => false,
		onChange: ({ model }) => {
			this.selection = model;
			this.#sortCards(model);
			this.dispatchEvent(new UmbChangeEvent());
		},
	});

	#sortCards(model: Array<string>) {
		const idToIndexMap: { [unique: string]: number } = {};
		model.forEach((item, index) => {
			idToIndexMap[item] = index;
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
	public set min(value: number) {
		this.#pickerContext.min = value;
	}
	public get min(): number {
		return this.#pickerContext.min;
	}

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
	public set max(value: number) {
		this.#pickerContext.max = value;
	}
	public get max(): number {
		return this.#pickerContext.max;
	}

	/**
	 * Max validation message.
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: String, attribute: 'min-message' })
	maxMessage = 'This field exceeds the allowed amount of items';

	public set selection(ids: Array<string>) {
		this.#pickerContext.setSelection(ids);
		this.#sorter.setModel(ids);
	}
	public get selection(): Array<string> {
		return this.#pickerContext.getSelection();
	}

	@property({ type: Array })
	allowedContentTypeIds?: string[] | undefined;

	@property({ type: Boolean })
	showOpenButton?: boolean;

	@property({ type: String })
	startNode = '';

	@property()
	public override set value(idsString: string) {
		this.selection = splitStringToArray(idsString);
	}
	public override get value() {
		return this.selection.join(',');
	}

	@state()
	private _editMediaPath = '';

	@state()
	private _cards: Array<UmbMediaCardItemModel> = [];

	#pickerContext = new UmbMediaPickerContext(this);

	#imagingRepository = new UmbImagingRepository(this);

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('media')
			.onSetup(() => {
				return { data: { entityType: 'media', preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editMediaPath = routeBuilder({});
			});

		this.observe(this.#pickerContext.selection, (selection) => (this.value = selection.join(',')));

		this.observe(this.#pickerContext.selectedItems, async (selectedItems) => {
			const missingCards = selectedItems.filter((item) => !this._cards.find((card) => card.unique === item.unique));
			if (selectedItems?.length && !missingCards.length) return;

			if (!selectedItems?.length) {
				this._cards = [];
				return;
			}

			const uniques = selectedItems.map((x) => x.unique);

			const { data: thumbnails } = await this.#imagingRepository.requestThumbnailUrls(uniques, 400, 400);

			this._cards = selectedItems.map((item) => {
				const thumbnail = thumbnails?.find((x) => x.unique === item.unique);
				return {
					...item,
					src: thumbnail?.url,
				};
			});
		});

		this.addValidator(
			'rangeUnderflow',
			() => this.minMessage,
			() => !!this.min && this.#pickerContext.getSelection().length < this.min,
		);
		this.addValidator(
			'rangeOverflow',
			() => this.maxMessage,
			() => !!this.max && this.#pickerContext.getSelection().length > this.max,
		);
	}

	protected getFormElement() {
		return undefined;
	}

	#pickableFilter: (item: UmbMediaItemModel) => boolean = (item) => {
		if (this.allowedContentTypeIds && this.allowedContentTypeIds.length > 0) {
			return this.allowedContentTypeIds.includes(item.mediaType.unique);
		}
		return true;
	};

	#openPicker() {
		this.#pickerContext.openPicker({
			multiple: this.max > 1,
			startNode: this.startNode,
			pickableFilter: this.#pickableFilter,
		});
	}

	#onRemove(item: UmbMediaCardItemModel) {
		this.#pickerContext.requestRemoveItem(item.unique);
	}

	override render() {
		return html`<div class="container">${this.#renderItems()} ${this.#renderAddButton()}</div>`;
	}

	#renderItems() {
		if (!this._cards?.length) return;
		return html`
			${repeat(
				this._cards,
				(item) => item.unique,
				(item) => this.#renderItem(item),
			)}
		`;
	}

	#renderAddButton() {
		if (this._cards && this.max && this._cards.length >= this.max) return;
		return html`
			<uui-button
				id="btn-add"
				look="placeholder"
				@click=${this.#openPicker}
				label=${this.localize.term('general_choose')}>
				<uui-icon name="icon-add"></uui-icon>
				${this.localize.term('general_choose')}
			</uui-button>
		`;
	}

	#renderItem(item: UmbMediaCardItemModel) {
		return html`
			<uui-card-media
				name=${ifDefined(item.name === null ? undefined : item.name)}
				detail=${ifDefined(item.unique)}
				href="${this._editMediaPath}edit/${item.unique}">
				${item.src
					? html`<img src=${item.src} alt=${item.name} />`
					: html`<umb-icon name=${ifDefined(item.mediaType.icon)}></umb-icon>`}
				${this.#renderIsTrashed(item)}
				<uui-action-bar slot="actions">
					<uui-button
						label=${this.localize.term('general_remove')}
						look="secondary"
						@click=${() => this.#onRemove(item)}>
						<uui-icon name="icon-trash"></uui-icon>
					</uui-button>
				</uui-action-bar>
			</uui-card-media>
		`;
	}

	#renderIsTrashed(item: UmbMediaCardItemModel) {
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

export { UmbInputMediaElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbInputMediaElement;
	}
}
