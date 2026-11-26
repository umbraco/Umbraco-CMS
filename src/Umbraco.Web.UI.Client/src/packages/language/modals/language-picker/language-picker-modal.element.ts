import { UmbLanguageCollectionRepository } from '../../collection/index.js';
import type { UmbLanguageItemModel } from '../../types.js';
import type { UmbLanguagePickerModalValue, UmbLanguagePickerModalData } from './language-picker-modal.token.js';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { html, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbDeprecation, UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
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

	override connectedCallback(): void {
		super.connectedCallback();
		this.#selectionManager.setSelectable(true);
		this.#selectionManager.setMultiple(this.data?.multiple ?? false);
		this.#selectionManager.setSelection(this.value?.selection ?? []);

		this.observe(this.#selectionManager.selection, (selection) => {
			this.value = { selection };
		});
	}

	protected override willUpdate(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>) {
		super.willUpdate(_changedProperties);

		// eslint-disable-next-line @typescript-eslint/no-deprecated
		if (_changedProperties.has('data') && this.data?.filter) {
			new UmbDeprecation({
				deprecated: 'The `filter` option of UmbLanguagePickerModalData',
				removeInVersion: '21.0.0',
				solution: 'Use `pickableFilter` to prevent languages from being picked instead.',
			}).warn();
		}
	}

	override async firstUpdated() {
		const { data } = await this.#collectionRepository.requestAllItems();
		this._languages = data?.items ?? [];
	}

	get #filteredLanguages() {
		// eslint-disable-next-line @typescript-eslint/no-deprecated
		if (this.data?.filter) {
			// eslint-disable-next-line @typescript-eslint/no-deprecated
			return this._languages.filter(this.data.filter);
		} else {
			return this._languages;
		}
	}

	#isPickable(item: UmbLanguageItemModel) {
		return this.data?.pickableFilter?.(item) ?? true;
	}

	#submit() {
		this.modalContext?.submit();
	}

	#close() {
		this.modalContext?.reject();
	}

	override render() {
		return html`<umb-body-layout headline=${this.localize.term('defaultdialogs_selectLanguages')}>
			<uui-box>
				${this.#filteredLanguages.length > 0
					? repeat(
							this.#filteredLanguages,
							(item) => item.unique,
							(item) => html`
								<uui-menu-item
									label=${item.name ?? ''}
									?selectable=${this.#isPickable(item)}
									?disabled=${!this.#isPickable(item)}
									@selected=${() => this.#selectionManager.select(item.unique)}
									@deselected=${() => this.#selectionManager.deselect(item.unique)}
									?selected=${this.value.selection.includes(item.unique)}>
									<uui-icon slot="icon" name="icon-globe"></uui-icon>
								</uui-menu-item>
							`,
						)
					: html`<umb-localize key="language_noFallbackLanguages">
							There are no other languages to choose from
						</umb-localize>`}
			</uui-box>
			<div slot="actions">
				<uui-button label=${this.localize.term('general_close')} @click=${this.#close}></uui-button>
				<uui-button
					label=${this.localize.term('general_submit')}
					look="primary"
					color="positive"
					@click=${this.#submit}></uui-button>
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
