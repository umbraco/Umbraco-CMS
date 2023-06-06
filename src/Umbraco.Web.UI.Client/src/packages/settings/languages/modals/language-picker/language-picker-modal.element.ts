import { UmbLanguageRepository } from '../../repository/language.repository.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';
import { LanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbSelectionManagerBase } from '@umbraco-cms/backoffice/utils';
import { UmbLanguagePickerModalResult, UmbLanguagePickerModalData } from '@umbraco-cms/backoffice/modal';

@customElement('umb-language-picker-modal')
export class UmbLanguagePickerModalElement extends UmbModalBaseElement<
	UmbLanguagePickerModalData,
	UmbLanguagePickerModalResult
> {
	@state()
	private _languages: Array<LanguageResponseModel> = [];

	#languageRepository = new UmbLanguageRepository(this);
	#selectionManager = new UmbSelectionManagerBase();

	connectedCallback(): void {
		super.connectedCallback();
		this.#selectionManager.setMultiple(this.data?.multiple ?? false);
		this.#selectionManager.setSelection(this.data?.selection ?? []);
	}

	async firstUpdated() {
		const { data } = await this.#languageRepository.requestLanguages();
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
		this.modalContext?.submit({ selection: this.#selectionManager.getSelection() });
	}

	#close() {
		this.modalContext?.reject();
	}

	render() {
		return html`<umb-body-layout headline="Select languages">
			<uui-box>
				${repeat(
					this.#filteredLanguages,
					(item) => item.isoCode,
					(item) => html`
						<uui-menu-item
							label=${item.name ?? ''}
							selectable
							@selected=${() => this.#selectionManager.select(item.isoCode!)}
							@deselected=${() => this.#selectionManager.deselect(item.isoCode!)}
							?selected=${this.#selectionManager.isSelected(item.isoCode!)}>
							<uui-icon slot="icon" name="umb:globe"></uui-icon>
						</uui-menu-item>
					`
				)}
			</uui-box>
			<div slot="actions">
				<uui-button label="Close" @click=${this.#close}></uui-button>
				<uui-button label="Submit" look="primary" color="positive" @click=${this.#submit}></uui-button>
			</div>
		</umb-body-layout> `;
	}

	static styles = [UUITextStyles, css``];
}

export default UmbLanguagePickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-picker-modal': UmbLanguagePickerModalElement;
	}
}
