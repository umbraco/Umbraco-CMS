import { CodeSnippetType } from '../../types.js';
import { UMB_TEMPLATING_PAGE_FIELD_BUILDER_MODAL } from '../templating-page-field-builder/templating-page-field-builder-modal.token.js';
import type {
	UmbTemplatingItemPickerModalData,
	UmbTemplatingItemPickerModalValue,
} from './templating-item-picker-modal.token.js';
import { UMB_PARTIAL_VIEW_PICKER_MODAL } from '@umbraco-cms/backoffice/partial-view';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import type { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_MANAGER_CONTEXT, UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UMB_DICTIONARY_PICKER_MODAL } from '@umbraco-cms/backoffice/dictionary';

@customElement('umb-templating-item-picker-modal')
export class UmbTemplatingItemPickerModalElement extends UmbModalBaseElement<
	UmbTemplatingItemPickerModalData,
	UmbTemplatingItemPickerModalValue
> {
	private _close() {
		this.modalContext?.reject();
	}

	#modalContext?: UmbModalManagerContext;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalContext = instance;
		});
	}

	async #openTemplatingPageFieldModal() {
		const pageFieldBuilderContext = this.#modalContext?.open(this, UMB_TEMPLATING_PAGE_FIELD_BUILDER_MODAL);
		const result = await pageFieldBuilderContext?.onSubmit().catch(() => undefined);

		if (result === undefined) return;

		const value = pageFieldBuilderContext?.getValue().output;

		if (!value) return;

		this.value = { value, type: CodeSnippetType.pageField };
		this.modalContext?.submit();
	}

	async #openPartialViewPickerModal() {
		const partialViewPickerContext = this.#modalContext?.open(this, UMB_PARTIAL_VIEW_PICKER_MODAL);
		const result = await partialViewPickerContext?.onSubmit().catch(() => undefined);

		if (result === undefined) return;

		const value = partialViewPickerContext?.getValue().selection[0];

		if (!value) return;

		this.value = {
			value: value,
			type: CodeSnippetType.partialView,
		};
		this.modalContext?.submit();
	}

	async #openDictionaryItemPickerModal() {
		const dictionaryItemPickerModal = this.#modalContext?.open(this, UMB_DICTIONARY_PICKER_MODAL, {
			data: {
				pickableFilter: (item) => item.unique !== null,
			},
		});
		const result = await dictionaryItemPickerModal?.onSubmit().catch(() => undefined);
		if (result === undefined) return;

		const dictionaryItem = dictionaryItemPickerModal?.getValue().selection[0];

		if (!dictionaryItem) return;

		this.value = { value: dictionaryItem, type: CodeSnippetType.dictionaryItem };
		this.modalContext?.submit();
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('template_insert')}>
				<uui-box> ${this.#renderItems()} </uui-box>
				<uui-button
					slot="actions"
					@click=${this._close}
					look="secondary"
					label=${this.localize.term('general_close')}></uui-button>
			</umb-body-layout>
		`;
	}

	#renderItems() {
		return html`<div id="main">
			<uui-button
				@click=${this.#openTemplatingPageFieldModal}
				look="placeholder"
				label=${this.localize.term('template_insert')}>
				<h3><umb-localize key="template_insertPageField">Value</umb-localize></h3>
				<p>
					<umb-localize key="template_insertPageFieldDesc">
						Displays the value of a named field from the current page, with options to modify the value or fallback to
						alternative values.
					</umb-localize>
				</p>
			</uui-button>
			${!this.data?.hidePartialViews
				? html`<uui-button
						@click=${this.#openPartialViewPickerModal}
						look="placeholder"
						label=${this.localize.term('template_insert')}>
						<h3><umb-localize key="template_insertPartialView">Partial view</umb-localize></h3>
						<p>
							<umb-localize key="template_insertPartialViewDesc">
								A partial view is a separate template file which can be rendered inside another template, it's great for
								reusing markup or for separating complex templates into separate files.
							</umb-localize>
						</p>
					</uui-button>`
				: ''}
			<uui-button
				@click=${this.#openDictionaryItemPickerModal}
				look="placeholder"
				label=${this.localize.term('template_insertDictionaryItem')}>
				<h3><umb-localize key="template_insertDictionaryItem">Dictionary Item</umb-localize></h3>
				<p>
					<umb-localize key="template_insertDictionaryItemDesc">
						A dictionary item is a placeholder for a translatable piece of text, which makes it easy to create designs
						for multilingual websites.
					</umb-localize>
				</p>
			</uui-button>
		</div>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
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

export default UmbTemplatingItemPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-templating-item-picker-modal': UmbTemplatingItemPickerModalElement;
	}
}
