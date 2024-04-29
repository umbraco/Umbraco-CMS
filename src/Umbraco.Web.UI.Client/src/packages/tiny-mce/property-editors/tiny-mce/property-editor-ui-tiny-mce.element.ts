import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';

import '../../components/input-tiny-mce/input-tiny-mce.element.js';

type RichTextEditorValue = {
	blocks: object;
	markup: string;
};

/**
 * @element umb-property-editor-ui-tiny-mce
 */
@customElement('umb-property-editor-ui-tiny-mce')
export class UmbPropertyEditorUITinyMceElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	#configuration?: UmbPropertyEditorConfigCollection;

	@property({ type: Object })
	value?: RichTextEditorValue = {
		blocks: {},
		markup: '',
	};

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this.#configuration = config;
	}
	public get config() {
		return this.#configuration;
	}

	#onChange(event: InputEvent & { target: HTMLInputElement }) {
		this.value = {
			blocks: {},
			markup: event.target.value,
		};
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`
			<umb-input-tiny-mce
				.configuration=${this.#configuration}
				.value=${this.value?.markup ?? ''}
				@change=${this.#onChange}>
			</umb-input-tiny-mce>
		`;
	}
}

export default UmbPropertyEditorUITinyMceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiny-mce': UmbPropertyEditorUITinyMceElement;
	}
}
