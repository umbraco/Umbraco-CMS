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
import type { UmbUploadableFileModel } from '@umbraco-cms/backoffice/media';

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

	@property({ type: Boolean })
	ignoreUserStartNodes?: boolean;

	@property()
	public set value(idsString: string) {
		// Its with full purpose we don't call super.value, as thats being handled by the observation of the context selection.
		this.selection = splitStringToArray(idsString);
	}
	public get value() {
		return this.selection.join(',');
	}

	@state()
	private _editMediaPath = '';

	@state()
	private _items?: Array<UmbMediaCardItemModel>;

	#pickerContext = new UmbMediaPickerContext(this);

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
		this.observe(this.#pickerContext.cardItems, (cardItems) => (this._items = cardItems));

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
		// TODO: Configure the media picker, with `ignoreUserStartNodes` [LK]
		this.#pickerContext.openPicker({
			hideTreeRoot: true,
			pickableFilter: this.#pickableFilter,
		});
	}

	async #onUploadCompleted(e: CustomEvent) {
		const completed = e.detail?.completed as Array<UmbUploadableFileModel>;
		const uploaded = completed.map((file) => file.unique);

		this.selection = [...this.selection, ...uploaded];
		this.dispatchEvent(new UmbChangeEvent());
	}

	render() {
		return html`${this.#renderDropzone()}
			<div class="container">${this.#renderItems()} ${this.#renderAddButton()}</div>`;
	}

	#renderDropzone() {
		if (this._items && this._items.length >= this.max) return;
		return html`<umb-dropzone
			id="dropzone"
			?multiple=${this.max === 1}
			@change=${this.#onUploadCompleted}></umb-dropzone>`;
	}

	#renderItems() {
		if (!this._items) return;
		return html`${repeat(
			this._items,
			(item) => item.unique,
			(item) => this.#renderItem(item),
		)}`;
	}

	#renderAddButton() {
		if (this._items && this.max && this._items.length >= this.max) return;
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
		// TODO: `file-ext` value has been hardcoded here. Find out if API model has value for it. [LK]
		return html`
			<uui-card-media name=${ifDefined(item.name === null ? undefined : item.name)} detail=${ifDefined(item.unique)}>
				${item.url
					? html`<img src=${item.url} alt=${item.name} />`
					: html`<umb-icon name=${ifDefined(item.icon)}></umb-icon>`}
				${this.#renderIsTrashed(item)}
				<uui-action-bar slot="actions">
					${this.#renderOpenButton(item)}
					<uui-button label="Copy media">
						<uui-icon name="icon-documents"></uui-icon>
					</uui-button>
					<uui-button
						@click=${() => this.#pickerContext.requestRemoveItem(item.unique)}
						label="Remove media ${item.name}">
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

	#renderOpenButton(item: UmbMediaCardItemModel) {
		if (!this.showOpenButton) return;
		return html`
			<uui-button
				compact
				href="${this._editMediaPath}edit/${item.unique}"
				label=${this.localize.term('general_edit') + ` ${item.name}`}>
				<uui-icon name="icon-edit"></uui-icon>
			</uui-button>
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
				grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
				grid-template-rows: repeat(auto-fill, minmax(160px, 1fr));
			}

			#btn-add {
				text-align: center;
				height: 100%;
			}

			uui-icon {
				display: block;
				margin: 0 auto;
			}

			uui-card-media[drag-placeholder] {
				opacity: 0.2;
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
