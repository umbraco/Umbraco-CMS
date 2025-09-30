import { UMB_UFM_CONTEXT } from '../../contexts/ufm.context.js';
import { UmbUfmRenderContext } from './ufm-render.context.js';
import { css, customElement, nothing, property, unsafeHTML, until } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-ufm-render')
export class UmbUfmRenderElement extends UmbLitElement {
	#context = new UmbUfmRenderContext(this);

	@property({ type: Boolean })
	inline = false;

	@property()
	markdown?: string;

	// No reactive property declaration because it's causing a re-render that is not needed.
	// This just works as a shortcut to set the values on the context. [NL]
	public set value(value: string | unknown | undefined) {
		this.#context.setValue(value);
	}
	public get value(): string | unknown | undefined {
		return this.#context.getValue();
	}

	#ufmContext?: typeof UMB_UFM_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_UFM_CONTEXT, (ufmContext) => {
			this.#ufmContext = ufmContext;
		});
	}

	override toString(): string {
		return this.shadowRoot?.textContent ?? '';
	}

	override render() {
		return until(this.#renderMarkdown());
	}

	async #renderMarkdown() {
		if (!this.#ufmContext || !this.markdown) return null;
		const markup = await this.#ufmContext.parse(this.markdown, this.inline);
		return markup ? unsafeHTML(markup) : nothing;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				position: relative;
			}

			* {
				max-width: 100%;
				word-wrap: break-word;
			}

			pre {
				overflow: auto;
			}

			code {
				font-family: var(--uui-font-monospace, monospace);
				white-space: pre-wrap;
				background-color: var(--uui-color-background);
				border-radius: var(--uui-border-radius);
				padding: 0.2em 0.4em;
			}

			:host > :first-child {
				margin-block-start: 0;
			}

			:host > :last-child {
				margin-block-end: 0;
			}
		`,
	];
}

export { UmbUfmRenderElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-ufm-render': UmbUfmRenderElement;
	}
}
