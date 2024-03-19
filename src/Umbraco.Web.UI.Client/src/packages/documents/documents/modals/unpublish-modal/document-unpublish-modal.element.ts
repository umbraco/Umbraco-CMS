import { UmbDocumentVariantState, type UmbDocumentVariantOptionModel } from '../../types.js';
import { UmbDocumentTrackedReferenceRepository } from '../../tracked-reference/index.js';
import { UMB_DOCUMENT_CONFIGURATION_CONTEXT } from '../../global-contexts/index.js';
import type {
	UmbDocumentUnpublishModalData,
	UmbDocumentUnpublishModalValue,
} from './document-unpublish-modal.token.js';
import { css, customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';

import '../shared/document-variant-language-picker.element.js';

@customElement('umb-document-unpublish-modal')
export class UmbDocumentUnpublishModalElement extends UmbModalBaseElement<
	UmbDocumentUnpublishModalData,
	UmbDocumentUnpublishModalValue
> {
	#selectionManager = new UmbSelectionManager<string>(this);
	#trackedReferencesRepository = new UmbDocumentTrackedReferenceRepository(this);

	@state()
	_options: Array<UmbDocumentVariantOptionModel> = [];

	@state()
	_hasTrackedReferences = false;

	@state()
	_hasUnpublishPermission = true;

	firstUpdated() {
		this.#configureSelectionManager();
		this.#getTrackedReferences();
	}

	async #configureSelectionManager() {
		this.#selectionManager.setMultiple(true);
		this.#selectionManager.setSelectable(true);

		// Only display variants that are relevant to pick from, i.e. variants that are draft or published with pending changes:
		this._options =
			this.data?.options.filter(
				(option) =>
					option.variant &&
					(!option.variant.state ||
						option.variant.state === UmbDocumentVariantState.PUBLISHED ||
						option.variant.state === UmbDocumentVariantState.PUBLISHED_PENDING_CHANGES),
			) ?? [];

		let selected = this.value?.selection ?? [];

		// Filter selection based on options:
		selected = selected.filter((s) => this._options.some((o) => o.unique === s));

		this.#selectionManager.setSelection(selected);
	}

	async #getTrackedReferences() {
		if (!this.data?.documentUnique) return;

		const { data, error } = await this.#trackedReferencesRepository.requestTrackedReference(
			this.data?.documentUnique,
			0,
			1,
		);

		if (error) {
			console.error(error);
			return;
		}

		if (!data) return;

		this._hasTrackedReferences = data.total > 0;

		// If there are tracked references, we also want to check if we are allowed to unpublish the document:
		if (this._hasTrackedReferences) {
			const documentConfigurationContext = await this.getContext(UMB_DOCUMENT_CONFIGURATION_CONTEXT);
			this._hasUnpublishPermission =
				(await documentConfigurationContext.getDocumentConfiguration())?.disableUnpublishWhenReferenced === false;
		}
	}

	#submit() {
		if (this._hasUnpublishPermission) {
			this.value = { selection: this.#selectionManager.getSelection() };
			this.modalContext?.submit();
			return;
		}
		this.modalContext?.reject();
	}

	#close() {
		this.modalContext?.reject();
	}

	render() {
		return html`<umb-body-layout headline=${this.localize.term('content_unpublish')}>
			<p id="subtitle">
				<umb-localize key="content_languagesToUnpublish">
					Select the languages to unpublish. Unpublishing a mandatory language will unpublish all languages.
				</umb-localize>
			</p>

			<umb-document-variant-language-picker
				.selectionManager=${this.#selectionManager}
				.variantLanguageOptions=${this._options}></umb-document-variant-language-picker>

			<p>
				<umb-localize key="prompt_confirmUnpublish">
					Unpublishing will remove this page and all its descendants from the site.
				</umb-localize>
			</p>

			${this.data?.documentUnique
				? html`
						<umb-document-tracked-reference-table
							id="tracked-references"
							unique=${this.data?.documentUnique}></umb-document-tracked-reference-table>
					`
				: nothing}
			${this._hasTrackedReferences
				? html`<uui-box id="tracked-references-warning">
						<umb-localize key="references_unpublishWarning">
							This item or its descendants is being referenced. Unpublishing can lead to broken links on your website.
							Please take the appropriate actions.
						</umb-localize>
					</uui-box>`
				: nothing}

			<div slot="actions">
				<uui-button label=${this.localize.term('general_close')} @click=${this.#close}></uui-button>
				<uui-button
					label="${this.localize.term('actions_unpublish')}"
					?disabled=${!this._hasUnpublishPermission || !this.#selectionManager.getSelection().length}
					look="primary"
					color="warning"
					@click=${this.#submit}></uui-button>
			</div>
		</umb-body-layout> `;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 600px;
				max-width: 90vw;
			}

			#tracked-references {
				--uui-table-cell-padding: 0;
			}

			#tracked-references-warning {
				margin-top: 1rem;
				background-color: var(--uui-color-danger);
				color: var(--uui-color-danger-contrast);
			}
		`,
	];
}

export default UmbDocumentUnpublishModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-unpublish-modal': UmbDocumentUnpublishModalElement;
	}
}
