import { getDisplayStateFromUserStatus } from '../../utils.js';
import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';
import type { UserStateModel } from '@umbraco-cms/backoffice/external/backend-api';

@customElement('umb-user-state-value-summary')
export class UmbUserStateValueSummaryElement extends UmbValueSummaryElementBase<UserStateModel | null> {
	override render() {
		if (!this._value || this._value === 'Active') return nothing;
		const displayState = getDisplayStateFromUserStatus(this._value);
		if (!displayState) return nothing;
		return html`<uui-tag size="s" look="${displayState.look}" color="${displayState.color}">
			${this._value}
		</uui-tag>`;
	}
}

export { UmbUserStateValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-state-value-summary': UmbUserStateValueSummaryElement;
	}
}
