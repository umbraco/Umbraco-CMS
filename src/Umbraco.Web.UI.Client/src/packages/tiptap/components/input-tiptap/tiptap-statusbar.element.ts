import type { UmbTiptapStatusbarValue } from '../types.js';
import { css, customElement, html, map, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbExtensionsElementInitializer } from '@umbraco-cms/backoffice/extension-api';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-tiptap-statusbar')
export class UmbTiptapStatusbarElement extends UmbLitElement {
	#attached = false;
	#extensionsController?: UmbExtensionsElementInitializer;

	@state()
	private _lookup?: Map<string, unknown>;

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
				this._lookup = new Map(
					extensionControllers.map((ext) => {
						(ext.component as HTMLElement)?.setAttribute('data-mark', `action:tiptap-statusbar:${ext.alias}`);
						return [ext.alias, ext.component];
					}),
				);
			},
		);

		this.#extensionsController.properties = { editor: this.editor, configuration: this.configuration };
	}

	override render() {
		if (!this.statusbar.flat().length) return nothing;
		return map(
			this.statusbar,
			(area) => html`<div class="area">${map(area, (alias) => this._lookup?.get(alias) ?? nothing)}</div>`,
		);
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
			border: 1px solid var(--uui-color-border);
			border-top-left-radius: 0;
			border-top-right-radius: 0;
			border-top: 0;

			min-height: var(--uui-size-layout-1);
			max-height: var(--uui-size-layout-2);

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
