import { getDisplayStateFromUserStatus } from '../../utils.js';
import { customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UserStateModel } from '@umbraco-cms/backoffice/external/backend-api';

@customElement('umb-user-state-value-minimal-display')
export class UmbUserStateValueMinimalDisplayElement extends UmbLitElement {
	@property({ attribute: false })
	value?: UserStateModel | null;

	override render() {
		if (!this.value || this.value === 'Active') return nothing;
		const displayState = getDisplayStateFromUserStatus(this.value);
		if (!displayState) return nothing;
		return html`<uui-tag size="s" look="${displayState.look}" color="${displayState.color}"> ${this.value} </uui-tag>`;
	}
}

export { UmbUserStateValueMinimalDisplayElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-state-value-minimal-display': UmbUserStateValueMinimalDisplayElement;
	}
}
