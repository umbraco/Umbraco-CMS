import type { CodeEditorLanguage } from '../models/index.js';
import type { UmbCodeEditorElement } from '../components/code-editor.element.js';
import { css, customElement, html, property, state, styleMap } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent, UmbInputEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';

import '../components/code-editor.element.js';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

const elementName = 'umb-property-editor-ui-code-editor';

@customElement(elementName)
export class UmbPropertyEditorUICodeEditorElement
	extends UmbFormControlMixin<string, typeof UmbLitElement, undefined>(UmbLitElement)
	implements UmbPropertyEditorUiElement
{
	#defaultLanguage: CodeEditorLanguage = 'javascript';

	@state()
	private _language?: CodeEditorLanguage = this.#defaultLanguage;

	@state()
	private _height = 400;

	@state()
	private _lineNumbers = true;

	@state()
	private _minimap = true;

	@state()
	private _wordWrap = false;

	mandatory?: boolean;
	@property({ type: String })
	mandatoryMessage = UMB_VALIDATION_EMPTY_LOCALIZATION_KEY;

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const language = config?.getValueByAlias<Array<CodeEditorLanguage> | CodeEditorLanguage | undefined>('language');
		this._language = Array.isArray(language) ? language[0] : language;

		this._height = Number(config?.getValueByAlias('height')) || 400;
		this._lineNumbers = config?.getValueByAlias('lineNumbers') ?? false;
		this._minimap = config?.getValueByAlias('minimap') ?? false;
		this._wordWrap = config?.getValueByAlias('wordWrap') ?? false;
	}

	#onChange(event: UmbInputEvent & { target: UmbCodeEditorElement }) {
		if (!(event instanceof UmbInputEvent)) return;
		this.value = event.target.code;
		this.dispatchEvent(new UmbChangeEvent());
	}

	constructor() {
		super();

		this.addValidator(
			'valueMissing',
			() => this.mandatoryMessage,
			() => !!this.mandatory && (!this.value || this.value.length === 0),
		);
	}

	override render() {
		return html`
			<umb-code-editor
				style=${styleMap({ height: `${this._height}px` })}
				.language=${this._language ?? this.#defaultLanguage}
				.code=${this.value ?? ''}
				?disable-line-numbers=${!this._lineNumbers}
				?disable-minimap=${!this._minimap}
				?word-wrap=${this._wordWrap}
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
