import type { UmbDocumentPublishModalData, UmbDocumentPublishModalValue } from './document-publish-modal.token.js';
import { css, customElement, html, nothing, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { appendToFrozenArray } from '@umbraco-cms/backoffice/observable-api';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

import '../variant-picker/document-variant-language-picker.element.js';

@customElement('umb-document-variant-picker-modal')
export class UmbDocumentPublishModalElement extends UmbModalBaseElement<
	UmbDocumentPublishModalData,
	UmbDocumentPublishModalValue
> {
	#selectionManager = new UmbSelectionManager<string>(this);

	@state()
	_selection: Array<string> = [];

	constructor() {
		super();
		this.observe(this.#selectionManager.selection, (selection) => {
			this._selection = selection;
		});

		this.#configureSelectionManager();
	}

	async #configureSelectionManager() {
		let selected = this.value?.selection ?? [];

		// Filter selection based on options:
		selected = selected.filter((s) => this.data?.options.some((o) => o.unique === s));

		if (selected.length === 0) {
			const context = await this.getContext(UMB_APP_LANGUAGE_CONTEXT);
			const appCulture = context.getAppCulture();
			// If the app language is one of the options, select it by default:
			if (appCulture && this.data?.options.some((o) => o.language.unique === appCulture)) {
				selected = appendToFrozenArray(selected, new UmbVariantId(appCulture, null).toString());
			}
		}

		this.#selectionManager.setMultiple(true);
		this.#selectionManager.setSelectable(true);
		this.#selectionManager.setSelection(selected);

		this.data?.options.forEach((variant) => {
			if (variant.language?.isMandatory) {
				this.#selectionManager.select(variant.unique);
			}
		});
	}

	#submit() {
		this.value = { selection: this.#selectionManager.getSelection() };
		this.modalContext?.submit();
	}

	#close() {
		this.modalContext?.reject();
	}

	render() {
		return html`<umb-body-layout headline=${this.localize.term('content_readyToPublish')}>
			<p id="subtitle">
				${this.localize.term(
					this.data?.allowScheduledPublish ? 'content_languagesToSchedule' : 'content_variantsToPublish',
				)}
			</p>
			<umb-document-variant-language-picker
				.selectionManager=${this.#selectionManager}
				.variantLanguageOptions=${this.data?.options ?? []}></umb-document-variant-language-picker>

			${when(
				this.data?.allowScheduledPublish,
				() => html`This is a scheduled publish`,
				() => nothing,
			)}

			<p>${this.localize.term('content_variantsWillBeSaved')}</p>

			<div slot="actions">
				<uui-button label=${this.localize.term('general_close')} @click=${this.#close}></uui-button>
				<uui-button
					label="${this.localize.term(
						this.data?.allowScheduledPublish ? 'buttons_schedulePublish' : 'buttons_saveAndPublish',
					)}"
					look="primary"
					color="positive"
					@click=${this.#submit}></uui-button>
			</div>
		</umb-body-layout> `;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 400px;
				max-width: 90vw;
			}
		`,
	];
}

export default UmbDocumentPublishModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-publish-modal': UmbDocumentPublishModalElement;
	}
}
