import { UMB_MODAL_TEMPLATING_INSERT_CHOOSE_TYPE_SIDEBAR_ALIAS } from '../../modals/manifests.js';
import { getInsertDictionarySnippet, getInsertPartialSnippet } from '../../utils.js';
import { ChooseInsertTypeModalResult, CodeSnippetType } from '../../modals/insert-choose-type-sidebar.element.js';
import { UmbDictionaryRepository } from '@umbraco-cms/backoffice/dictionary';
import { customElement, property, css, html } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import {
	UMB_DICTIONARY_ITEM_PICKER_MODAL,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_PARTIAL_VIEW_PICKER_MODAL,
	UmbDictionaryItemPickerModalResult,
	UmbModalManagerContext,
	UmbModalContext,
	UmbModalToken,
	UmbPartialViewPickerModalResult,
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

export const UMB_MODAL_TEMPLATING_INSERT_CHOOSE_TYPE_SIDEBAR_MODAL = new UmbModalToken<{ hidePartialView: boolean }>(
	UMB_MODAL_TEMPLATING_INSERT_CHOOSE_TYPE_SIDEBAR_ALIAS,
	{
		type: 'sidebar',
		size: 'small',
	}
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
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	async determineInsertValue(modalResult: ChooseInsertTypeModalResult) {
		const { type, value } = modalResult;

		switch (type) {
			case CodeSnippetType.partialView: {
				this.#getPartialViewSnippet(value as UmbPartialViewPickerModalResult);
				break;
			}
			case CodeSnippetType.dictionaryItem: {
				await this.#getDictionaryItemSnippet(value as UmbDictionaryItemPickerModalResult);
				this.#dispatchInsertEvent();

				break;
			}
			case CodeSnippetType.macro: {
				throw new Error('Not implemented');
			}
		}
	}

	#getDictionaryItemSnippet = async (modalResult: UmbDictionaryItemPickerModalResult) => {
		const id = modalResult.selection[0];
		if (id === null) return;
		const { data } = await this.#dictionaryRepository.requestById(id);
		this.value = getInsertDictionarySnippet(data?.name ?? '');
	};

	#getPartialViewSnippet = async (modalResult: UmbPartialViewPickerModalResult) => {
		this.value = getInsertPartialSnippet(modalResult.selection?.[0] ?? '');
	};

	#openChooseTypeModal = () => {
		this.#openModal = this._modalContext?.open(UMB_MODAL_TEMPLATING_INSERT_CHOOSE_TYPE_SIDEBAR_MODAL, {
			hidePartialView: this.hidePartialView,
		});
		this.#openModal?.onSubmit().then((closedModal: ChooseInsertTypeModalResult) => {
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
			pickableFilter: (item) => item.id !== null,
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
					<uui-icon name="umb:add"></uui-icon>Insert</uui-button
				>
				<umb-button-with-dropdown
					look="secondary"
					compact
					placement="bottom-start"
					id="insert-button"
					label="open insert menu">
					<ul id="insert-menu" slot="dropdown">
						<li>
							<uui-menu-item
								class="insert-menu-item"
								label="Dictionary item"
								title="Dictionary item"
								@click=${this.#openInsertDictionaryItemModal}>
							</uui-menu-item>
						</li>
						<!-- <li>
							<uui-menu-item class="insert-menu-item" label="Macro" title="Macro"> </uui-menu-item>
						</li> -->
					</ul>
				</umb-button-with-dropdown>
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
		UUITextStyles,
		css`
			:host {
				--umb-header-layout-height: 70px;
			}

			#insert-menu {
				margin: 0;
				padding: 0;
				margin-top: var(--uui-size-space-3);
				background-color: var(--uui-color-surface);
				box-shadow: var(--uui-shadow-depth-3);
				min-width: 150px;
			}

			#insert-menu > li,
			ul {
				padding: 0;
				width: 100%;
				list-style: none;
			}

			ul {
				transform: translateX(-100px);
			}

			.insert-menu-item {
				width: 100%;
			}

			umb-button-with-dropdown {
				--umb-button-with-dropdown-symbol-expand-margin-left: 0;
			}

			uui-icon[name='umb:add'] {
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
