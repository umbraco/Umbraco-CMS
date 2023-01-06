import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { UmbWorkspacePropertyContext } from './workspace-property.context';
import { createExtensionElement } from '@umbraco-cms/extensions-api';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import type { DataTypePropertyData, ManifestPropertyEditorUI, ManifestTypes } from '@umbraco-cms/models';

import '../../property-actions/shared/property-action-menu/property-action-menu.element';
import 'src/backoffice/shared/components/workspace/workspace-property-layout/workspace-property-layout.element';
import { UmbObserverController } from 'src/core/observable-api/observer.controller';
import { UmbLitElement } from '@umbraco-cms/element';

/**
 *  @element umb-property
 *  @description - Component for displaying a entity property. The Element will render a Property Editor based on the Property Editor UI alias passed to the element.
 *  The element will also render all Property Actions related to the Property Editor.
 */

@customElement('umb-property')
export class UmbPropertyElement extends UmbLitElement {
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

	@state()
	private _label?:string;

	@state()
	private _description?:string;

	/**
	 * Label. Name of the property
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	@property({ type: String })
	public set label(label: string) {
		this._propertyContext.setLabel(label);
	}

	/**
	 * Description: render a description underneath the label.
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	@property({ type: String })
	public set description(description: string) {
		this._propertyContext.setDescription(description);
	}

	/**
	 * Alias
	 * @public
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	@property({ type: String })
	public set alias(alias: string) {
		this._propertyContext.setAlias(alias);
	}

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
	 * @type {object}
	 * @attr
	 * @default undefined
	 */
	@property({ type: Object, attribute: false })
	public set value(value: object | string) {
		this._propertyContext.setValue(value);
	}

	/**
	 * Config. Configuration to pass to the Property Editor UI. This is also the configuration data stored on the Data Type.
	 * @public
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	@property({ type: Object, attribute: false })
	public set config(value: DataTypePropertyData[]) {
		this._propertyContext.setConfig(value);
	}

	// TODO: make interface for UMBPropertyEditorElement
	@state()
	private _element?: { value?: any; config?: any } & HTMLElement; // TODO: invent interface for propertyEditorUI.


	private _propertyContext = new UmbWorkspacePropertyContext(this);

	private propertyEditorUIObserver?: UmbObserverController<ManifestTypes>;


	constructor() {
		super();


		this.observe(this._propertyContext.label, (label) => {
			console.log("_propertyContext replied with label", label)
			this._label = label;
		});
		this.observe(this._propertyContext.label, (description) => {
			this._description = description;
		});

		// TODO: move event to context. maybe rename to property-editor-value-change.
		this.addEventListener('property-editor-change', this._onPropertyEditorChange as any as EventListener);


	}

	private _observePropertyEditorUI() {
		this.propertyEditorUIObserver?.destroy();
		this.propertyEditorUIObserver = this.observe(umbExtensionsRegistry.getByTypeAndAlias('propertyEditorUI', this.propertyEditorUIAlias), (manifest) => {
			this._gotEditorUI(manifest);
		});
	}

	private _gotEditorUI(manifest?: ManifestPropertyEditorUI | null) {
		if (!manifest) {
			// TODO: if propertyEditorUIAlias didn't exist in store, we should do some nice fail UI.
			return;
		}

		createExtensionElement(manifest)
			.then((el) => {
				const oldValue = this._element;
				this._element = el;

				this.observe(this._propertyContext.value, (value) => {
					if(this._element) {
						this._element.value = value;
					}
				});
				this.observe(this._propertyContext.config, (config) => {
					if(this._element) {
						this._element.config = config;
					}
				});

				this.requestUpdate('element', oldValue);
				
			})
			.catch(() => {
				// TODO: loading JS failed so we should do some nice UI. (This does only happen if extension has a js prop, otherwise we concluded that no source was needed resolved the load.)
			});
	}



	private _onPropertyEditorChange = (e: CustomEvent) => {
		const target = e.composedPath()[0] as any;
		this.value = target.value;

		// TODO: update context.
		//TODO: Property-Context: Figure out the requirements for this. Cause currently the alias-prop(getter) is required, but its not obvious.

		this.dispatchEvent(new CustomEvent('property-value-change', { bubbles: true, composed: true }));
		e.stopPropagation();
	};

	render() {
		return html`
			<umb-workspace-property-layout id="layout" label="${ifDefined(this._label)}" description="${ifDefined(this._description)}">
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
					.propertyEditorUIAlias="${this._propertyEditorUIAlias}"
					.value="${this.value}"></umb-property-action-menu>`
			: ''}`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-property': UmbPropertyElement;
	}
}
