import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UmbTagsInputElement } from '../../components/tags-input/tags-input.element';
import { UMB_WORKSPACE_PROPERTY_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/workspace';
import type { UmbDataTypePropertyCollection } from '@umbraco-cms/backoffice/components';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-tags
 */
@customElement('umb-property-editor-ui-tags')
export class UmbPropertyEditorUITagsElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {
	@property()
	value: string[] = [];

	@state()
	private _group?: string;

	@state()
	private _culture?: string | null;
	//TODO: Use type from VariantID

	@property({ type: Array, attribute: false })
	public set config(config: UmbDataTypePropertyCollection) {
		this._group = config.getValueByAlias('group');
		this.value = config.getValueByAlias('items') ?? [];
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

	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUITagsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tags': UmbPropertyEditorUITagsElement;
	}
}
