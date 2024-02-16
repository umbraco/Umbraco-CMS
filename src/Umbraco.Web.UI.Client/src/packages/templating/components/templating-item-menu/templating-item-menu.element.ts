import { UMB_PARTIAL_VIEW_PICKER_MODAL } from '../../modals/partial-view-picker/partial-view-picker-modal.token.js';
import { UMB_TEMPLATING_PAGE_FIELD_BUILDER_MODAL } from '../../modals/templating-page-field-builder/templating-page-field-builder-modal.token.js';
import { CodeSnippetType } from '../../types.js';
import {
	UMB_TEMPLATING_ITEM_PICKER_MODAL,
	type UmbTemplatingItemPickerModalValue,
} from '../../modals/templating-item-picker/templating-item-picker-modal.token.js';
import { getInsertDictionarySnippet, getInsertPartialSnippet } from '../../utils/index.js';
import { UmbDictionaryDetailRepository } from '@umbraco-cms/backoffice/dictionary';
import { customElement, property, css, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbModalManagerContext, UmbModalContext } from '@umbraco-cms/backoffice/modal';
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
				this.value = getInsertPartialSnippet(value);
				this.#dispatchInsertEvent();
				break;
			}
			case CodeSnippetType.dictionaryItem: {
				await this.#getDictionaryItemSnippet(value);
				this.#dispatchInsertEvent();
				break;
			}
			case CodeSnippetType.pageField: {
				this.value = value;
				this.#dispatchInsertEvent();
				break;
			}
		}
	}

	async #getDictionaryItemSnippet(unique: string) {
		if (unique === null) return;
		const { data } = await this.#dictionaryDetailRepository.requestByUnique(unique);
		this.value = getInsertDictionarySnippet(data?.name ?? '');
	}

	async #openTemplatingItemPickerModal() {
		const itemPickerContext = this._modalContext?.open(UMB_TEMPLATING_ITEM_PICKER_MODAL);
		await itemPickerContext?.onSubmit();

		const value = itemPickerContext?.getValue();
		if (!value) return;

		this.determineInsertValue(value);
	}

	async #openPartialViewPickerModal() {
		const partialViewPickerContext = this._modalContext?.open(UMB_PARTIAL_VIEW_PICKER_MODAL);
		await partialViewPickerContext?.onSubmit();

		const path = partialViewPickerContext?.getValue().selection[0];
		if (!path) return;

		this.determineInsertValue({ type: CodeSnippetType.partialView, value: path });
	}

	async #openDictionaryItemPickerModal() {
		const dictionaryItemPickerContext = this._modalContext?.open(UMB_DICTIONARY_ITEM_PICKER_MODAL);
		await dictionaryItemPickerContext?.onSubmit();

		const item = dictionaryItemPickerContext?.getValue().selection[0];
		if (!item) return;

		this.determineInsertValue({ type: CodeSnippetType.dictionaryItem, value: item });
	}

	async #openPageFieldBuilderModal() {
		const pageFieldBuilderContext = this._modalContext?.open(UMB_TEMPLATING_PAGE_FIELD_BUILDER_MODAL);
		await pageFieldBuilderContext?.onSubmit();

		const output = pageFieldBuilderContext?.getValue().output;
		if (!output) return;

		// The output is already built due to the preview in the modal. Can insert it directly now.
		this.value = output;
		this.#dispatchInsertEvent();
	}

	#dispatchInsertEvent() {
		this.dispatchEvent(new CustomEvent('insert', { bubbles: false, cancelable: true, composed: false }));
	}

	render() {
		return html`
			<uui-button-group>
				<uui-button
					look="secondary"
					@click=${this.#openTemplatingItemPickerModal}
					label=${this.localize.term('template_insert')}>
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
						@click=${this.#openPageFieldBuilderModal}></uui-menu-item>
					<uui-menu-item
						class="insert-menu-item"
						label=${this.localize.term('template_insertPartialView')}
						title=${this.localize.term('template_insertPartialView')}
						@click=${this.#openPartialViewPickerModal}>
					</uui-menu-item>
					<uui-menu-item
						class="insert-menu-item"
						label=${this.localize.term('template_insertDictionaryItem')}
						title=${this.localize.term('template_insertDictionaryItem')}
						@click=${this.#openDictionaryItemPickerModal}>
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
