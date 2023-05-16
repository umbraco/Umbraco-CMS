import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UMB_MODAL_TEMPLATING_INSERT_FIELD_SIDEBAR_ALIAS } from './manifests';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';
import {
	UMB_MODAL_CONTEXT_TOKEN,
	UmbModalContext,
	UmbModalToken,
	UMB_PARTIAL_VIEW_PICKER_MODAL,
	UmbModalHandler,
	UMB_DICTIONARY_ITEM_PICKER_MODAL,
	UmbDictionaryItemPickerModalResult,
} from '@umbraco-cms/backoffice/modal';

export const UMB_MODAL_TEMPLATING_INSERT_FIELD_SIDEBAR_MODAL = new UmbModalToken(
	UMB_MODAL_TEMPLATING_INSERT_FIELD_SIDEBAR_ALIAS,
	{
		type: 'sidebar',
		size: 'small',
	}
);

export interface ChooseInsertTypeModalData {
	hidePartialViews?: boolean;
}

export enum CodeSnippetType {
	partialView = 'partialView',
	umbracoField = 'umbracoField',
	dictionaryItem = 'dictionaryItem',
	macro = 'macro',
}
export interface ChooseInsertTypeModalResult {
	value: string | UmbDictionaryItemPickerModalResult;
	type: CodeSnippetType;
}

@customElement('umb-templating-choose-insert-type-modal')
export default class UmbChooseInsertTypeModalElement extends UmbModalBaseElement<
	ChooseInsertTypeModalData,
	ChooseInsertTypeModalResult
> {
	private _close() {
		this.modalHandler?.reject();
	}

	private _modalContext?: UmbModalContext;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	#openModal?: UmbModalHandler;

	#openInsertValueSidebar() {
		this.#openModal = this._modalContext?.open(UMB_MODAL_TEMPLATING_INSERT_FIELD_SIDEBAR_MODAL);
		this.#openModal?.onSubmit().then((chosenValue) => {
			if (chosenValue) this.modalHandler?.submit({ value: chosenValue, type: CodeSnippetType.umbracoField });
		});
	}

	#openInsertPartialViewSidebar() {
		this.#openModal = this._modalContext?.open(UMB_PARTIAL_VIEW_PICKER_MODAL);
		this.#openModal?.onSubmit().then((partialViewPickerModalResult) => {
			if (partialViewPickerModalResult)
				this.modalHandler?.submit({
					type: CodeSnippetType.partialView,
					value: partialViewPickerModalResult.selection[0],
				});
		});
	}

	#openInsertDictionaryItemModal() {
		this.#openModal = this._modalContext?.open(UMB_DICTIONARY_ITEM_PICKER_MODAL);
		this.#openModal?.onSubmit().then((dictionaryItemPickerModalResult) => {
			if (dictionaryItemPickerModalResult)
				this.modalHandler?.submit({ value: dictionaryItemPickerModalResult, type: CodeSnippetType.dictionaryItem });
		});
	}

	render() {
		return html`
			<umb-body-layout headline="Insert">
				<div id="main">
					<uui-box>
						<uui-button @click=${this.#openInsertValueSidebar} look="placeholder" label="Insert value"
							><h3>Value</h3>
							<p>
								Displays the value of a named field from the current page, with options to modify the value or fallback
								to alternative values.
							</p></uui-button
						>
						${this.data?.hidePartialViews
							? ''
							: html`<uui-button @click=${this.#openInsertPartialViewSidebar} look="placeholder" label="Insert value"
									><h3>Partial view</h3>
									<p>
										A partial view is a separate template file which can be rendered inside another template, it's great
										for reusing markup or for separating complex templates into separate files.
									</p></uui-button
							  >`}
						<uui-button @click=${this._close} look="placeholder" label="Insert Macro"
							><h3>Macro</h3>
							<p>
								A Macro is a configurable component which is great for reusable parts of your design, where you need the
								option to provide parameters, such as galleries, forms and lists.
							</p></uui-button
						>
						<uui-button @click=${this.#openInsertDictionaryItemModal} look="placeholder" label="Insert Dictionary item"
							><h3>Dictionary item</h3>
							<p>
								A dictionary item is a placeholder for a translatable piece of text, which makes it easy to create
								designs for multilingual websites.
							</p></uui-button
						>
					</uui-box>
				</div>
				<div slot="actions">
					<uui-button @click=${this._close} look="secondary">Close</uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				color: var(--uui-color-text);
			}

			#main {
				box-sizing: border-box;
				padding: var(--uui-size-space-5);
				height: calc(100vh - 124px);
			}

			#main uui-button:not(:last-of-type) {
				display: block;
				margin-bottom: var(--uui-size-space-5);
			}

			h3,
			p {
				text-align: left;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-templating-choose-insert-type-modal': UmbChooseInsertTypeModalElement;
	}
}
