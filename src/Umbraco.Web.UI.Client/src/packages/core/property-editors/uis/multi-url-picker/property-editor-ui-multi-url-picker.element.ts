import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import type { UUIModalSidebarSize } from '@umbraco-cms/backoffice/external/uui';
import { UmbInputMultiUrlElement } from '@umbraco-cms/backoffice/components';
import { UMB_WORKSPACE_PROPERTY_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/workspace';
import { UmbLinkPickerLink } from '@umbraco-cms/backoffice/modal';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbDataTypeConfigCollection } from '@umbraco-cms/backoffice/components';

/**
 * @element umb-property-editor-ui-multi-url-picker
 */
@customElement('umb-property-editor-ui-multi-url-picker')
export class UmbPropertyEditorUIMultiUrlPickerElement
	extends UmbLitElement
	implements UmbPropertyEditorExtensionElement
{
	@property({ type: Array })
	value: UmbLinkPickerLink[] = [];

	@property({ attribute: false })
	public set config(config: UmbDataTypeConfigCollection | undefined) {
		this._overlaySize = config?.getValueByAlias('overlaySize');
		this._hideAnchor = config?.getValueByAlias('hideAnchor');
		this._ignoreUserStartNodes = config?.getValueByAlias('ignoreUserStartNodes');
		this._minNumber = config?.getValueByAlias('minNumber');
		this._maxNumber = config?.getValueByAlias('maxNumber');
	}

	@state()
	private _overlaySize?: UUIModalSidebarSize;

	@state()
	private _hideAnchor?: boolean;

	@state()
	private _ignoreUserStartNodes?: boolean;

	@state()
	private _maxNumber?: number;

	@state()
	private _minNumber?: number;

	@state()
	private _alias?: string;

	@state()
	private _propertyVariantId?: string;

	constructor() {
		super();

		this.consumeContext(UMB_WORKSPACE_PROPERTY_CONTEXT_TOKEN, (context) => {
			this.observe(context.alias, (alias) => {
				this._alias = alias;
			});
			this.observe(context.variantId, (variantId) => {
				this._propertyVariantId = variantId?.toString() || 'invariant';
			});
		});
	}

	private _onChange(event: CustomEvent) {
		this.value = (event.target as UmbInputMultiUrlElement).urls;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<umb-input-multi-url
			.alias="${this._alias}"
			.variantId="${this._propertyVariantId}"
			@change="${this._onChange}"
			.overlaySize="${this._overlaySize}"
			?hide-anchor="${this._hideAnchor}"
			.ignoreUserStartNodes=${this._ignoreUserStartNodes}
			.max=${this._maxNumber}
			.min=${this._minNumber}
			.urls="${this.value ?? []}"></umb-input-multi-url>`;
	}

	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUIMultiUrlPickerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-multi-url-picker': UmbPropertyEditorUIMultiUrlPickerElement;
	}
}
