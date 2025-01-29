import { UmbUfmElementBase } from '../ufm-element-base.js';
import { customElement, property } from '@umbraco-cms/backoffice/external/lit';

// eslint-disable-next-line local-rules/enforce-umb-prefix-on-element-name
@customElement('ufm-localize')
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
		'ufm-localize': UmbUfmLocalizeElement;
	}
}
