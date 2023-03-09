import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { UUITextareaElement, UUITextareaEvent } from '@umbraco-ui/uui';
import { UmbWorkspaceDictionaryContext } from '../../dictionary-workspace.context';
import { UmbDictionaryRepository } from '../../../repository/dictionary.repository';
import { UmbLitElement } from '@umbraco-cms/element';
import { DictionaryItemModel, LanguageModel } from '@umbraco-cms/backend-api';

@customElement('umb-workspace-view-dictionary-edit')
export class UmbWorkspaceViewDictionaryEditElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				padding: var(--uui-size-space-6);
			}
		`,
	];

	@state()
	private _dictionary?: DictionaryItemModel;

	#repo!: UmbDictionaryRepository;

	@state()
	private _languages: Array<LanguageModel> = [];

	#workspaceContext!: UmbWorkspaceDictionaryContext;

	async connectedCallback() {
		super.connectedCallback();

		this.#repo = new UmbDictionaryRepository(this);
		this._languages = await this.#repo.getLanguages();

		this.consumeContext<UmbWorkspaceDictionaryContext>('umbWorkspaceContext', (_instance) => {
			this.#workspaceContext = _instance;
			this.#observeDictionary();
		});
	}

	#observeDictionary() {
		this.observe(this.#workspaceContext.dictionary, (dictionary) => {
			this._dictionary = dictionary;
		});
	}

	#renderTranslation(language: LanguageModel) {
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
				<p>Edit the different language versions for the dictionary item '<em>${this._dictionary?.name}</em>' below.</p>

				${repeat(
					this._languages,
					(item) => item.isoCode,
					(item) => this.#renderTranslation(item)
				)}
			</uui-box>
		`;
	}
}

export default UmbWorkspaceViewDictionaryEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-view-dictionary-edit': UmbWorkspaceViewDictionaryEditElement;
	}
}
