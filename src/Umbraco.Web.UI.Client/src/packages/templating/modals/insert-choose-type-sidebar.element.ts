import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';
import {
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UmbModalManagerContext,
	UMB_PARTIAL_VIEW_PICKER_MODAL,
	UmbModalContext,
	UMB_DICTIONARY_ITEM_PICKER_MODAL,
	UmbDictionaryItemPickerModalResult,
} from '@umbraco-cms/backoffice/modal';

export interface ChooseInsertTypeModalData {
	hidePartialViews?: boolean;
}

export enum CodeSnippetType {
	partialView = 'partialView',
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
		this.modalContext?.reject();
	}

	private _modalContext?: UmbModalManagerContext;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	#openModal?: UmbModalContext;

	#openInsertPartialViewSidebar() {
		this.#openModal = this._modalContext?.open(UMB_PARTIAL_VIEW_PICKER_MODAL);
		this.#openModal?.onSubmit().then((partialViewPickerModalResult) => {
			if (partialViewPickerModalResult)
				this.modalContext?.submit({
					type: CodeSnippetType.partialView,
					value: partialViewPickerModalResult.selection[0],
				});
		});
	}

	#openInsertDictionaryItemModal() {
		this.#openModal = this._modalContext?.open(UMB_DICTIONARY_ITEM_PICKER_MODAL, {
			pickableFilter: (item) => item.id !== null,
		});
		this.#openModal?.onSubmit().then((dictionaryItemPickerModalResult) => {
			if (dictionaryItemPickerModalResult)
				this.modalContext?.submit({ value: dictionaryItemPickerModalResult, type: CodeSnippetType.dictionaryItem });
		});
	}

	//TODO: insert this when we have partial views
	#renderInsertPartialViewButton() {
		return html`${this.data?.hidePartialViews
			? ''
			: html`<uui-button @click=${this.#openInsertPartialViewSidebar} look="placeholder" label="Insert value"
					><h3>Partial view</h3>
					<p>
						A partial view is a separate template file which can be rendered inside another template, it's great for
						reusing markup or for separating complex templates into separate files.
					</p></uui-button
			  >`}`;
	}

	render() {
		return html`
			<umb-body-layout headline="Insert">
				<div id="main">
					<uui-box>
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
					<uui-button @click=${this._close} look="secondary" label="Close">Close</uui-button>
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
				--umb-header-layout-height: 70px;
			}

			#main {
				box-sizing: border-box;
				height: calc(
					100dvh - var(--umb-header-layout-height) - var(--umb-footer-layout-height) - 2 * var(--uui-size-layout-1)
				);
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
