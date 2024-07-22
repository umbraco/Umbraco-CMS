import type { CodeEditorLanguage } from '../models/index.js';
import type { UmbCodeEditorElement } from '../components/code-editor.element.js';
import { css, customElement, html, property, state, styleMap } from '@umbraco-cms/backoffice/external/lit';
import { UmbInputEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';

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

	#onChange(event: UmbInputEvent & { target: UmbCodeEditorElement }) {
		if (!(event instanceof UmbInputEvent)) return;
		this.value = event.target.code;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		return html`
			<umb-code-editor
				style=${styleMap({ height: `${this._height}px` })}
				.language=${this._language ?? this.#defaultLanguage}
				.code=${this.value ?? ''}
				@input=${this.#onChange}>
			</umb-code-editor>
		`;
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

export { UmbPropertyEditorUICodeEditorElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbPropertyEditorUICodeEditorElement;
	}
}
