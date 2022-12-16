import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement, PropertyValueMap } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { createExtensionElement } from '@umbraco-cms/extensions-api';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import type { ManifestPropertyEditorUI, ManifestTypes } from '@umbraco-cms/models';

import '../../property-actions/shared/property-action-menu/property-action-menu.element';
import '../../workspaces/shared/workspace-property-layout/workspace-property-layout.element';

/**
 *  @element umb-entity-property
 *  @description - Component for displaying a entity property. The Element will render a Property Editor based on the Property Editor UI alias passed to the element.
 *  The element will also render all Property Actions related to the Property Editor.
 */
@customElement('umb-entity-property')
export class UmbEntityPropertyElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
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

	/**
	 * Label. Name of the property
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	@property({ type: String })
	public label = '';

	/**
	 * Description: render a description underneath the label.
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	@property({ type: String })
	public description = '';

	/**
	 * Alias
	 * @public
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	@property({ type: String })
	public alias = '';

	/**
	 * Property Editor UI Alias. Render the Property Editor UI registered for this alias.
	 * @public
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	private _propertyEditorUIAlias = '';
	@property({ type: String, attribute: 'property-editor-ui-alias' })
	public get propertyEditorUIAlias(): string {
		return this._propertyEditorUIAlias;
	}
	public set propertyEditorUIAlias(value: string) {
		this._propertyEditorUIAlias = value;
		this._observePropertyEditorUI();
	}

	/**
	 * Property Editor UI Alias. Render the Property Editor UI registered for this alias.
	 * @public
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	@property({ type: Object, attribute: false })
	public value?: any;

	/**
	 * Config. Configuration to pass to the Property Editor UI. This is also the configuration data stored on the Data Type.
	 * @public
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	@property({ type: Object, attribute: false })
	public config?: any;

	// TODO: make interface for UMBPropertyEditorElement
	@state()
	private _element?: { value?: any; config?: any } & HTMLElement; // TODO: invent interface for propertyEditorUI.

	connectedCallback(): void {
		super.connectedCallback();
		this._observePropertyEditorUI();
		this.addEventListener('property-editor-change', this._onPropertyEditorChange as any as EventListener);
	}

	private _observePropertyEditorUI() {
		this.observe<ManifestTypes>(umbExtensionsRegistry.getByAlias(this.propertyEditorUIAlias), (manifest) => {
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

	render() {
		return html`
			<umb-workspace-property-layout id="layout" label="${this.label}" description="${this.description}">
				${this._renderPropertyActionMenu()}
				<div slot="editor">${this._element}</div>
			</umb-workspace-property-layout>
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
