import { UUITextStyles } from '@umbraco-ui/uui';
import { CSSResultGroup, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';

import { createExtensionElement } from '../../../core/extension';
import type { ManifestTreeItemAction } from '../../../core/models';

@customElement('umb-tree-item-action-extension')
export class UmbTreeItemActionExtensionElement extends LitElement {
	static styles: CSSResultGroup = [UUITextStyles];

	private _treeAction?: ManifestTreeItemAction;
	@property({ type: Object })
	public get treeAction(): ManifestTreeItemAction | undefined {
		return this._treeAction;
	}
	public set treeAction(value: ManifestTreeItemAction | undefined) {
		this._treeAction = value;
		this._createElement();
	}

	@state()
	private _element?: UmbTreeItemActionExtensionElement;

	private async _createElement() {
		if (!this.treeAction) return;

		try {
			this._element = (await createExtensionElement(this.treeAction)) as UmbTreeItemActionExtensionElement | undefined;
			if (!this._element) return;

			this._element.treeAction = this.treeAction;
		} catch (error) {
			// TODO: loading JS failed so we should do some nice UI. (This does only happen if extension has a js prop, otherwise we concluded that no source was needed resolved the load.)
		}
	}

	render() {
		return html`${this._element}`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-item-action-extension': UmbTreeItemActionExtensionElement;
	}
}
