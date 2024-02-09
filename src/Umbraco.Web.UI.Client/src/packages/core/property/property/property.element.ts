import { UmbPropertyContext } from './property.context.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { createExtensionElement } from '@umbraco-cms/backoffice/extension-api';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorConfig,
} from '@umbraco-cms/backoffice/property-editor';

/**
 *  @element umb-property
 *  @description Component for displaying a property with editor from extension registry.
 *	The Element will render a Property Editor based on the Property Editor UI alias passed to the element.
 *  This will also render all Property Actions related to the Property Editor UI Alias.
 */

@customElement('umb-property')
export class UmbPropertyElement extends UmbLitElement {
	/**
	 * Label. Name of the property
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	@property({ type: String })
	public set label(label: string) {
		this.#propertyContext.setLabel(label);
	}

	/**
	 * Description: render a description underneath the label.
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	@property({ type: String })
	public set description(description: string) {
		this.#propertyContext.setDescription(description);
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
		this.#propertyContext.setAlias(alias);
	}

	/**
	 * Property Editor UI Alias. Render the Property Editor UI registered for this alias.
	 * @public
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	@property({ type: String, attribute: 'property-editor-ui-alias' })
	public set propertyEditorUiAlias(value: string) {
		if (this._propertyEditorUiAlias === value) return;
		this._propertyEditorUiAlias = value;
		this._observePropertyEditorUI();
	}
	private _propertyEditorUiAlias = '';

	/**
	 * Config. Configuration to pass to the Property Editor UI. This is also the configuration data stored on the Data Type.
	 * @public
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	@property({ type: Array, attribute: false })
	public set config(value: UmbPropertyEditorConfig | undefined) {
		this.#propertyContext.setConfig(value);
	}

	@state()
	private _variantDifference?: string;

	@state()
	private _element?: ManifestPropertyEditorUi['ELEMENT_TYPE'];

	@state()
	private _value?: unknown;

	@state()
	private _alias?: string;

	@state()
	private _label?: string;

	@state()
	private _description?: string;

	#propertyContext = new UmbPropertyContext(this);

	#valueObserver?: UmbObserverController<unknown>;
	#configObserver?: UmbObserverController<UmbPropertyEditorConfigCollection | undefined>;

	constructor() {
		super();

		this.observe(this.#propertyContext.alias, (alias) => {
			this._alias = alias;
		});
		this.observe(this.#propertyContext.label, (label) => {
			this._label = label;
		});
		this.observe(this.#propertyContext.description, (description) => {
			this._description = description;
		});
		this.observe(this.#propertyContext.variantDifference, (variantDifference) => {
			this._variantDifference = variantDifference;
		});
	}

	private _onPropertyEditorChange = (e: CustomEvent) => {
		const target = e.composedPath()[0] as any;

		//this.value = target.value; // Sets value in context.
		this.#propertyContext.setValue(target.value);
		e.stopPropagation();
	};

	private _observePropertyEditorUI() {
		this.observe(
			umbExtensionsRegistry.byTypeAndAlias('propertyEditorUi', this._propertyEditorUiAlias),
			(manifest) => {
				this._gotEditorUI(manifest);
			},
			'_observePropertyEditorUI',
		);
	}

	private async _gotEditorUI(manifest?: ManifestPropertyEditorUi | null) {
		this.#propertyContext.setEditor(undefined);

		if (!manifest) {
			// TODO: if propertyEditorUiAlias didn't exist in store, we should do some nice fail UI.
			return;
		}

		const el = await createExtensionElement(manifest);

		if (el) {
			const oldElement = this._element;

			// cleanup:
			this.#valueObserver?.destroy();
			this.#configObserver?.destroy();
			oldElement?.removeEventListener('property-value-change', this._onPropertyEditorChange as any as EventListener);

			this._element = el as ManifestPropertyEditorUi['ELEMENT_TYPE'];

			this.#propertyContext.setEditor(this._element);

			if (this._element) {
				// TODO: Could this be changed to change event? (or additionally support change?)
				this._element.addEventListener('property-value-change', this._onPropertyEditorChange as any as EventListener);

				// No need for a controller alias, as the clean is handled via the observer prop:
				this.#valueObserver = this.observe(this.#propertyContext.value, (value) => {
					this._value = value;
					if (this._element) {
						this._element.value = value;
					}
				});
				this.#configObserver = this.observe(this.#propertyContext.config, (config) => {
					if (this._element && config) {
						this._element.config = config;
					}
				});
			}

			this.requestUpdate('element', oldElement);
		}
	}

	render() {
		return html`
			<umb-property-layout
				id="layout"
				alias="${ifDefined(this._alias)}"
				label="${ifDefined(this._label)}"
				description="${ifDefined(this._description)}">
				${this._renderPropertyActionMenu()}
				${this._variantDifference
					? html`<uui-tag look="secondary" slot="description">${this._variantDifference}</uui-tag>`
					: ''}
				<div slot="editor">${this._element}</div>
			</umb-property-layout>
		`;
	}

	private _renderPropertyActionMenu() {
		return html`${this._propertyEditorUiAlias
			? html`<umb-property-action-menu
					slot="action-menu"
					id="action-menu"
					.propertyEditorUiAlias=${this._propertyEditorUiAlias}
					.value=${this._value}></umb-property-action-menu>`
			: ''}`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
			}

			p {
				color: var(--uui-color-text-alt);
			}

			#action-menu {
				opacity: 0;
			}

			#layout:focus-within #action-menu,
			#layout:hover #action-menu,
			#action-menu[open] {
				opacity: 1;
			}

			uui-tag {
				margin-top: var(--uui-size-space-4);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-property': UmbPropertyElement;
	}
}
