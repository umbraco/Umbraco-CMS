import { UUITextStyles } from '@umbraco-ui/uui';
import { CSSResultGroup, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';

import { createExtensionElement } from '../../../core/extension';
import type { ManifestEntityAction } from '../../../core/models';

@customElement('umb-tree-action')
export class UmbTreeActionElement extends LitElement {
	static styles: CSSResultGroup = [UUITextStyles];

	private _treeAction?: ManifestEntityAction;
	@property({ type: Object })
	public get treeAction(): ManifestEntityAction | undefined {
		return this._treeAction;
	}
	public set treeAction(value: ManifestEntityAction | undefined) {
		this._treeAction = value;
		this._createElement();
	}

	@state()
	private _element?: UmbTreeActionElement;

	private async _createElement() {
		if (!this.treeAction) return;

		try {
			this._element = (await createExtensionElement(this.treeAction)) as UmbTreeActionElement | undefined;
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
		'umb-tree-action': UmbTreeActionElement;
	}
}
