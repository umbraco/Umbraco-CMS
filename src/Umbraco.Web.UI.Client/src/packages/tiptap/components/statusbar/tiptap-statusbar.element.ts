import type { UmbTiptapStatusbarValue } from '../types.js';
import { css, customElement, html, nothing, property, repeat } from '@umbraco-cms/backoffice/external/lit';
import { debounce } from '@umbraco-cms/backoffice/utils';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbExtensionsElementInitializer } from '@umbraco-cms/backoffice/extension-api';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

/**
 * Provides a status bar for the {@link UmbInputTiptapElement}
 * @element umb-tiptap-statusbar
 * @cssprop --umb-tiptap-edge-border-color - Defines the edge border color
 */
@customElement('umb-tiptap-statusbar')
export class UmbTiptapStatusbarElement extends UmbLitElement {
	#attached = false;

	#debouncer = debounce(() => this.requestUpdate(), 100);

	#extensionsController?: UmbExtensionsElementInitializer;

	#lookup: Map<string, unknown> = new Map();

	@property({ type: Boolean, reflect: true })
	readonly = false;

	@property({ attribute: false })
	editor?: Editor;

	@property({ attribute: false })
	configuration?: UmbPropertyEditorConfigCollection;

	@property({ attribute: false })
	public set statusbar(value: UmbTiptapStatusbarValue) {
		if (typeof value === 'string') {
			value = [[], [value]];
		} else if (Array.isArray(value) && value.length === 1) {
			value = [[], value[0]];
		}

		this.#statusbar = value;
	}
	public get statusbar(): UmbTiptapStatusbarValue {
		return this.#statusbar;
	}
	#statusbar: UmbTiptapStatusbarValue = [[], []];

	override connectedCallback() {
		super.connectedCallback();
		this.#attached = true;
		this.#observeExtensions();
	}

	override disconnectedCallback() {
		this.#attached = false;
		this.#extensionsController?.destroy();
		this.#extensionsController = undefined;
		super.disconnectedCallback();
	}

	#observeExtensions() {
		if (!this.#attached) return;
		this.#extensionsController?.destroy();

		this.#extensionsController = new UmbExtensionsElementInitializer(
			this,
			umbExtensionsRegistry,
			'tiptapStatusbarExtension',
			(manifest) => this.statusbar.flat().includes(manifest.alias),
			(extensionControllers) => {
				extensionControllers.forEach((ext) => {
					if (!this.#lookup.has(ext.alias)) {
						(ext.component as HTMLElement)?.setAttribute('data-mark', `action:tiptap-statusbar:${ext.alias}`);
						this.#lookup.set(ext.alias, ext.component);
						this.#debouncer();
					}
				});
			},
		);

		this.#extensionsController.properties = { editor: this.editor, configuration: this.configuration };
	}

	override render() {
		if (!this.statusbar.flat().length) return nothing;
		return this.#renderAreas(this.statusbar);
	}

	#renderAreas(statusbar: UmbTiptapStatusbarValue) {
		return repeat(statusbar, (area) => html`<div class="area">${this.#renderActions(area)}</div>`);
	}

	#renderActions(aliases: Array<string>) {
		return repeat(aliases, (alias) => this.#lookup?.get(alias) ?? nothing);
	}

	static override readonly styles = css`
		:host([readonly]) {
			display: none;
		}

		:host {
			--uui-button-height: var(--uui-size-layout-2);

			display: flex;
			flex-wrap: wrap;
			align-items: center;
			justify-content: space-between;

			border-radius: var(--uui-border-radius);
			border: 1px solid var(--umb-tiptap-edge-border-color, var(--uui-color-border));
			border-top-left-radius: 0;
			border-top-right-radius: 0;
			border-top: 0;
			box-sizing: border-box;

			min-height: var(--uui-size-layout-1);
			max-height: calc(var(--uui-size-layout-2) + var(--uui-size-1));

			padding: 0 var(--uui-size-3);

			> p {
				margin: 0;
			}

			.area {
				display: inline-flex;
				flex-wrap: wrap;
				align-items: stretch;
			}
		}
	`;
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-statusbar': UmbTiptapStatusbarElement;
	}
}
