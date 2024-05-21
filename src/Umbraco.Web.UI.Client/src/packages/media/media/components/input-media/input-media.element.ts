import type { UmbMediaCardItemModel } from '../../modals/index.js';
import type { UmbMediaItemModel } from '../../repository/index.js';
import { UmbMediaPickerContext } from './input-media.context.js';
import { css, html, customElement, property, state, ifDefined, repeat } from '@umbraco-cms/backoffice/external/lit';
import { splitStringToArray } from '@umbraco-cms/backoffice/utils';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController, UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/modal';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-input-media')
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
			this.dispatchEvent(new UmbChangeEvent());
		},
	});

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

	@property({ type: Boolean })
	multiple = false;

	@property()
	public set value(idsString: string) {
		// Its with full purpose we don't call super.value, as thats being handled by the observation of the context selection.
		this.selection = splitStringToArray(idsString);
	}
	public get value() {
		return this.selection.join(',');
	}

	@state()
	protected editMediaPath = '';

	@state()
	protected items?: Array<UmbMediaCardItemModel>;

	#pickerContext = new UmbMediaPickerContext(this);

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('media')
			.onSetup(() => {
				return { data: { entityType: 'media', preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this.editMediaPath = routeBuilder({});
			});

		this.pickerContextObservers();
		this.addValidators();
	}

	protected pickerContextObservers() {
		this.observe(this.#pickerContext.selection, (selection) => (this.value = selection.join(',')));
		this.observe(this.#pickerContext.cardItems, (cardItems) => {
			this.items = cardItems;
		});
	}

	protected addValidators() {
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
			multiple: this.multiple,
			startNode: this.startNode,
			pickableFilter: this.#pickableFilter,
		});
	}

	protected onRemove(item: UmbMediaCardItemModel) {
		this.#pickerContext.requestRemoveItem(item.unique);
	}

	render() {
		return html`<div class="container">${this.renderItems()} ${this.#renderAddButton()}</div>`;
	}

	protected renderItems() {
		if (!this.items?.length) return;
		return html`${repeat(
			this.items,
			(item) => item.unique,
			(item, index) => this.renderItem(item, index),
		)}`;
	}

	#renderAddButton() {
		if ((this.items && this.max && this.items.length >= this.max) || (this.items?.length && !this.multiple)) return;
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

	protected renderItem(item: UmbMediaCardItemModel, index: number) {
		return html`
			<uui-card-media
				name=${ifDefined(item.name === null ? undefined : item.name)}
				detail=${ifDefined(item.unique)}
				href="${this.editMediaPath}edit/${item.unique}">
				${item.url
					? html`<img src=${item.url} alt=${item.name} />`
					: html`<umb-icon name=${ifDefined(item.mediaType.icon)}></umb-icon>`}
				${this.renderIsTrashed(item)}
				<uui-action-bar slot="actions">
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

	protected renderIsTrashed(item: UmbMediaCardItemModel) {
		if (!item.isTrashed) return;
		return html`
			<uui-tag size="s" slot="tag" color="danger">
				<umb-localize key="mediaPicker_trashed">Trashed</umb-localize>
			</uui-tag>
		`;
	}

	static styles = [
		css`
			:host {
				position: relative;
			}
			.container {
				display: grid;
				gap: var(--uui-size-space-3);
				grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
				grid-auto-rows: 150px;
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

export default UmbInputMediaElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-media': UmbInputMediaElement;
	}
}
