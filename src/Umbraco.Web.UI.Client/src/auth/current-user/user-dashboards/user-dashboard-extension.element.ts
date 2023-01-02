import { CSSResultGroup, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { createExtensionElement } from '@umbraco-cms/extensions-api';
import type { ManifestUserDashboard } from '@umbraco-cms/models';

@customElement('umb-user-dashboard-extension')
export class UmbUserDashboardExtensionElement extends LitElement {
	static styles: CSSResultGroup = [UUITextStyles];

	private _userDashboard?: ManifestUserDashboard;

	@property({ type: Object })
	public get userDashboard(): ManifestUserDashboard | undefined {
		return this._userDashboard;
	}
	public set userDashboard(value: ManifestUserDashboard | undefined) {
		this._userDashboard = value;
		this._createElement();
	}

	@state()
	private _element?: any;

	private async _createElement() {
		if (!this.userDashboard) return;

		try {
			this._element = (await createExtensionElement(this.userDashboard)) as any | undefined;
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
		'umb-user-dashboard-extension': UmbUserDashboardExtensionElement;
	}
}
