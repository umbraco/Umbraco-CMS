import { UMB_UFM_RENDER_CONTEXT } from '../ufm-render/ufm-render.context.js';
import { UmbUfmElementBase } from '../ufm-element-base.js';
import { customElement, property } from '@umbraco-cms/backoffice/external/lit';

const elementName = 'ufm-label-value';

@customElement(elementName)
export class UmbUfmLabelValueElement extends UmbUfmElementBase {
	@property()
	alias?: string;

	constructor() {
		super();

		this.consumeContext(UMB_UFM_RENDER_CONTEXT, (context) => {
			this.observe(
				context?.value,
				(value) => {
					if (this.alias !== undefined && value !== undefined && typeof value === 'object') {
						this.value = (value as Record<string, unknown>)[this.alias];
					} else {
						this.value = value;
					}
				},
				'observeValue',
			);
		});
	}
}

export { UmbUfmLabelValueElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbUfmLabelValueElement;
	}
}
