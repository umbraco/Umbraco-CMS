import type { UmbMediaItemModel } from '../../repository/item/types.js';
import type { UmbValueSummaryApi, UmbValueSummaryElementInterface } from '@umbraco-cms/backoffice/value-summary';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-media-start-node-value-summary')
export class UmbMediaStartNodeValueSummaryElement extends UmbLitElement implements UmbValueSummaryElementInterface {
	@property({ attribute: false })
	set api(api: UmbValueSummaryApi | undefined) {
		this.#api = api;
		if (api) {
			this.observe(api.value, (v) => (this._value = v as UmbMediaItemModel | null), 'value');
		}
	}
	get api() {
		return this.#api;
	}

	#api?: UmbValueSummaryApi;

	@state()
	private _value?: UmbMediaItemModel | null;

	override render() {
		return html`<span>${this._value?.name ?? this.localize.term('media_mediaRoot')}</span>`;
	}
}

export { UmbMediaStartNodeValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-start-node-value-summary': UmbMediaStartNodeValueSummaryElement;
	}
}
