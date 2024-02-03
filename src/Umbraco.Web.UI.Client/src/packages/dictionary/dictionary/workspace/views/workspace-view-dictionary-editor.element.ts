import { UMB_DICTIONARY_WORKSPACE_CONTEXT } from '../dictionary-workspace.context.js';
import type { UmbDictionaryDetailModel } from '../../types.js';
import type { UUITextareaElement } from '@umbraco-cms/backoffice/external/uui';
import { UUITextareaEvent } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state, repeat, ifDefined, unsafeHTML } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbLanguageCollectionRepository, type UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';

@customElement('umb-workspace-view-dictionary-editor')
export class UmbWorkspaceViewDictionaryEditorElement extends UmbLitElement {
	@state()
	private _dictionary?: UmbDictionaryDetailModel;

	#languageCollectionRepository = new UmbLanguageCollectionRepository(this);

	@state()
	private _languages: Array<UmbLanguageDetailModel> = [];

	#workspaceContext!: typeof UMB_DICTIONARY_WORKSPACE_CONTEXT.TYPE;

	async connectedCallback() {
		super.connectedCallback();

		this.consumeContext(UMB_DICTIONARY_WORKSPACE_CONTEXT, (_instance) => {
			this.#workspaceContext = _instance;
			this.#observeDictionary();
		});
	}

	async firstUpdated() {
		const { data } = await this.#languageCollectionRepository.requestCollection({ skip: 0, take: 200 });
		if (data) {
			this._languages = data.items;
		}
	}

	#observeDictionary() {
		this.observe(this.#workspaceContext.dictionary, (dictionary) => {
			this._dictionary = dictionary;
		});
	}

	#renderTranslation(language: UmbLanguageDetailModel) {
		if (!language.unique) return;

		const translation = this._dictionary?.translations?.find((x) => x.isoCode === language.unique);

		return html` <umb-property-layout label=${language.name ?? language.unique}>
			<uui-textarea
				slot="editor"
				name=${language.unique}
				label="translation"
				@change=${this.#onTextareaChange}
				value=${ifDefined(translation?.translation)}></uui-textarea>
		</umb-property-layout>`;
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
					(item) => item.unique,
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
