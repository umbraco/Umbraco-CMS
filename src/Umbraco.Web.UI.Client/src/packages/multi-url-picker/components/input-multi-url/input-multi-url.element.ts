import type { UmbLinkPickerLink } from '../../link-picker-modal/types.js';
import { UMB_LINK_PICKER_MODAL } from '../../link-picker-modal/link-picker-modal.token.js';
import {
	css,
	customElement,
	html,
	ifDefined,
	nothing,
	property,
	repeat,
	state,
	when,
} from '@umbraco-cms/backoffice/external/lit';
import { simpleHashCode } from '@umbraco-cms/backoffice/observable-api';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import {
	UmbDocumentItemRepository,
	UmbDocumentUrlRepository,
	UmbDocumentUrlsDataResolver,
} from '@umbraco-cms/backoffice/document';
import { UmbMediaItemRepository, UmbMediaUrlRepository } from '@umbraco-cms/backoffice/media';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import type { UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';
import type { UUIModalSidebarSize } from '@umbraco-cms/backoffice/external/uui';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

/**
 * @element umb-input-multi-url
 * @fires change - when the value of the input changes
 * @fires blur - when the input loses focus
 * @fires focus - when the input gains focus
 */
@customElement('umb-input-multi-url')
export class UmbInputMultiUrlElement extends UmbFormControlMixin<string, typeof UmbLitElement, undefined>(
	UmbLitElement,
) {
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

	protected override getFormElement() {
		return undefined;
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
	@property({ type: String, attribute: 'max-message' })
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
	 * @default
	 */
	@property({ attribute: false })
	set urls(data: Array<UmbLinkPickerLink>) {
		data ??= [];
		this.#urls = [...data]; // Unfreeze data coming from State, so we can manipulate it.
		super.value = this.#urls.map((x) => x.url).join(',');
		this.#sorter.setModel(this.#urls);
		this.#populateLinksNameAndUrl();
	}
	get urls(): Array<UmbLinkPickerLink> {
		return this.#urls;
	}

	#urls: Array<UmbLinkPickerLink> = [];

	#documentItemRepository = new UmbDocumentItemRepository(this);
	#documentUrlRepository = new UmbDocumentUrlRepository(this);
	#documentUrlsDataResolver = new UmbDocumentUrlsDataResolver(this);

	#mediaItemRepository = new UmbMediaItemRepository(this);
	#mediaUrlRepository = new UmbMediaUrlRepository(this);

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
	@property({ type: Boolean })
	required = false;
	@property({ type: String })
	requiredMessage = UMB_VALIDATION_EMPTY_LOCALIZATION_KEY;

	@state()
	private _modalRoute?: UmbModalRouteBuilder;

	@state()
	private _resolvedLinkNames: Array<{ unique: string; name: string }> = [];

	@state()
	private _resolvedLinkUrls: Array<{ unique: string; url: string }> = [];

	#linkPickerModal;

	constructor() {
		super();

		this.addValidator(
			'valueMissing',
			() => this.requiredMessage,
			() => !this.readonly && this.required && (!this.value || this.value === ''),
		);

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
						isNew: index === null,
						config: {
							hideAnchor: this.hideAnchor,
						},
					},
					value: {
						link: {
							icon: data?.icon,
							name: data?.name,
							published: data?.published,
							queryString: data?.queryString,
							target: data?.target,
							trashed: data?.trashed,
							type: data?.type,
							unique: data?.unique,
							url: data?.url,
							culture: data?.culture,
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

	#populateLinksNameAndUrl() {
		this._resolvedLinkNames = [];
		this._resolvedLinkUrls = [];

		// Documents and media have URLs saved in the local link format.
		// Display the actual URL to align with what the user sees when they selected it initially.
		this.#urls.forEach(async (link) => {
			if (!link.unique) return;

			let name: string | undefined = undefined;
			let url: string | undefined = undefined;

			switch (link.type) {
				case 'document': {
					if (!link.name || link.name.length === 0) {
						name = await this.#getNameForDocument(link.unique);
					}
					url = await this.#getUrlForDocument(link.unique);
					break;
				}
				case 'media': {
					if (!link.name || link.name.length === 0) {
						name = await this.#getNameForMedia(link.unique);
					}
					url = await this.#getUrlForMedia(link.unique);
					break;
				}
				default:
					break;
			}

			if (name) {
				const resolvedName = { unique: link.unique, name };
				this._resolvedLinkNames = [...this._resolvedLinkNames, resolvedName];
			}

			if (url) {
				const resolvedUrl = { unique: link.unique, url };
				this._resolvedLinkUrls = [...this._resolvedLinkUrls, resolvedUrl];
			}
		});
	}

	async #getUrlForDocument(unique: string) {
		const { data: data } = await this.#documentUrlRepository.requestItems([unique]);

		this.#documentUrlsDataResolver.setData(data?.[0]?.urls);

		const resolvedUrls = await this.#documentUrlsDataResolver.getUrls();
		return resolvedUrls?.[0]?.url ?? '';
	}

	async #getUrlForMedia(unique: string) {
		const { data } = await this.#mediaUrlRepository.requestItems([unique]);
		return data?.[0].url ?? '';
	}

	async #getNameForDocument(unique: string) {
		const { data } = await this.#documentItemRepository.requestItems([unique]);
		// TODO: [v17] Review usage of `item.variants[0].name` as this needs to be implemented properly! [LK]
		return data?.[0]?.variants[0].name ?? '';
	}

	async #getNameForMedia(unique: string) {
		const { data } = await this.#mediaItemRepository.requestItems([unique]);
		return data?.[0]?.name ?? '';
	}

	async #requestRemoveItem(index: number, name?: string) {
		const item = this.#urls[index];
		if (!item) throw new Error('Could not find item at index: ' + index);

		await umbConfirmModal(this, {
			color: 'danger',
			headline: `Remove ${name || item.name || 'item'}?`,
			content: 'Are you sure you want to remove this item?',
			confirmLabel: '#general_remove',
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

	#getResolvedItemName(link: UmbLinkPickerLink): string {
		return (link.name || this._resolvedLinkNames.find((name) => name.unique === link.unique)?.name) ?? '';
	}

	#getResolvedItemUrl(link: UmbLinkPickerLink): string {
		const baseUrl = link.culture
			? link.url
			: (this._resolvedLinkUrls.find((url) => url.unique === link.unique)?.url ?? '');
		return baseUrl + (link.queryString || '');
	}

	override render() {
		return html`${this.#renderItems()} ${this.#renderAddButton()}`;
	}

	#renderAddButton() {
		if (this.max === 1 && this.urls && this.urls.length >= this.max) return nothing;
		if (this.readonly && this.urls.length > 0) {
			return nothing;
		} else {
			return html`
				<uui-button
					id="btn-add"
					look="placeholder"
					label=${this.localize.term('general_add')}
					.href=${this._modalRoute?.({ index: -1 })}
					?disabled=${this.readonly}></uui-button>
			`;
		}
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
		const href = this.readonly ? undefined : (this._modalRoute?.({ index }) ?? undefined);
		const name = this.#getResolvedItemName(link);
		const url = this.#getResolvedItemUrl(link);

		return html`
			<uui-ref-node
				id=${unique}
				href=${ifDefined(href)}
				name=${name || url}
				detail=${ifDefined(name ? url : undefined)}
				?readonly=${this.readonly}>
				<umb-icon slot="icon" name=${link.icon || 'icon-link'}></umb-icon>
				${when(
					!this.readonly,
					() => html`
						<uui-action-bar slot="actions">
							<uui-button
								label=${this.localize.term('general_remove')}
								@click=${() => this.#requestRemoveItem(index, name)}></uui-button>
						</uui-action-bar>
					`,
				)}
			</uui-ref-node>
		`;
	}

	static override styles = [
		css`
			#btn-add {
				width: 100%;
			}

			uui-ref-list:not(:has(:nth-child(1))) {
				margin-top: -20px;
				padding-top: 20px;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-multi-url': UmbInputMultiUrlElement;
	}
}
