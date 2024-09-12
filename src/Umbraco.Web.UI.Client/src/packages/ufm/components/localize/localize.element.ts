import { UmbUfmElementBase } from '../ufm-element-base.js';
import { customElement, property } from '@umbraco-cms/backoffice/external/lit';

const elementName = 'ufm-localize';

@customElement(elementName)
export class UmbUfmLocalizeElement extends UmbUfmElementBase {
	@property()
	public set alias(value: string | undefined) {
		if (!value) return;
		this.#alias = value;
		this.value = this.localize.term(value);
	}
	public get alias(): string | undefined {
		return this.#alias;
	}
	#alias?: string;
}

export { UmbUfmLocalizeElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbUfmLocalizeElement;
	}
}
