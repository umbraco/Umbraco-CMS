import {
	UMB_PARTIAL_VIEW_PICKER_MODAL,
	type UmbPartialViewPickerModalValue,
} from '../../modals/partial-view-picker/partial-view-picker-modal.token.js';
import { CodeSnippetType } from '../../types.js';
import {
	UMB_TEMPLATING_ITEM_PICKER_MODAL,
	type UmbTemplatingItemPickerModalValue,
} from '../../modals/templating-item-picker/templating-item-picker-modal.token.js';
import { getInsertDictionarySnippet, getInsertPartialSnippet } from '../../utils/index.js';
import { UmbDictionaryDetailRepository } from '@umbraco-cms/backoffice/dictionary';
import { customElement, property, css, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	UmbDictionaryItemPickerModalValue,
	UmbModalManagerContext,
	UmbModalContext,
} from '@umbraco-cms/backoffice/modal';
import { UMB_DICTIONARY_ITEM_PICKER_MODAL, UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

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

	async determineInsertValue(modalValue: UmbTemplatingItemPickerModalValue) {
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
		this.#openModal = this._modalContext?.open(UMB_TEMPLATING_ITEM_PICKER_MODAL, {
			data: {
				hidePartialViews: this.hidePartialView,
			},
		});
		this.#openModal?.onSubmit().then((closedModal: UmbTemplatingItemPickerModalValue) => {
			this.determineInsertValue(closedModal);
		});
	};

	#openInsertPageFieldSidebar() {
		//this.#openModel = this._modalContext?.open();
	}

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
						label=${this.localize.term('template_insertPageField')}
						title=${this.localize.term('template_insertPageField')}
						@click=${this.#openInsertPageFieldSidebar}></uui-menu-item>
					<uui-menu-item
						class="insert-menu-item"
						label=${this.localize.term('template_insertPartialView')}
						title=${this.localize.term('template_insertPartialView')}
						@click=${this.#openInsertPartialViewSidebar}>
					</uui-menu-item>
					<uui-menu-item
						class="insert-menu-item"
						label=${this.localize.term('template_insertDictionaryItem')}
						title=${this.localize.term('template_insertDictionaryItem')}
						@click=${this.#openInsertDictionaryItemModal}>
					</uui-menu-item>
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
