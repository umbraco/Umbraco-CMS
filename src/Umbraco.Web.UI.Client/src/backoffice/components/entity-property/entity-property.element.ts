import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement, PropertyValueMap } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { Subscription } from 'rxjs';

import { UmbContextConsumerMixin } from '../../../core/context';
import { createExtensionElement, UmbExtensionRegistry } from '../../../core/extension';
import type { ManifestPropertyEditorUI } from '../../../core/models';

import '../../property-actions/shared/property-action-menu/property-action-menu.element';
import '../../editors/shared/editor-property-layout/editor-property-layout.element';

@customElement('umb-entity-property')
export class UmbEntityPropertyElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
			}

			p {
				color: var(--uui-color-text-alt);
			}

			#property-action-menu {
				opacity: 0;
			}

			#layout:focus-within #property-action-menu,
			#layout:hover #property-action-menu,
			#property-action-menu[open] {
				opacity: 1;
			}

			hr {
				border: 0;
				border-top: 1px solid var(--uui-color-border);
			}
		`,
	];

	@property({ type: String })
	public label = '';

	@property({ type: String })
	public description = '';

	@property({ type: String })
	public alias = '';

	private _propertyEditorUIAlias = '';
	@property({ type: String, attribute: 'property-editor-ui-alias' })
	public get propertyEditorUIAlias(): string {
		return this._propertyEditorUIAlias;
	}
	public set propertyEditorUIAlias(value: string) {
		this._propertyEditorUIAlias = value;
		this._observePropertyEditorUI();
	}

	@property({ type: Object, attribute: false })
	value?: any;

	@property({ type: Object, attribute: false })
	config?: any;

	// TODO: make interface for UMBPropertyEditorElement
	@state()
	private _element?: { value?: any; config?: any } & HTMLElement; // TODO: invent interface for propertyEditorUI.

	private _extensionRegistry?: UmbExtensionRegistry;
	private _propertyEditorUISubscription?: Subscription;

	constructor() {
		super();

		this.consumeContext('umbExtensionRegistry', (_instance: UmbExtensionRegistry) => {
			this._extensionRegistry = _instance;
			this._observePropertyEditorUI();
		});
	}

	connectedCallback(): void {
		super.connectedCallback();
		this.addEventListener('property-editor-change', this._onPropertyEditorChange as any as EventListener);
	}

	private _observePropertyEditorUI() {
		if (!this._extensionRegistry) return;

		this._propertyEditorUISubscription?.unsubscribe();

		this._propertyEditorUISubscription = this._extensionRegistry
			.getByAlias(this.propertyEditorUIAlias)
			.subscribe((manifest) => {
				if (manifest?.type === 'propertyEditorUI') {
					this._gotData(manifest);
				}
			});
	}

	private _gotData(propertyEditorUIManifest?: ManifestPropertyEditorUI) {
		if (!propertyEditorUIManifest) {
			// TODO: if dataTypeKey didn't exist in store, we should do some nice UI.
			return;
		}

		createExtensionElement(propertyEditorUIManifest)
			.then((el) => {
				const oldValue = this._element;
				this._element = el;

				if (this._element) {
					this._element.value = this.value; // Be aware its duplicated code
					this._element.config = this.config; // Be aware its duplicated code
				}
				this.requestUpdate('element', oldValue);
			})
			.catch(() => {
				// TODO: loading JS failed so we should do some nice UI. (This does only happen if extension has a js prop, otherwise we concluded that no source was needed resolved the load.)
			});
	}

	private _onPropertyEditorChange = (e: CustomEvent) => {
		const target = e.composedPath()[0] as any;
		this.value = target.value;
		this.dispatchEvent(new CustomEvent('property-value-change', { bubbles: true, composed: true }));
		e.stopPropagation();
	};

	/** Lit does not currently handle dynamic tag names, therefor we are doing some manual rendering */
	// TODO: Refactor into a base class for dynamic-tag element? we will be using this a lot for extensions.
	// This could potentially hook into Lit and parse all properties defined in the specific class on to the dynamic-element. (see static elementProperties: PropertyDeclarationMap;)
	willUpdate(changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>) {
		super.willUpdate(changedProperties);

		if (changedProperties.has('value') && this._element) {
			this._element.value = this.value; // Be aware its duplicated code
		}

		if (changedProperties.has('config') && this._element) {
			this._element.config = this.config; // Be aware its duplicated code
		}
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._propertyEditorUISubscription?.unsubscribe();
	}

	render() {
		return html`
			<umb-editor-property-layout id="layout" label="${this.label}" description="${this.description}">
				${this._renderPropertyActionMenu()}
				<div slot="editor">${this._element}</div>
			</umb-editor-property-layout>
			<hr />
		`;
	}

	private _renderPropertyActionMenu() {
		return html`${this.propertyEditorUIAlias
			? html`<umb-property-action-menu
					slot="property-action-menu"
					id="property-action-menu"
					.propertyEditorUIAlias="${this.propertyEditorUIAlias}"
					.value="${this.value}"></umb-property-action-menu>`
			: ''}`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-property': UmbEntityPropertyElement;
	}
}
