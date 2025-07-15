import { getInsertDictionarySnippet, getInsertPartialSnippet } from '../../utils/index.js';
import {
	UMB_TEMPLATING_ITEM_PICKER_MODAL,
	type UmbTemplatingItemPickerModalValue,
} from '../../modals/templating-item-picker/templating-item-picker-modal.token.js';
import { CodeSnippetType } from '../../types.js';
import { UMB_TEMPLATING_PAGE_FIELD_BUILDER_MODAL } from '../../modals/templating-page-field-builder/templating-page-field-builder-modal.token.js';
import { UmbDictionaryDetailRepository, UMB_DICTIONARY_PICKER_MODAL } from '@umbraco-cms/backoffice/dictionary';
import { customElement, property, css, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_PARTIAL_VIEW_PICKER_MODAL } from '@umbraco-cms/backoffice/partial-view';

@customElement('umb-templating-insert-menu')
export class UmbTemplatingInsertMenuElement extends UmbLitElement {
	@property()
	value = '';

	@property({ type: Boolean })
	hidePartialViews = false;

	#modalContext?: UmbModalManagerContext;

	#dictionaryDetailRepository = new UmbDictionaryDetailRepository(this);

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalContext = instance;
		});
	}

	async determineInsertValue(modalValue: UmbTemplatingItemPickerModalValue) {
		const { type, value } = modalValue;

		switch (type) {
			case CodeSnippetType.partialView: {
				const regex = /^%2F|%25dot%25cshtml$/g;
				const prettyPath = value.replace(regex, '').replace(/%2F/g, '/');
				this.value = getInsertPartialSnippet(prettyPath);
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
		const itemPickerContext = this.#modalContext?.open(this, UMB_TEMPLATING_ITEM_PICKER_MODAL, {
			data: { hidePartialViews: this.hidePartialViews },
		});
		const result = await itemPickerContext?.onSubmit().catch(() => undefined);

		if (result === undefined) return;

		const value = itemPickerContext?.getValue();

		if (!value) return;

		this.determineInsertValue(value);
	}

	async #openPartialViewPickerModal() {
		const partialViewPickerContext = this.#modalContext?.open(this, UMB_PARTIAL_VIEW_PICKER_MODAL);
		const result = await partialViewPickerContext?.onSubmit().catch(() => undefined);

		if (result === undefined) return;

		const value = partialViewPickerContext?.getValue().selection[0];

		if (!value) return;

		this.determineInsertValue({ type: CodeSnippetType.partialView, value });
	}

	async #openDictionaryItemPickerModal() {
		const dictionaryItemPickerContext = this.#modalContext?.open(this, UMB_DICTIONARY_PICKER_MODAL);
		const result = await dictionaryItemPickerContext?.onSubmit().catch(() => undefined);

		if (result === undefined) return;

		const value = dictionaryItemPickerContext?.getValue().selection[0];

		if (!value) return;

		this.determineInsertValue({ type: CodeSnippetType.dictionaryItem, value });
	}

	async #openPageFieldBuilderModal() {
		const pageFieldBuilderContext = this.#modalContext?.open(this, UMB_TEMPLATING_PAGE_FIELD_BUILDER_MODAL);
		const result = await pageFieldBuilderContext?.onSubmit().catch(() => undefined);

		if (result === undefined) return;

		const value = pageFieldBuilderContext?.getValue().output;

		if (!value) return;

		// The output is already built due to the preview in the modal. Can insert it directly now.
		this.value = value;
		this.#dispatchInsertEvent();
	}

	#dispatchInsertEvent() {
		this.dispatchEvent(new CustomEvent('insert', { bubbles: false, cancelable: true, composed: false }));
	}

	override render() {
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
					${!this.hidePartialViews
						? html`<uui-menu-item
								class="insert-menu-item"
								label=${this.localize.term('template_insertPartialView')}
								title=${this.localize.term('template_insertPartialView')}
								@click=${this.#openPartialViewPickerModal}>
							</uui-menu-item>`
						: ''}
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

	static override styles = [
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
