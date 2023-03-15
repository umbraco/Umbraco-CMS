import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { UmbVariantId } from '../../variants/variant-id.class';
import { UmbWorkspacePropertyContext } from './workspace-property.context';
import { createExtensionElement, umbExtensionsRegistry } from '@umbraco-cms/extensions-api';
import type { ManifestPropertyEditorUI, ManifestTypes } from '@umbraco-cms/models';

import '../../property-actions/shared/property-action-menu/property-action-menu.element';
import '../../../../backoffice/shared/components/workspace/workspace-property-layout/workspace-property-layout.element';
import { UmbObserverController } from '@umbraco-cms/observable-api';
import { UmbLitElement } from '@umbraco-cms/element';
import { DataTypePropertyModel } from '@umbraco-cms/backend-api';
import { UmbPropertyEditorElement } from '@umbraco-cms/property-editor';

/**
 *  @element umb-workspace-property
 *  @description - Component for displaying a entity property. The Element will render a Property Editor based on the Property Editor UI alias passed to the element.
 *  The element will also render all Property Actions related to the Property Editor.
 */

@customElement('umb-workspace-property')
export class UmbWorkspacePropertyElement extends UmbLitElement {
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
		`,
	];

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
	private _propertyEditorUiAlias = '';
	@property({ type: String, attribute: 'property-editor-ui-alias' })
	public set propertyEditorUiAlias(value: string) {
		if (this._propertyEditorUiAlias === value) return;
		this._propertyEditorUiAlias = value;
		this._observePropertyEditorUI();
	}

	/**
	 * Property Value, this is the value stored in the property.
	 * @public
	 * @type {unknown}
	 * @attr
	 * @default undefined
	 */
	@property({ attribute: false })
	public set value(value: unknown) {
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
	public set config(value: DataTypePropertyModel[]) {
		this._propertyContext.setConfig(value);
	}

	/**
	 * PropertyVariantId. A Variant ID to identify which variant its value is stored on.
	 * @public
	 * @type {UmbVariantId}
	 * @attr
	 * @default null
	 */
	@property({ type: Object, attribute: false })
	public set propertyVariantId(value: UmbVariantId | undefined) {
		this._propertyContext.setVariantId(value);
		//this._variantDisplayName = value?.toString();
	}

	@state()
	private _variantDifference?: string;

	@state()
	private _element?: UmbPropertyEditorElement;

	@state()
	private _value?: unknown;

	@state()
	private _alias?: string;

	@state()
	private _label?: string;

	@state()
	private _description?: string;

	private _propertyContext = new UmbWorkspacePropertyContext(this);

	private propertyEditorUIObserver?: UmbObserverController<ManifestTypes>;

	private _valueObserver?: UmbObserverController<unknown>;
	private _configObserver?: UmbObserverController<unknown>;

	constructor() {
		super();

		this.observe(this._propertyContext.alias, (alias) => {
			this._alias = alias;
		});
		this.observe(this._propertyContext.label, (label) => {
			this._label = label;
		});
		this.observe(this._propertyContext.description, (description) => {
			this._description = description;
		});
		this.observe(this._propertyContext.variantDifference, (variantDifference) => {
			this._variantDifference = variantDifference;
		});
	}

	private _onPropertyEditorChange = (e: CustomEvent) => {
		const target = e.composedPath()[0] as any;

		//this.value = target.value; // Sets value in context.
		this._propertyContext.changeValue(target.value);
		e.stopPropagation();
	};

	private _observePropertyEditorUI() {
		this.propertyEditorUIObserver?.destroy();
		this.propertyEditorUIObserver = this.observe(
			umbExtensionsRegistry.getByTypeAndAlias('propertyEditorUI', this._propertyEditorUiAlias),
			(manifest) => {
				this._gotEditorUI(manifest);
			}
		);
	}

	private _gotEditorUI(manifest?: ManifestPropertyEditorUI | null) {
		if (!manifest) {
			// TODO: if propertyEditorUiAlias didn't exist in store, we should do some nice fail UI.
			return;
		}

		createExtensionElement(manifest)
			.then((el) => {
				const oldValue = this._element;

				oldValue?.removeEventListener('change', this._onPropertyEditorChange as any as EventListener);

				this._element = el as UmbPropertyEditorElement;

				this._valueObserver?.destroy();
				this._configObserver?.destroy();

				if (this._element) {
					this._element.addEventListener('property-value-change', this._onPropertyEditorChange as any as EventListener);

					this._valueObserver = this.observe(this._propertyContext.value, (value) => {
						this._value = value;
						if (this._element) {
							this._element.value = value;
						}
					});
					this._configObserver = this.observe(this._propertyContext.config, (config) => {
						if (this._element && config) {
							this._element.config = config;
						}
					});
				}

				this.requestUpdate('element', oldValue);
			})
			.catch(() => {
				// TODO: loading JS failed so we should do some nice UI. (This does only happen if extension has a js prop, otherwise we concluded that no source was needed resolved the load.)
			});
	}

	render() {
		return html`
			<umb-workspace-property-layout
				id="layout"
				alias="${ifDefined(this._alias)}"
				label="${ifDefined(this._label)}"
				description="${ifDefined(this._description)}">
				${this._renderPropertyActionMenu()}
				${this._variantDifference
					? html`<uui-tag look="secondary" slot="description">${this._variantDifference}</uui-tag>`
					: ''}
				<div slot="editor">${this._element}</div>
			</umb-workspace-property-layout>
		`;
	}

	private _renderPropertyActionMenu() {
		return html`${this._propertyEditorUiAlias
			? html`<umb-property-action-menu
					slot="property-action-menu"
					id="property-action-menu"
					.propertyEditorUiAlias="${this._propertyEditorUiAlias}"
					.value="${this._value}"></umb-property-action-menu>`
			: ''}`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-property': UmbWorkspacePropertyElement;
	}
}
