import { ExampleBlockViewBase } from './block-view-base.element.js';
import { exampleBlockViewStyles } from './block-view.styles.js';
import type { ExampleRichTextValue } from './block-view.utils.js';
import { css, customElement, html, unsafeHTML } from '@umbraco-cms/backoffice/external/lit';
import { sanitizeHTML } from '@umbraco-cms/backoffice/utils';

@customElement('example-text-block-view')
export class ExampleTextBlockView extends ExampleBlockViewBase {
	override render() {
		const markup = (this.content?.text as ExampleRichTextValue | undefined)?.markup ?? '';
		return html`<div class="copy">${unsafeHTML(sanitizeHTML(markup))}</div>`;
	}

	static override styles = [
		exampleBlockViewStyles,
		css`
			:host {
				border: none;
				background-color: transparent;
			}

			.copy {
				max-width: 60ch;
				padding: var(--uui-size-space-5) 0;
				font-size: 1.0625rem;
				line-height: 1.6;
			}

			.copy h1,
			.copy h2,
			.copy h3 {
				margin: 0 0 var(--uui-size-space-4);
				font-size: clamp(1.5rem, 3vw, 2.25rem);
				font-weight: 800;
				letter-spacing: -0.03em;
				line-height: 1.05;
			}

			.copy p {
				margin: 0 0 var(--uui-size-space-4);
				color: var(--uui-color-text-alt);
			}
		`,
	];
}

export default ExampleTextBlockView;

declare global {
	interface HTMLElementTagNameMap {
		'example-text-block-view': ExampleTextBlockView;
	}
}
