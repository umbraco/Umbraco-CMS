import { UMB_MODAL_TEMPLATING_INSERT_CHOOSE_TYPE_SIDEBAR_ALIAS } from '../../modals/manifests.js';
import { getInsertDictionarySnippet, getInsertPartialSnippet } from '../../utils/index.js';
import type { UmbChooseInsertTypeModalValue} from '../../modals/insert-choose-type-sidebar.element.js';
import { CodeSnippetType } from '../../modals/insert-choose-type-sidebar.element.js';
import { UmbDictionaryRepository } from '@umbraco-cms/backoffice/dictionary';
import { customElement, property, css, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	UmbDictionaryItemPickerModalValue,
	UmbModalManagerContext,
	UmbModalContext,
	UmbPartialViewPickerModalValue} from '@umbraco-cms/backoffice/modal';
import {
	UMB_DICTIONARY_ITEM_PICKER_MODAL,
	UMB_MODAL_MANAGER_CONTEXT,
	UMB_PARTIAL_VIEW_PICKER_MODAL,
	UmbModalToken
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

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

	#dictionaryRepository = new UmbDictionaryRepository(this);

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
			case CodeSnippetType.macro: {
				throw new Error('Not implemented');
			}
		}
	}

	#getDictionaryItemSnippet = async (modalValue: UmbDictionaryItemPickerModalValue) => {
		const id = modalValue.selection[0];
		if (id === null) return;
		const { data } = await this.#dictionaryRepository.requestById(id);
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
				<uui-button look="secondary" @click=${this.#openChooseTypeModal} label="Choose value to insert">
					<uui-icon name="icon-add"></uui-icon>Insert
				</uui-button>
				<umb-dropdown look="secondary" compact placement="bottom-end" id="insert-button" label="open insert menu">
					<uui-menu-item
						class="insert-menu-item"
						label="Dictionary item"
						title="Dictionary item"
						@click=${this.#openInsertDictionaryItemModal}>
					</uui-menu-item>

					<!-- <li>
							<uui-menu-item class="insert-menu-item" label="Macro" title="Macro"> </uui-menu-item>
						</li> -->
				</umb-dropdown>
			</uui-button-group>
		`;
	}

	//TODO: put this back in when partial view is implemented
	// ${this.hidePartialView
	// 		? ''
	// 		: html` <li>
	// 				<uui-menu-item
	// 					class="insert-menu-item"
	// 					label="Partial view"
	// 					title="Partial view"
	// 					@click=${this.#openInsertPartialViewSidebar}>
	// 				</uui-menu-item>
	// 		  </li>`}

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
