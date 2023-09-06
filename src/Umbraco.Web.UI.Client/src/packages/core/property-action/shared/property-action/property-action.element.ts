import type { UmbPropertyAction } from './property-action.model.js';
import { CSSResultGroup, html, LitElement, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { createExtensionElement } from '@umbraco-cms/backoffice/extension-api';

import type { ManifestPropertyAction } from '@umbraco-cms/backoffice/extension-registry';

// TODO: Here is a problem. The UmbPropertyActionElement is used for the type of the Extension Element. But is also a component that renders the Extension Element...
@customElement('umb-property-action')
export class UmbPropertyActionElement extends LitElement implements UmbPropertyAction {
	private _propertyAction?: ManifestPropertyAction;
	@property({ type: Object })
	public get propertyAction(): ManifestPropertyAction | undefined {
		return this._propertyAction;
	}
	public set propertyAction(value: ManifestPropertyAction | undefined) {
		this._propertyAction = value;
		this._createElement();
	}

	// TODO: we need to investigate context api vs values props and events
	@property({ attribute: false })
	public value?: unknown;

	@state()
	private _element?: UmbPropertyActionElement;

	private async _createElement() {
		if (!this.propertyAction) return;

		try {
			// TODO: Here is a problem. The UmbPropertyActionElement is used for the type of the Extension Element. But is also a component that renders the Extension Element...
			this._element = (await createExtensionElement(this.propertyAction)) as UmbPropertyActionElement | undefined;
			if (!this._element) return;

			this._element.value = this.value;
		} catch (error) {
			// TODO: loading JS failed so we should do some nice UI. (This does only happen if extension has a js prop, otherwise we concluded that no source was needed resolved the load.)
		}
	}

	render() {
		return html`${this._element}`;
	}

	static styles: CSSResultGroup = [UmbTextStyles];
}
