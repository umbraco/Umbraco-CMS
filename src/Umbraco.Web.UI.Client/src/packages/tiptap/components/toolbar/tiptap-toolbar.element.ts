import type { UmbTiptapToolbarValue } from '../types.js';
import { css, customElement, html, nothing, property, repeat } from '@umbraco-cms/backoffice/external/lit';
import { debounce } from '@umbraco-cms/backoffice/utils';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbExtensionsElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

import '../cascading-menu-popover/cascading-menu-popover.element.js';

/**
 * Provides a sticky toolbar for the {@link UmbInputTiptapElement}
 * @element umb-tiptap-toolbar
 * @cssprop --umb-tiptap-edge-border-color - Defines the edge border color
 * @cssprop --umb-tiptap-top - Defines the top value for the sticky toolbar
 */
@customElement('umb-tiptap-toolbar')
export class UmbTiptapToolbarElement extends UmbLitElement {
	#attached = false;

	#debouncer = debounce(() => this.requestUpdate(), 100);

	#extensionsController?: UmbExtensionsElementAndApiInitializer;

	#lookup: Map<string, unknown> = new Map();

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
				extensionControllers.forEach((ext) => {
					if (!this.#lookup.has(ext.alias)) {
						(ext.component as HTMLElement)?.setAttribute('data-mark', `action:tiptap-toolbar:${ext.alias}`);
						this.#lookup.set(ext.alias, ext.component);
						this.#debouncer();
					}
				});
			},
			undefined,
			undefined,
			() => import('../../extensions/default-tiptap-toolbar-api.js'),
		);

		this.#extensionsController.apiProperties = { configuration: this.configuration };
		this.#extensionsController.elementProperties = { editor: this.editor, configuration: this.configuration };
	}

	override render() {
		if (!this.toolbar.flat(2).length) return nothing;
		return this.#renderRows(this.toolbar);
	}

	#renderRows(rows: UmbTiptapToolbarValue) {
		return repeat(rows, (row) => html`<div class="row">${this.#renderGroups(row)}</div>`);
	}

	#renderGroups(groups: Array<Array<string>>) {
		return repeat(groups, (group) => html`<div class="group">${this.#renderActions(group)}</div>`);
	}

	#renderActions(aliases: Array<string>) {
		return repeat(aliases, (alias) => this.#lookup?.get(alias) ?? this.#renderActionPlaceholder());
	}

	#renderActionPlaceholder() {
		return html`<span class="skeleton" role="none"></span>`;
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

			border-top-color: var(--umb-tiptap-edge-border-color, var(--uui-color-border));
			border-left-color: var(--umb-tiptap-edge-border-color, var(--uui-color-border));
			border-right-color: var(--umb-tiptap-edge-border-color, var(--uui-color-border));
			box-sizing: border-box;

			background-color: var(--uui-color-surface);
			color: var(--color-text);
			font-size: var(--uui-type-default-size);

			display: flex;
			flex-direction: column;

			position: sticky;
			top: var(--umb-tiptap-top, -25px);
			left: 0;
			right: 0;
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

			min-height: var(--uui-size-12, 36px);

			.group {
				display: inline-flex;
				flex-wrap: wrap;
				align-items: stretch;

				min-height: var(--uui-size-12, 36px);

				&:not(:last-child)::after {
					content: '';
					background-color: var(--uui-color-border);
					width: 1px;
					place-self: center;
					height: var(--uui-size-7, 21px);
					margin: 0 var(--uui-size-3);
				}
			}
		}

		.skeleton {
			background-color: var(--uui-color-background);
			height: var(--uui-size-12, 36px);
			width: var(--uui-size-10, 30px);
			margin-left: 1px;
		}
	`;
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-toolbar': UmbTiptapToolbarElement;
	}
}
