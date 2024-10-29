import type { UmbTiptapToolbarValue } from '../types.js';
import { css, customElement, html, map, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbExtensionsElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

import '../toolbar/tiptap-toolbar-dropdown-base.element.js';

const elementName = 'umb-tiptap-toolbar';

@customElement(elementName)
export class UmbTiptapToolbarElement extends UmbLitElement {
	#attached = false;
	#extensionsController?: UmbExtensionsElementAndApiInitializer;

	@state()
	private _lookup?: Map<string, unknown>;

	@property({ type: Boolean, reflect: true })
	readonly = false;

	@property({ attribute: false })
	editor?: Editor;

	@property({ attribute: false })
	configuration?: UmbPropertyEditorConfigCollection;

	@property({ attribute: false })
	toolbar: UmbTiptapToolbarValue = [[[]]];

	override connectedCallback(): void {
		super.connectedCallback();
		this.#attached = true;
		this.#observeExtensions();
	}
	override disconnectedCallback(): void {
		this.#attached = false;
		this.#extensionsController?.destroy();
		this.#extensionsController = undefined;
		super.disconnectedCallback();
	}

	#observeExtensions(): void {
		if (!this.#attached) return;
		this.#extensionsController?.destroy();

		this.#extensionsController = new UmbExtensionsElementAndApiInitializer(
			this,
			umbExtensionsRegistry,
			'tiptapToolbarExtension',
			[],
			(manifest) => this.toolbar.flat(2).includes(manifest.alias),
			(extensionControllers) => {
				this._lookup = new Map(extensionControllers.map((ext) => [ext.alias, ext.component]));
			},
		);

		this.#extensionsController.apiProperties = { configuration: this.configuration };
		this.#extensionsController.elementProperties = { editor: this.editor, configuration: this.configuration };
	}

	override render() {
		return html`${map(this.toolbar, (row, rowIndex) =>
			map(
				row,
				(group, groupIndex) => html`
					${map(group, (alias, aliasIndex) => {
						const newRow = rowIndex !== 0 && groupIndex === 0 && aliasIndex === 0;
						const component = this._lookup?.get(alias);
						if (!component) return nothing;
						return html`
							<div class="item" ?data-new-row=${newRow} style=${newRow ? 'grid-column: 1 / span 3' : ''}>
								${component}
							</div>
						`;
					})}
					<div class="separator"></div>
				`,
			),
		)} `;
	}

	static override readonly styles = css`
		:host([readonly]) {
			pointer-events: none;
			background-color: var(--uui-color-surface-alt);
		}

		:host {
			border-radius: var(--uui-border-radius);
			border: 1px solid var(--uui-color-border);
			border-bottom-left-radius: 0;
			border-bottom-right-radius: 0;
			box-shadow:
				0 2px 2px -2px rgba(34, 47, 62, 0.1),
				0 8px 8px -4px rgba(34, 47, 62, 0.07);

			background-color: var(--uui-color-surface-alt);
			color: var(--color-text);
			display: grid;
			grid-template-columns: repeat(auto-fill, 10px);
			grid-auto-flow: row;

			position: sticky;
			top: -25px;
			left: 0px;
			right: 0px;
			padding: var(--uui-size-space-3);
			z-index: 9999999;
		}

		.item {
			grid-column: span 3;
		}

		.separator {
			background-color: var(--uui-color-border);
			width: 1px;
			place-self: center;
			height: 22px;
		}
		.separator:last-child,
		.separator:has(+ [data-new-row]) {
			display: none;
		}
	`;
}

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbTiptapToolbarElement;
	}
}
