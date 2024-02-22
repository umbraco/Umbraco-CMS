import { UmbLanguageCollectionRepository } from '../../collection/index.js';
import type { UmbLanguageItemModel } from '../../repository/index.js';
import type { UmbLanguagePickerModalValue, UmbLanguagePickerModalData } from './language-picker-modal.token.js';
import { html, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-language-picker-modal')
export class UmbLanguagePickerModalElement extends UmbModalBaseElement<
	UmbLanguagePickerModalData,
	UmbLanguagePickerModalValue
> {
	@state()
	private _languages: Array<UmbLanguageItemModel> = [];

	#collectionRepository = new UmbLanguageCollectionRepository(this);
	#selectionManager = new UmbSelectionManager(this);

	connectedCallback(): void {
		super.connectedCallback();
		this.#selectionManager.setSelectable(true);
		this.#selectionManager.setMultiple(this.data?.multiple ?? false);
		this.#selectionManager.setSelection(this.value?.selection ?? []);
	}

	async firstUpdated() {
		const { data } = await this.#collectionRepository.requestCollection({});
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
		return html`<umb-body-layout headline="Select languages">
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
				<uui-button label="Close" @click=${this.#close}></uui-button>
				<uui-button label="Submit" look="primary" color="positive" @click=${this.#submit}></uui-button>
			</div>
		</umb-body-layout> `;
	}
}

export default UmbLanguagePickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-picker-modal': UmbLanguagePickerModalElement;
	}
}
