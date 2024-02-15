import { UMB_MODAL_TEMPLATING_INSERT_CHOOSE_TYPE_SIDEBAR_ALIAS } from '../../modals/manifests.js';
import { getInsertDictionarySnippet, getInsertPartialSnippet } from '../../utils/index.js';
import type { UmbChooseInsertTypeModalValue } from '../../modals/insert-choose-type-sidebar.element.js';
import { CodeSnippetType } from '../../modals/insert-choose-type-sidebar.element.js';
import { UmbDictionaryDetailRepository } from '@umbraco-cms/backoffice/dictionary';
import { customElement, property, css, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	UmbDictionaryItemPickerModalValue,
	UmbModalManagerContext,
	UmbModalContext,
	UmbPartialViewPickerModalValue,
} from '@umbraco-cms/backoffice/modal';
import {
	UMB_DICTIONARY_ITEM_PICKER_MODAL,
	UMB_MODAL_MANAGER_CONTEXT,
	UMB_PARTIAL_VIEW_PICKER_MODAL,
	UmbModalToken,
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

export const UMB_MODAL_TEMPLATING_INSERT_CHOOSE_TYPE_SIDEBAR_MODAL = new UmbModalToken<{ hidePartialView: boolean }>(
	UMB_MODAL_TEMPLATING_INSERT_CHOOSE_TYPE_SIDEBAR_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);

@customElement('umb-templating-insert-menu')
export class UmbTemplatingInsertMenuElement extends UmbLitElement {
	@property()
	value = '';

	private _modalContext?: UmbModalManagerContext;

	#openModal?: UmbModalContext;

	#dictionaryDetailRepository = new UmbDictionaryDetailRepository(this);

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this._modalContext = instance;
		});
	}

	async determineInsertValue(modalValue: UmbChooseInsertTypeModalValue) {
		const { type, value } = modalValue;

		switch (type) {
			case CodeSnippetType.partialView: {
				this.#getPartialViewSnippet(value as UmbPartialViewPickerModalValue);
				break;
			}
			case CodeSnippetType.dictionaryItem: {
				await this.#getDictionaryItemSnippet(value as UmbDictionaryItemPickerModalValue);
				this.#dispatchInsertEvent();

				break;
			}
		}
	}

	#getDictionaryItemSnippet = async (modalValue: UmbDictionaryItemPickerModalValue) => {
		const unique = modalValue.selection[0];
		if (unique === null) return;
		const { data } = await this.#dictionaryDetailRepository.requestByUnique(unique);
		this.value = getInsertDictionarySnippet(data?.name ?? '');
	};

	#getPartialViewSnippet = async (modalValue: UmbPartialViewPickerModalValue) => {
		this.value = getInsertPartialSnippet(modalValue.selection?.[0] ?? '');
	};

	#openChooseTypeModal = () => {
		this.#openModal = this._modalContext?.open(UMB_MODAL_TEMPLATING_INSERT_CHOOSE_TYPE_SIDEBAR_MODAL, {
			data: {
				hidePartialView: this.hidePartialView,
			},
		});
		this.#openModal?.onSubmit().then((closedModal: UmbChooseInsertTypeModalValue) => {
			this.determineInsertValue(closedModal);
		});
	};

	#openInsertPartialViewSidebar() {
		this.#openModal = this._modalContext?.open(UMB_PARTIAL_VIEW_PICKER_MODAL);
		this.#openModal?.onSubmit().then((value) => {
			this.#getPartialViewSnippet(value).then(() => {
				this.#dispatchInsertEvent();
			});
		});
	}

	#openInsertDictionaryItemModal() {
		this.#openModal = this._modalContext?.open(UMB_DICTIONARY_ITEM_PICKER_MODAL, {
			data: {
				hideTreeRoot: true,
				pickableFilter: (item) => item.id !== null,
			},
		});
		this.#openModal?.onSubmit().then((value) => {
			this.#getDictionaryItemSnippet(value).then(() => {
				this.#dispatchInsertEvent();
			});
		});
	}

	#dispatchInsertEvent() {
		this.dispatchEvent(new CustomEvent('insert', { bubbles: false, cancelable: true, composed: false }));
	}

	@property({ type: Boolean })
	hidePartialView = false;

	render() {
		return html`
			<uui-button-group>
				<uui-button look="secondary" @click=${this.#openChooseTypeModal} label=${this.localize.term('template_insert')}>
					<uui-icon name="icon-add"></uui-icon>${this.localize.term('template_insert')}
				</uui-button>
				<umb-dropdown
					look="secondary"
					compact
					placement="bottom-end"
					id="insert-button"
					label=${this.localize.term('template_insert')}>
					<uui-menu-item
						class="insert-menu-item"
						label=${this.localize.term('template_insertDictionaryItem')}
						title=${this.localize.term('template_insertDictionaryItem')}
						@click=${this.#openInsertDictionaryItemModal}>
					</uui-menu-item>
					${!this.hidePartialView
						? html`<uui-menu-item
								class="insert-menu-item"
								label=${this.localize.term('template_insertPartialView')}
								title=${this.localize.term('template_insertPartialView')}
								@click=${this.#openInsertPartialViewSidebar}>
						  </uui-menu-item>`
						: ''}
				</umb-dropdown>
			</uui-button-group>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				--umb-header-layout-height: 70px;
			}

			.insert-menu-item {
				width: 100%;
			}

			uui-icon[name='icon-add'] {
				margin-right: var(--uui-size-4);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-templating-insert-menu': UmbTemplatingInsertMenuElement;
	}
}
