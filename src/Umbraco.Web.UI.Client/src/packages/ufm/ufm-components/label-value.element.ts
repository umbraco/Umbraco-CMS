import { UMB_UFM_RENDER_CONTEXT } from '../components/ufm-render/index.js';
import { customElement, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const elementName = 'ufm-label-value';

@customElement(elementName)
export class UmbUfmLabelValueElement extends UmbLitElement {
	@property()
	alias?: string;

	@state()
	private _value?: unknown;

	constructor() {
		super();

		this.consumeContext(UMB_UFM_RENDER_CONTEXT, (context) => {
			this.observe(
				context.value,
				(value) => {
					if (this.alias !== undefined && value !== undefined && typeof value === 'object') {
						this._value = (value as Record<string, unknown>)[this.alias];
					} else {
						this._value = value;
					}
				},
				'observeValue',
			);
		});
	}

	override render() {
		return this._value !== undefined ? this._value : nothing;
	}
}

export { UmbUfmLabelValueElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbUfmLabelValueElement;
	}
}
