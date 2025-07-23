import type { UmbMediaCardItemModel } from '../../types.js';
import { UmbMediaPickerInputContext } from './input-media.context.js';
import {
	css,
	customElement,
	html,
	ifDefined,
	nothing,
	property,
	repeat,
	state,
} from '@umbraco-cms/backoffice/external/lit';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbSorterController, UmbSorterResolvePlacementAsGrid } from '@umbraco-cms/backoffice/sorter';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import type { UmbTreeStartNode } from '@umbraco-cms/backoffice/tree';
import { UMB_MEDIA_TYPE_ENTITY_TYPE } from '@umbraco-cms/backoffice/media-type';

import '@umbraco-cms/backoffice/imaging';

const elementName = 'umb-input-media';
@customElement(elementName)
export class UmbInputMediaElement extends UmbFormControlMixin<string | undefined, typeof UmbLitElement>(UmbLitElement) {
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
		resolvePlacement: UmbSorterResolvePlacementAsGrid,
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
	 * @default
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
	 * @default
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
	@property({ type: String, attribute: 'max-message' })
	maxMessage = 'This field exceeds the allowed amount of items';

	public set selection(ids: Array<string>) {
		this.#pickerContext.setSelection(ids);
		this.#sorter.setModel(ids);
	}
	public get selection(): Array<string> {
		return this.#pickerContext.getSelection();
	}

	@property({ type: Array })
	allowedContentTypeIds?: Array<string> | undefined;

	@property({ type: Boolean, attribute: 'include-trashed' })
	includeTrashed = false;

	@property({ type: Object, attribute: false })
	startNode?: UmbTreeStartNode;

	@property({ type: String })
	public override set value(selectionString: string | undefined) {
		this.selection = splitStringToArray(selectionString);
	}
	public override get value(): string | undefined {
		return this.selection.length > 0 ? this.selection.join(',') : undefined;
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
	private _editMediaPath = '';

	@state()
	private _cards: Array<UmbMediaCardItemModel> = [];

	#pickerContext = new UmbMediaPickerInputContext(this);

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

			this._cards = selectedItems ?? [];
		});

		this.addValidator(
			'rangeUnderflow',
			() => this.minMessage,
			() => !!this.min && this.selection.length < this.min,
		);
		this.addValidator(
			'rangeOverflow',
			() => this.maxMessage,
			() => !!this.max && this.selection.length > this.max,
		);
	}

	#openPicker() {
		this.#pickerContext.openPicker(
			{
				multiple: this.max > 1,
				startNode: this.startNode,
			},
			{
				allowedContentTypes: this.allowedContentTypeIds?.map((id) => ({
					unique: id,
					entityType: UMB_MEDIA_TYPE_ENTITY_TYPE,
				})),
				includeTrashed: this.includeTrashed,
			},
		);
	}

	async #onRemove(item: UmbMediaCardItemModel) {
		await this.#pickerContext.requestRemoveItem(item.unique);
		this._cards = this._cards.filter((x) => x.unique !== item.unique);
	}

	override render() {
		return html`<div class="container">${this.#renderItems()} ${this.#renderAddButton()}</div>`;
	}

	#renderItems() {
		if (!this._cards?.length) return nothing;
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
		if (this._cards && this.max && this._cards.length >= this.max) return nothing;
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

	#renderItem(item: UmbMediaCardItemModel) {
		const href = this.readonly ? undefined : `${this._editMediaPath}edit/${item.unique}`;
		return html`
			<uui-card-media
				name=${ifDefined(item.name === null ? undefined : item.name)}
				data-mark="${item.entityType}:${item.unique}"
				href="${ifDefined(href)}"
				?readonly=${this.readonly}>
				<umb-imaging-thumbnail
					unique=${item.unique}
					alt=${item.name}
					icon=${item.mediaType.icon}></umb-imaging-thumbnail>
				${this.#renderIsTrashed(item)}
				<uui-action-bar slot="actions"> ${this.#renderRemoveAction(item)}</uui-action-bar>
			</uui-card-media>
		`;
	}

	#renderRemoveAction(item: UmbMediaCardItemModel) {
		if (this.readonly) return nothing;
		return html`
			<uui-button label=${this.localize.term('general_remove')} look="secondary" @click=${() => this.#onRemove(item)}>
				<uui-icon name="icon-trash"></uui-icon>
			</uui-button>
		`;
	}

	#renderIsTrashed(item: UmbMediaCardItemModel) {
		if (!item.isTrashed) return nothing;
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
				gap: var(--uui-size-space-3);
				grid-template-columns: repeat(auto-fill, minmax(var(--umb-card-medium-min-width), 1fr));
				grid-auto-rows: var(--umb-card-medium-min-width);
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
