import { UMB_UFM_CONTEXT } from '../../contexts/ufm.context.js';
import { getTextFromDescendants } from '../../utils/get-text-from-descendants.function.js';
import { UmbUfmRenderContext } from './ufm-render.context.js';
import { css, customElement, nothing, property, unsafeHTML, until } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

/**
 * Fired whenever the rendered shadow DOM's text content changes, including after
 * the initial async markdown parse completes and as nested UFM custom-components
 * resolve their async content (e.g. `<ufm-content-name>` after a repository lookup).
 * Consumers wanting a kept-in-sync text snapshot should listen for this rather
 * than polling `toString()`.
 * @event umb-ufm-resolved
 */
export type UmbUfmResolvedEvent = CustomEvent<{ text: string }>;

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
	#shadowObservers = new Map<ShadowRoot, MutationObserver>();
	#lastResolvedText?: string;
	#emitScheduled = false;

	constructor() {
		super();

		this.consumeContext(UMB_UFM_CONTEXT, (ufmContext) => {
			this.#ufmContext = ufmContext;
		});
	}

	override toString(): string {
		return getTextFromDescendants(this.shadowRoot);
	}

	override connectedCallback(): void {
		super.connectedCallback();
		if (this.shadowRoot) {
			this.#installShadowObserver(this.shadowRoot);
		}
	}

	override disconnectedCallback(): void {
		super.disconnectedCallback();
		for (const observer of this.#shadowObservers.values()) {
			observer.disconnect();
		}
		this.#shadowObservers.clear();
		this.#lastResolvedText = undefined;
		this.#emitScheduled = false;
	}

	#installShadowObserver(root: ShadowRoot): void {
		if (this.#shadowObservers.has(root)) return;
		const observer = new MutationObserver((mutations) => this.#onShadowMutation(mutations));
		observer.observe(root, { childList: true, subtree: true, characterData: true });
		this.#shadowObservers.set(root, observer);
		this.#discoverDescendantShadowRoots(root);
	}

	// UFM custom-components like `<ufm-content-name>` resolve their text inside their
	// own shadow root, which is invisible to a parent's MutationObserver. We pick those
	// up by scanning each newly-added element (and its descendants) for shadow roots,
	// and tear observers down for removed shadow roots so the map doesn't grow stale.
	#onShadowMutation(mutations: MutationRecord[]): void {
		for (const mutation of mutations) {
			for (const node of mutation.addedNodes) {
				if (node.nodeType !== Node.ELEMENT_NODE) continue;
				const element = node as Element;
				if (element.shadowRoot) {
					this.#installShadowObserver(element.shadowRoot);
				}
				this.#discoverDescendantShadowRoots(element);
			}
			for (const node of mutation.removedNodes) {
				if (node.nodeType !== Node.ELEMENT_NODE) continue;
				this.#disposeDescendantShadowObservers(node as Element);
			}
		}
		this.#scheduleEmitResolved();
	}

	#disposeDescendantShadowObservers(element: Element): void {
		if (element.shadowRoot) {
			this.#shadowObservers.get(element.shadowRoot)?.disconnect();
			this.#shadowObservers.delete(element.shadowRoot);
		}
		for (const child of element.children) {
			this.#disposeDescendantShadowObservers(child);
		}
	}

	#discoverDescendantShadowRoots(root: ShadowRoot | Element): void {
		for (const child of root.children) {
			if (child.shadowRoot) {
				this.#installShadowObserver(child.shadowRoot);
			}
			this.#discoverDescendantShadowRoots(child);
		}
	}

	#scheduleEmitResolved() {
		if (this.#emitScheduled) return;
		this.#emitScheduled = true;
		queueMicrotask(() => {
			this.#emitScheduled = false;
			this.#emitResolved();
		});
	}

	#emitResolved() {
		const text = this.toString();
		if (text === this.#lastResolvedText) return;
		this.#lastResolvedText = text;
		this.dispatchEvent(
			new CustomEvent('umb-ufm-resolved', {
				detail: { text },
				bubbles: false,
				composed: false,
			}),
		);
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
