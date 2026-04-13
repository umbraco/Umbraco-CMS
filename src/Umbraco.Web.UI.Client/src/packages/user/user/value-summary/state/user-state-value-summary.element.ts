import { getDisplayStateFromUserStatus } from '../../utils.js';
import type { UmbValueSummaryApi, UmbValueSummaryElement } from '@umbraco-cms/backoffice/value-summary';
import { customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UserStateModel } from '@umbraco-cms/backoffice/external/backend-api';

@customElement('umb-user-state-value-summary')
export class UmbUserStateValueSummaryElement extends UmbLitElement implements UmbValueSummaryElement {
	@property({ attribute: false })
	set api(api: UmbValueSummaryApi | undefined) {
		this.#api = api;
		if (api) {
			this.observe(api.value, (v) => (this._value = v as UserStateModel | null), 'value');
		}
	}
	get api() {
		return this.#api;
	}

	#api?: UmbValueSummaryApi;

	@state()
	private _value?: UserStateModel | null;

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
