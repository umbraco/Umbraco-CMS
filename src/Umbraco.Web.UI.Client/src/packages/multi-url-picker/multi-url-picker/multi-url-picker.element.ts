import type { UmbLinkPickerLink } from '../link-picker-modal/types.js';
import { UMB_LINK_PICKER_MODAL } from '../link-picker-modal/link-picker-modal.token.js';
import { css, customElement, html, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { simpleHashCode } from '@umbraco-cms/backoffice/observable-api';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import type { UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UUIModalSidebarSize } from '@umbraco-cms/backoffice/external/uui';

/**
 * @element umb-input-multi-url
 * @fires change - when the value of the input changes
 * @fires blur - when the input loses focus
 * @fires focus - when the input gains focus
 */
const elementName = 'umb-multi-url-picker';
@customElement(elementName)
export class UmbMultiUrlPickerElement extends UUIFormControlMixin(UmbLitElement, '') {
	#sorter = new UmbSorterController<UmbLinkPickerLink>(this, {
		getUniqueOfElement: (element) => {
			return element.id;
		},
		getUniqueOfModel: (modelEntry) => {
			return this.#getUnique(modelEntry);
		},
		identifier: 'Umb.SorterIdentifier.InputMultiUrl',
		itemSelector: 'uui-ref-node',
		containerSelector: 'uui-ref-list',
		onChange: ({ model }) => {
			this.urls = model;
			this.#dispatchChangeEvent();
		},
	});

	protected getFormElement() {
		return undefined;
	}

	@property()
	public set alias(value: string | undefined) {
		this.#linkPickerModal.setUniquePathValue('propertyAlias', value);
	}
	public get alias(): string | undefined {
		return this.#linkPickerModal.getUniquePathValue('propertyAlias');
	}

	@property()
	public set variantId(value: string | UmbVariantId | undefined) {
		this.#linkPickerModal.setUniquePathValue('variantId', value?.toString());
	}
	public get variantId(): string | undefined {
		return this.#linkPickerModal.getUniquePathValue('variantId');
	}

	/**
	 * This is a minimum amount of selected items in this input.
	 * @type {number}
	 * @attr
	 * @default undefined
	 */
	@property({ type: Number })
	min?: number;

	/**
	 * Min validation message.
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: String, attribute: 'min-message' })
	minMessage = 'This field needs more items';

	/**
	 * This is a maximum amount of selected items in this input.
	 * @type {number}
	 * @attr
	 * @default undefined
	 */
	@property({ type: Number })
	max?: number;

	/**
	 * Max validation message.
	 * @type {boolean}
	 * @attr
	 * @default
	 */
	@property({ type: String, attribute: 'min-message' })
	maxMessage = 'This field exceeds the allowed amount of items';

	/**
	 @attr 'hide-anchor'
	 */
	@property({ type: Boolean, attribute: 'hide-anchor' })
	hideAnchor?: boolean;

	/**
	 * @type {UUIModalSidebarSize}
	 * @attr
	 * @default "small"
	 */
	@property()
	overlaySize?: UUIModalSidebarSize;

	/**
	 * @type {Array<UmbLinkPickerLink>}
	 * @default []
	 */
	@property({ attribute: false })
	set urls(data: Array<UmbLinkPickerLink>) {
		data ??= [];
		this.#urls = [...data]; // Unfreeze data coming from State, so we can manipulate it.
		super.value = this.#urls.map((x) => x.url).join(',');
		this.#sorter.setModel(this.#urls);
	}
	get urls(): Array<UmbLinkPickerLink> {
		return this.#urls;
	}

	#urls: Array<UmbLinkPickerLink> = [];

	@state()
	private _modalRoute?: UmbModalRouteBuilder;

	#linkPickerModal;

	constructor() {
		super();

		this.addValidator(
			'rangeUnderflow',
			() => this.minMessage,
			() => !!this.min && this.urls.length < this.min,
		);
		this.addValidator(
			'rangeOverflow',
			() => this.maxMessage,
			() => !!this.max && this.urls.length > this.max,
		);

		this.#linkPickerModal = new UmbModalRouteRegistrationController(this, UMB_LINK_PICKER_MODAL)
			.addAdditionalPath(`:index`)
			.addUniquePaths(['propertyAlias', 'variantId'])
			.onSetup((params) => {
				// Get index:
				const indexParam = params.index;
				if (!indexParam) return false;
				let index: number | null = parseInt(params.index);
				if (Number.isNaN(index)) return false;

				// Use the index to find data:
				let data: UmbLinkPickerLink | null = null;
				if (index >= 0 && index < this.urls.length) {
					data = this.#getItemByIndex(index);
				} else {
					// If not then make a new pick:
					index = null;
				}

				return {
					modal: {
						size: this.overlaySize || 'small',
					},
					data: {
						index: index,
						config: {
							hideAnchor: this.hideAnchor,
						},
					},
					value: {
						link: {
							name: data?.name,
							published: data?.published,
							queryString: data?.queryString,
							target: data?.target,
							trashed: data?.trashed,
							type: data?.type,
							unique: data?.unique,
							url: data?.url,
						},
					},
				};
			})
			.onSubmit((value) => {
				if (!value) return;
				this.#setSelection(value.link, this.#linkPickerModal.modalContext?.data.index ?? null);
			})
			.observeRouteBuilder((routeBuilder) => {
				this._modalRoute = routeBuilder;
			});
	}

	async #requestRemoveItem(index: number) {
		const item = this.#urls[index];
		if (!item) throw new Error('Could not find item at index: ' + index);

		await umbConfirmModal(this, {
			color: 'danger',
			headline: `Remove ${item.name}?`,
			content: 'Are you sure you want to remove this item',
			confirmLabel: 'Remove',
		});

		this.#removeItem(index);
	}

	#removeItem(index: number) {
		this.urls.splice(index, 1);
		this.#dispatchChangeEvent();
	}

	#getItemByIndex(index: number) {
		return this.urls[index];
	}

	#getUnique(link: UmbLinkPickerLink): string {
		return 'x' + simpleHashCode(JSON.stringify(link)).toString(16);
	}

	#setSelection(selection: UmbLinkPickerLink, index: number | null) {
		if (index !== null && index >= 0) {
			this.urls[index] = selection;
		} else {
			this.urls.push(selection);
		}

		this.#dispatchChangeEvent();
	}

	#dispatchChangeEvent() {
		this.requestUpdate();
		this.dispatchEvent(new UmbChangeEvent());
	}

	render() {
		return html`${this.#renderItems()} ${this.#renderAddButton()}`;
	}

	#renderAddButton() {
		if (this.max === 1 && this.urls && this.urls.length >= this.max) return;
		return html`
			<uui-button
				id="btn-add"
				look="placeholder"
				label=${this.localize.term('general_add')}
				.href=${this._modalRoute?.({ index: -1 })}></uui-button>
		`;
	}

	#renderItems() {
		if (!this.urls) return;
		return html`
			<uui-ref-list>
				${repeat(
					this.urls,
					(link) => link.unique,
					(link, index) => this.#renderItem(link, index),
				)}
			</uui-ref-list>
		`;
	}

	#renderItem(link: UmbLinkPickerLink, index: number) {
		const unique = this.#getUnique(link);
		const href = this._modalRoute?.({ index }) ?? '#';
		return html`
			<uui-ref-node
				id=${unique}
				href=${href}
				name=${link.name || ''}
				detail=${(link.url || '') + (link.queryString || '')}>
				<umb-icon slot="icon" name=${link.icon || 'icon-link'}></umb-icon>
				<uui-action-bar slot="actions">
					<uui-button href=${href} label=${this.localize.term('general_edit')}></uui-button>
					<uui-button
						@click=${() => this.#requestRemoveItem(index)}
						label=${this.localize.term('general_remove')}></uui-button>
				</uui-action-bar>
			</uui-ref-node>
		`;
	}

	static override styles = [
		css`
			#btn-add {
				width: 100%;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbMultiUrlPickerElement;
	}
}
