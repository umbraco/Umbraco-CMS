import { UMB_DICTIONARY_WORKSPACE_CONTEXT } from '../../dictionary-workspace.context.js';
import { UmbDictionaryRepository } from '../../../repository/dictionary.repository.js';
import { UUITextareaElement, UUITextareaEvent } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state, repeat, ifDefined, unsafeHTML } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { DictionaryItemResponseModel, LanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';
@customElement('umb-workspace-view-dictionary-editor')
export class UmbWorkspaceViewDictionaryEditorElement extends UmbLitElement {
	@state()
	private _dictionary?: DictionaryItemResponseModel;

	#repo!: UmbDictionaryRepository;

	@state()
	private _languages: Array<LanguageResponseModel> = [];

	#workspaceContext!: typeof UMB_DICTIONARY_WORKSPACE_CONTEXT.TYPE;

	async connectedCallback() {
		super.connectedCallback();

		this.#repo = new UmbDictionaryRepository(this);
		this._languages = await this.#repo.getLanguages();

		this.consumeContext(UMB_DICTIONARY_WORKSPACE_CONTEXT, (_instance) => {
			this.#workspaceContext = _instance;
			this.#observeDictionary();
		});
	}

	#observeDictionary() {
		this.observe(this.#workspaceContext.dictionary, (dictionary) => {
			this._dictionary = dictionary;
		});
	}

	#renderTranslation(language: LanguageResponseModel) {
		if (!language.isoCode) return;

		const translation = this._dictionary?.translations?.find((x) => x.isoCode === language.isoCode);

		return html` <umb-workspace-property-layout label=${language.name ?? language.isoCode}>
			<uui-textarea
				slot="editor"
				name=${language.isoCode}
				label="translation"
				@change=${this.#onTextareaChange}
				value=${ifDefined(translation?.translation)}></uui-textarea>
		</umb-workspace-property-layout>`;
	}

	#onTextareaChange(e: Event) {
		if (e instanceof UUITextareaEvent) {
			const target = e.composedPath()[0] as UUITextareaElement;
			const translation = target.value.toString();
			const isoCode = target.getAttribute('name')!;

			this.#workspaceContext.setPropertyValue(isoCode, translation);
		}
	}

	render() {
		return html`
			<uui-box>
				${unsafeHTML(this.localize.term('dictionaryItem_description', this._dictionary?.name || 'unnamed'))}
				${repeat(
					this._languages,
					(item) => item.isoCode,
					(item) => this.#renderTranslation(item),
				)}
			</uui-box>
		`;
	}

	static styles = [
		css`
			:host {
				display: block;
				padding: var(--uui-size-space-6);
			}
		`,
	];
}

export default UmbWorkspaceViewDictionaryEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-view-dictionary-editor': UmbWorkspaceViewDictionaryEditorElement;
	}
}
