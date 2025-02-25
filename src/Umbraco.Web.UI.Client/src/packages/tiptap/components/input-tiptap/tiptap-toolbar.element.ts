import type { UmbTiptapToolbarValue } from '../types.js';
import { css, customElement, html, map, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbExtensionsElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

import '../cascading-menu-popover/cascading-menu-popover.element.js';

@customElement('umb-tiptap-toolbar')
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
			undefined,
			undefined,
			() => import('../toolbar/default-tiptap-toolbar-element.api.js'),
		);

		this.#extensionsController.apiProperties = { configuration: this.configuration };
		this.#extensionsController.elementProperties = { editor: this.editor, configuration: this.configuration };
	}

	override render() {
		return html`
			${map(
				this.toolbar,
				(row) => html`
					<div class="row">
						${map(
							row,
							(group) => html`<div class="group">${map(group, (alias) => this._lookup?.get(alias) ?? nothing)}</div>`,
						)}
					</div>
				`,
			)}
		`;
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

			background-color: var(--uui-color-surface);
			color: var(--color-text);
			font-size: var(--uui-type-default-size);

			display: flex;
			flex-direction: column;

			position: sticky;
			top: -25px;
			left: 0px;
			right: 0px;
			padding: var(--uui-size-3);
			z-index: 9999999;

			box-shadow:
				0 2px 2px -2px rgba(34, 47, 62, 0.1),
				0 8px 8px -4px rgba(34, 47, 62, 0.07);
		}

		.row {
			display: flex;
			flex-direction: row;
			flex-wrap: wrap;

			.group {
				display: inline-flex;
				flex-wrap: wrap;
				align-items: stretch;

				&:not(:last-child)::after {
					content: '';
					background-color: var(--uui-color-border);
					width: 1px;
					place-self: center;
					height: 22px;
					margin: 0 var(--uui-size-3);
				}
			}
		}
	`;
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-toolbar': UmbTiptapToolbarElement;
	}
}
