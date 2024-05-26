import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';

import '../../components/input-tiny-mce/input-tiny-mce.element.js';
import { UmbBlockRteEntriesContext, UmbBlockRteManagerContext } from '@umbraco-cms/backoffice/block-rte';
import type { UmbBlockDataBaseValueType } from '@umbraco-cms/backoffice/block';

export interface UmbRichTextEditorValueType extends UmbBlockDataBaseValueType {
	markup: string;
}

/**
 * @element umb-property-editor-ui-tiny-mce
 */
@customElement('umb-property-editor-ui-tiny-mce')
export class UmbPropertyEditorUITinyMceElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	#configuration?: UmbPropertyEditorConfigCollection;

	@property({ attribute: false })
	public set value(value: UmbRichTextEditorValueType | undefined) {
		const buildUpValue: Partial<UmbRichTextEditorValueType> = value ? { ...value } : {};
		buildUpValue.markup ??= '';
		buildUpValue.contentData ??= [];
		buildUpValue.settingsData ??= [];
		this._value = buildUpValue as UmbRichTextEditorValueType;

		this.#managerContext.setContents(buildUpValue.contentData);
		this.#managerContext.setSettings(buildUpValue.settingsData);
	}
	public get value(): UmbRichTextEditorValueType {
		return this._value;
	}

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this.#configuration = config;
	}
	public get config() {
		return this.#configuration;
	}

	@state()
	private _value: UmbRichTextEditorValueType = {
		markup: '',
		contentData: [],
		settingsData: [],
	};

	#managerContext = new UmbBlockRteManagerContext(this);
	#entriesContext = new UmbBlockRteEntriesContext(this);

	constructor() {
		super();

		this.observe(this.#entriesContext.layoutEntries, (layouts) => {
			// Update manager:
			this.#managerContext.setLayouts(layouts);
		});

		this.observe(this.#managerContext.contents, (contents) => {
			this._value = { ...this._value, contentData: contents };
			this.#fireChangeEvent();
		});
		this.observe(this.#managerContext.settings, (settings) => {
			this._value = { ...this._value, settingsData: settings };
			this.#fireChangeEvent();
		});
	}

	#fireChangeEvent() {
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	#onChange(event: InputEvent & { target: HTMLInputElement }) {
		this.value = {
			...this._value,
			markup: event.target.value,
		};
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`
			<umb-input-tiny-mce
				.configuration=${this.#configuration}
				.value=${this._value?.markup ?? ''}
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
