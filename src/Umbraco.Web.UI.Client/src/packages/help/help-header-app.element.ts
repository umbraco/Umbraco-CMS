import type { CSSResultGroup } from '@umbraco-cms/backoffice/external/lit';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbHeaderAppButtonElement } from '@umbraco-cms/backoffice/components';

const elementName = 'umb-help-header-app';
@customElement(elementName)
export class UmbHelpHeaderAppElement extends UmbHeaderAppButtonElement {
	override render() {
		return html` <div>My Header App</div> `;
	}

	static override styles: CSSResultGroup = [UmbHeaderAppButtonElement.styles, css``];
}

export { UmbHelpHeaderAppElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbHelpHeaderAppElement;
	}
}
