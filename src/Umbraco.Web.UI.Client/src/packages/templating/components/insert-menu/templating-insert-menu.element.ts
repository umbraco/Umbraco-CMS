import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UMB_MODAL_TEMPLATING_INSERT_CHOOSE_TYPE_SIDEBAR_ALIAS } from '../../modals/manifests';
import { UmbDictionaryRepository } from '../../../translation/dictionary/repository/dictionary.repository';
import { getInsertDictionarySnippet, getInsertPartialSnippet } from '../../utils';
import {
	ChooseInsertTypeModalResult,
	CodeSnippetType,
	UMB_MODAL_TEMPLATING_INSERT_FIELD_SIDEBAR_MODAL,
} from '../../modals/insert-choose-type-sidebar.element';
import {
	UMB_DICTIONARY_ITEM_PICKER_MODAL,
	UMB_MODAL_CONTEXT_TOKEN,
	UMB_PARTIAL_VIEW_PICKER_MODAL,
	UmbDictionaryItemPickerModalResult,
	UmbModalContext,
	UmbModalHandler,
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

	private _modalContext?: UmbModalContext;

	#openModal?: UmbModalHandler;

	#dictionaryRepository = new UmbDictionaryRepository(this);

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	async determineInsertValue(modalResult: ChooseInsertTypeModalResult) {
		const { type, value } = modalResult;

		switch (type) {
			case CodeSnippetType.umbracoField: {
				this.#getUmbracoFieldValueSnippet(value as string);
				break;
			}
			case CodeSnippetType.partialView: {
				this.#getPartialViewSnippet(value as UmbPartialViewPickerModalResult);
				break;
			}
			case CodeSnippetType.dictionaryItem: {
				this.#getDictionaryItemSnippet(value as UmbDictionaryItemPickerModalResult);
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

	#getUmbracoFieldValueSnippet = async (value: string) => {
		this.value = value;
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

	#openInsertValueSidebar() {
		this.#openModal = this._modalContext?.open(UMB_MODAL_TEMPLATING_INSERT_FIELD_SIDEBAR_MODAL);
		this.#openModal?.onSubmit().then((value) => {
			this.value = value;
			this.#dispatchInsertEvent();
		});
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
		this.#openModal = this._modalContext?.open(UMB_DICTIONARY_ITEM_PICKER_MODAL);
		this.#openModal?.onSubmit().then((value) => {
			this.#getDictionaryItemSnippet(value).then(() => {
				this.#dispatchInsertEvent();
			});
		});
	}

	#dispatchInsertEvent() {
		this.dispatchEvent(new CustomEvent('insert', { bubbles: true, cancelable: true, composed: false }));
	}

	@property()
	hidePartialView = false;

	render() {
		return html`
			<uui-button-group>
				<uui-button look="secondary" @click=${this.#openChooseTypeModal}>
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
								target="_blank"
								label="Value"
								title="Value"
								@click=${this.#openInsertValueSidebar}>
							</uui-menu-item>
						</li>
						${this.hidePartialView
							? ''
							: html` <li>
									<uui-menu-item
										class="insert-menu-item"
										label="Partial view"
										title="Partial view"
										@click=${this.#openInsertPartialViewSidebar}>
									</uui-menu-item>
							  </li>`}
						<li>
							<uui-menu-item
								class="insert-menu-item"
								label="Dictionary item"
								title="Dictionary item"
								@click=${this.#openInsertDictionaryItemModal}>
							</uui-menu-item>
						</li>
						<li>
							<uui-menu-item class="insert-menu-item" label="Macro" title="Macro"> </uui-menu-item>
						</li>
					</ul>
				</umb-button-with-dropdown>
			</uui-button-group>
		`;
	}

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
