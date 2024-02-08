import type {
	UmbDocumentLanguagePickerModalValue,
	UmbDocumentLanguagePickerModalData,
} from './language-picker-modal.token.js';
import { UmbLanguageCollectionRepository } from '@umbraco-cms/backoffice/language';
import type { UmbLanguageItemModel } from '@umbraco-cms/backoffice/language';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, repeat, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-document-language-picker-modal')
export class UmbDocumentLanguagePickerModalElement extends UmbModalBaseElement<
	UmbDocumentLanguagePickerModalData,
	UmbDocumentLanguagePickerModalValue
> {
	@state()
	private _languages: Array<UmbLanguageItemModel> = [];

	#collectionRepository = new UmbLanguageCollectionRepository(this);
	#selectionManager = new UmbSelectionManager(this);

	connectedCallback(): void {
		super.connectedCallback();
		this.#selectionManager.setSelectable(true);
		this.#selectionManager.setMultiple(true);
		this.#selectionManager.setSelection(this.value?.selection ?? []);
	}

	async firstUpdated() {
		const { data } = await this.#collectionRepository.requestCollection({ skip: 0, take: 1000 });
		this._languages = data?.items ?? [];
	}

	get #filteredLanguages() {
		if (this.data?.filter) {
			return this._languages.filter(this.data.filter);
		} else {
			return this._languages;
		}
	}

	#submit() {
		this.value = { selection: this.#selectionManager.getSelection() };
		this.modalContext?.submit();
	}

	#close() {
		this.modalContext?.reject();
	}

	render() {
		return html`<umb-body-layout headline=${ifDefined(this.data?.headline)}>
			<uui-box>
				${repeat(
					this.#filteredLanguages,
					(item) => item.unique,
					(item) => html`
						<uui-menu-item
							label=${item.name ?? ''}
							selectable
							@selected=${() => this.#selectionManager.select(item.unique)}
							@deselected=${() => this.#selectionManager.deselect(item.unique)}
							?selected=${this.#selectionManager.isSelected(item.unique)}>
							<uui-icon slot="icon" name="icon-globe"></uui-icon>
						</uui-menu-item>
					`,
				)}
			</uui-box>
			<div slot="actions">
				<uui-button label=${this.localize.term('general_cancel')} @click=${this.#close}></uui-button>
				<uui-button
					label="${this.data?.confirmLabel ?? this.localize.term('general_submit')}"
					look="primary"
					color="positive"
					@click=${this.#submit}></uui-button>
			</div>
		</umb-body-layout> `;
	}

	static styles = [UmbTextStyles, css``];
}

export default UmbDocumentLanguagePickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-language-picker-modal': UmbDocumentLanguagePickerModalElement;
	}
}
