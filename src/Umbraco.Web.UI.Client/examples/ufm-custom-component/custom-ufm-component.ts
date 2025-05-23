import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbUfmComponentBase } from '@umbraco-cms/backoffice/ufm';
import type { UfmToken } from '@umbraco-cms/backoffice/ufm';

export class UmbCustomUfmComponent extends UmbUfmComponentBase {
	render(token: UfmToken) {
		// You could do further regular expression/text processing,
		// then output your custom HTML markup.
		return `<ufm-custom-component text="${token.text}"></ufm-custom-component>`;
	}
}

// eslint-disable-next-line local-rules/enforce-umb-prefix-on-element-name
@customElement('ufm-custom-component')
export class UmbCustomUfmComponentElement extends UmbLitElement {
	@property()
	text?: string;

	override render() {
		return html`<marquee>${this.text}</marquee>`;
	}
}

export { UmbCustomUfmComponent as api };
export { UmbCustomUfmComponentElement as element };
