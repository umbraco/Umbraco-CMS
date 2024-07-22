import type { UmbCodeEditorElement } from '../components/code-editor.element.js';
import type { CodeEditorLanguage } from '../models/index.js';
import { html, customElement, property, state, css, styleMap } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import {
	UmbPropertyValueChangeEvent,
	type UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import type { UmbInputEvent } from '@umbraco-cms/backoffice/event';

import '../components/code-editor.element.js';

const elementName = 'umb-property-editor-ui-code-editor';

@customElement(elementName)
export class UmbPropertyEditorUICodeEditorElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	#defaultLanguage: CodeEditorLanguage = 'javascript';

	@state()
	private _language?: CodeEditorLanguage = this.#defaultLanguage;

	@state()
	private _height = 400;

	@property()
	value = '';

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		this._language = config?.getValueByAlias<CodeEditorLanguage>('language') ?? this.#defaultLanguage;
		this._height = Number(config?.getValueByAlias('height')) || 400;
	}

	#onChange(e: UmbInputEvent) {
		const target = e.target as UmbCodeEditorElement;
		this.value = target.code as string;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		return html`<umb-code-editor
			style=${styleMap({ height: `${this._height}px` })}
			.code=${this.value ?? ''}
			@input=${this.#onChange}></umb-code-editor>`;
				.language=${this._language ?? this.#defaultLanguage}
	}

	static override styles = [
		css`
			umb-code-editor {
				border-radius: var(--uui-border-radius);
				border: 1px solid var(--uui-color-divider-emphasis);
			}
		`,
	];
}

export default UmbPropertyEditorUICodeEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbPropertyEditorUICodeEditorElement;
	}
}
