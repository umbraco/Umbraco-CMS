import { UmbTagsInputElement } from '../../components/tags-input/tags-input.element.js';
import { html, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { UMB_WORKSPACE_PROPERTY_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/workspace';
import type { UmbDataTypeConfigCollection } from '@umbraco-cms/backoffice/components';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-tags
 */
@customElement('umb-property-editor-ui-tags')
export class UmbPropertyEditorUITagsElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {
	private _value: Array<string> = [];

	@property({ type: Array })
	public get value(): Array<string> {
		return this._value;
	}
	public set value(value: Array<string>) {
		this._value = value || [];
	}

	@state()
	private _group?: string;

	@state()
	private _culture?: string | null;
	//TODO: Use type from VariantID

	@property({ attribute: false })
	public set config(config: UmbDataTypeConfigCollection | undefined) {
		this._group = config?.getValueByAlias('group');
		this.value = config?.getValueByAlias('items') ?? [];
	}

	constructor() {
		super();
		this.consumeContext(UMB_WORKSPACE_PROPERTY_CONTEXT_TOKEN, (context) => {
			this.observe(context.variantId, (id) => {
				if (id && id.culture !== undefined) {
					this._culture = id.culture;
				}
			});
		});
	}

	private _onChange(event: CustomEvent) {
		this.value = ((event.target as UmbTagsInputElement).value as string).split(',');
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<umb-tags-input
			group="${ifDefined(this._group)}"
			.culture=${this._culture}
			.items=${this.value}
			@change=${this._onChange}></umb-tags-input>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUITagsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tags': UmbPropertyEditorUITagsElement;
	}
}
