import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-translation-section')
export class UmbTranslationSection extends LitElement {
	static styles = [UUITextStyles];

	render() {
		return html`<umb-section></umb-section>`;
	}
}

export default UmbTranslationSection;

declare global {
	interface HTMLElementTagNameMap {
		'umb-translation-section': UmbTranslationSection;
	}
}
