import { UmbPropertyContext } from './property.context.js';
import { css, customElement, html, property, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { createExtensionElement, UmbExtensionsApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import {
	UmbBindServerValidationToFormControl,
	UmbFormControlValidator,
	UmbObserveValidationStateController,
} from '@umbraco-cms/backoffice/validation';
import {
	type ManifestPropertyEditorUi,
	type UmbPropertyEditorConfigCollection,
	type UmbPropertyEditorConfig,
	UMB_MISSING_PROPERTY_EDITOR_UI_UI_ALIAS,
} from '@umbraco-cms/backoffice/property-editor';
import type {
	UmbPropertyTypeAppearanceModel,
	UmbPropertyTypeValidationModel,
} from '@umbraco-cms/backoffice/content-type';
import type { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { UMB_MARK_ATTRIBUTE_NAME } from '@umbraco-cms/backoffice/const';
import { UmbRoutePathAddendumContext } from '@umbraco-cms/backoffice/router';

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
	 * @default
	 */
	@property({ type: String })
	public set label(label: string | undefined) {
		this.#propertyContext.setLabel(label);
	}
	public get label() {
		return this.#propertyContext.getLabel();
	}

	/**
	 * Description: render a description underneath the label.
	 * @type {string}
	 * @default
	 */
	@property({ type: String })
	public set description(description: string | undefined) {
		this.#propertyContext.setDescription(description);
	}
	public get description() {
		return this.#propertyContext.getDescription();
	}

	/**
	 * Appearance: Appearance settings for the property.
	 */
	@property({ type: Object, attribute: false })
	public set appearance(appearance: UmbPropertyTypeAppearanceModel | undefined) {
		this.#propertyContext.setAppearance(appearance);
	}
	public get appearance() {
		return this.#propertyContext.getAppearance();
	}

	/**
	 * Alias
	 * @public
	 * @type {string}
	 * @default
	 */
	@property({ type: String })
	public set alias(alias: string) {
		this.setAttribute(UMB_MARK_ATTRIBUTE_NAME, 'property:' + alias);
		this.#propertyContext.setAlias(alias);
	}
	public get alias() {
		return this.#propertyContext.getAlias() ?? '';
	}

	/**
	 * Property Editor UI Alias. Render the Property Editor UI registered for this alias.
	 * @public
	 * @type {string}
	 * @default
	 */
	@property({ type: String, attribute: 'property-editor-ui-alias' })
	public set propertyEditorUiAlias(value: string | undefined) {
		this._propertyEditorUiAlias = value;
		this._observePropertyEditorUI();
	}
	public get propertyEditorUiAlias(): string {
		return this._propertyEditorUiAlias ?? '';
	}
	private _propertyEditorUiAlias?: string;

	@property({ type: String, attribute: 'property-editor-data-source-alias' })
	public set propertyEditorDataSourceAlias(value: string | undefined) {
		this.#propertyContext.setEditorDataSourceAlias(value);

		if (this._element) {
			this._element.dataSourceAlias = value;
		}
	}
	public get propertyEditorDataSourceAlias(): string | undefined {
		return this.#propertyContext.getEditorDataSourceAlias();
	}

	/**
	 * Config. Configuration to pass to the Property Editor UI. This is also the configuration data stored on the Data Type.
	 * @public
	 * @type {string}
	 * @default
	 */
	@property({ type: Array, attribute: false })
	public set config(value: UmbPropertyEditorConfig | undefined) {
		this.#propertyContext.setConfig(value);
	}
	public get config(): UmbPropertyEditorConfig | undefined {
		return this.#propertyContext.getConfig();
	}

	/**
	 * Validation: Validation settings for the property.
	 */
	@property({ type: Object, attribute: false })
	public set validation(validation: UmbPropertyTypeValidationModel | undefined) {
		this.#propertyContext.setValidation(validation);
	}
	public get validation() {
		return this.#propertyContext.getValidation();
	}

	/**
	 * DataPath, declare the path to the value of the data that this property represents.
	 * @public
	 * @type {string}
	 * @default
	 */
	@property({ type: String, attribute: 'data-path' })
	public set dataPath(dataPath: string | undefined) {
		this.#propertyContext.setDataPath(dataPath);
		new UmbObserveValidationStateController(
			this,
			dataPath,
			(invalid) => {
				this._invalid = invalid;
			},
			'observeValidationState',
		);
		this.#setupControlValidation();
	}
	public get dataPath(): string | undefined {
		return this.#propertyContext.getDataPath();
	}

	/**
	 * Sets the property to readonly, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @default false
	 */
	private _readonly: boolean = false;
	@property({ type: Boolean, reflect: true })
	public set readonly(value: boolean) {
		this._readonly = value;

		const unique = 'UMB_ELEMENT';

		if (this._readonly) {
			this.#propertyContext.readOnlyState.addState({
				unique,
			});
		} else {
			this.#propertyContext.readOnlyState.removeState(unique);
		}
	}
	public get readonly(): boolean {
		return this._readonly;
	}

	@state()
	private _variantDifferenceTerm?: string;

	@state()
	private _element?: ManifestPropertyEditorUi['ELEMENT_TYPE'];

	@state()
	private _invalid?: boolean;

	@state()
	private _alias?: string;

	@state()
	private _label?: string;

	@state()
	private _description?: string;

	@state()
	private _orientation: 'horizontal' | 'vertical' = 'horizontal';

	@state()
	private _mandatory?: boolean;

	@state()
	private _supportsReadOnly: boolean = false;

	@state()
	private _isReadOnly = false;

	#propertyContext = new UmbPropertyContext(this);
	#pathAddendum = new UmbRoutePathAddendumContext(this);

	#controlValidator?: UmbFormControlValidator;
	#validationMessageBinder?: UmbBindServerValidationToFormControl;
	#valueObserver?: UmbObserverController<unknown>;
	#configObserver?: UmbObserverController<UmbPropertyEditorConfigCollection | undefined>;
	#validationMessageObserver?: UmbObserverController<string | undefined>;
	#extensionsController?: UmbExtensionsApiInitializer<any>;

	constructor() {
		super();

		this.observe(
			this.#propertyContext.alias,
			(alias) => {
				this._alias = alias;
				if (this._element) {
					this._element.alias = alias;
				}
				this.#pathAddendum.setAddendum(alias);
			},
			null,
		);

		this.observe(
			this.#propertyContext.label,
			(label) => {
				this._label = label;
				if (this._element) {
					this._element.name = label;
				}
			},
			null,
		);

		this.observe(
			this.#propertyContext.description,
			(description) => {
				this._description = description;
			},
			null,
		);

		this.observe(
			this.#propertyContext.variantDifference,
			(variantDifference) => {
				this._variantDifferenceTerm = variantDifference;
			},
			null,
		);

		this.observe(
			this.#propertyContext.appearance,
			(appearance) => {
				this._orientation = appearance?.labelOnTop ? 'vertical' : 'horizontal';
			},
			null,
		);

		this.observe(
			this.#propertyContext.validationMandatory,
			(mandatory) => {
				this._mandatory = mandatory;
				if (this._element) {
					this._element.mandatory = mandatory;
				}
			},
			null,
		);

		this.observe(
			this.#propertyContext.isReadOnly,
			(value) => {
				this._isReadOnly = value;
				if (this._element) {
					this._element.readonly = value;
					this._element.toggleAttribute('readonly', value);
				}
			},
			null,
		);
	}

	private _onPropertyEditorChange = (e: CustomEvent): void => {
		const target = e.composedPath()[0] as any;
		if (this._element !== target) {
			console.error(
				"Property Editor received a Change Event who's target is not the Property Editor Element. Do not make bubble and composed change events.",
			);
			return;
		}

		//this.value = target.value; // Sets value in context.
		this.#propertyContext.setValue(target.value);
		e.stopPropagation();
	};

	private _observePropertyEditorUI(): void {
		if (this._propertyEditorUiAlias) {
			this.observe(
				umbExtensionsRegistry.byTypeAndAlias('propertyEditorUi', this._propertyEditorUiAlias),
				(manifest) => {
					if (!manifest && this._propertyEditorUiAlias !== UMB_MISSING_PROPERTY_EDITOR_UI_UI_ALIAS) {
						this._propertyEditorUiAlias = UMB_MISSING_PROPERTY_EDITOR_UI_UI_ALIAS;
						this._observePropertyEditorUI();
						return;
					}
					this._gotEditorUI(manifest);
				},
				'_observePropertyEditorUI',
			);
		}
	}

	private async _gotEditorUI(manifest?: ManifestPropertyEditorUi | null): Promise<void> {
		if (this._element && this._element.manifest === manifest) {
			// If we already have an element and the manifest haven't changed, we don't need to do anything.
			return;
		}

		this.#extensionsController?.destroy();
		this.#propertyContext.setEditor(undefined);
		this.#propertyContext.setEditorManifest(manifest ?? undefined);

		if (!manifest) {
			return;
		}

		const el = await createExtensionElement(manifest);
		this._supportsReadOnly = manifest.meta.supportsReadOnly || false;

		if (el) {
			const oldElement = this._element;

			// cleanup:
			this.#valueObserver?.destroy();
			this.#configObserver?.destroy();
			this.#validationMessageObserver?.destroy();
			oldElement?.removeEventListener('change', this._onPropertyEditorChange as any as EventListener);
			/** @deprecated The `UmbPropertyValueChangeEvent` has been deprecated, and will be removed in Umbraco 18. [LK] */
			oldElement?.removeEventListener('property-value-change', this._onPropertyEditorChange as any as EventListener);
			oldElement?.destroy?.();

			this._element = el as ManifestPropertyEditorUi['ELEMENT_TYPE'];

			this.#propertyContext.setEditor(this._element);

			if (this._element) {
				this._element.addEventListener('change', this._onPropertyEditorChange as any as EventListener);
				/** @deprecated The `UmbPropertyValueChangeEvent` has been deprecated, and will be removed in Umbraco 18. [LK] */
				this._element.addEventListener('property-value-change', this._onPropertyEditorChange as any as EventListener);
				// No need to observe mandatory or label, as we already do so and set it on the _element if present: [NL]
				this._element.manifest = manifest;
				this._element.mandatory = this._mandatory;
				this._element.alias = this._alias;
				this._element.name = this._label;
				this._element.dataSourceAlias = this.#propertyContext.getEditorDataSourceAlias();

				// No need for a controller alias, as the clean is handled via the observer prop:
				this.#valueObserver = this.observe(
					this.#propertyContext.value,
					(value) => {
						// Set the value on the element:
						this._element!.value = value;
						if (this.#validationMessageBinder) {
							this.#validationMessageBinder.value = value;
						}
					},
					null,
				);
				this.#configObserver = this.observe(
					this.#propertyContext.config,
					(config) => {
						if (config) {
							this._element!.config = config;
						}
					},
					null,
				);
				this.#validationMessageObserver = this.observe(
					this.#propertyContext.validationMandatoryMessage,
					(mandatoryMessage) => {
						if (mandatoryMessage) {
							this._element!.mandatoryMessage = mandatoryMessage ?? undefined;
						}
					},
					null,
				);

				this.#setupControlValidation();

				this._element.readonly = this._isReadOnly;
				this._element.toggleAttribute('readonly', this._isReadOnly);

				this.#createController(manifest);
			}

			this.requestUpdate('element', oldElement);
		}
	}

	/**
	 * (Re)binds the form control validator and server-validation message binder to the current element and `dataPath`.
	 * This has to be redone whenever either changes on an already-created element — a variant switch can leave the
	 * same Property Editor UI element in place while the `dataPath` (and the data it points to) changes underneath it.
	 *
	 * Destroying the previous validator here — called synchronously from the `dataPath` setter — matters: it happens
	 * before the value for the new dataPath is pushed onto the element, so the outgoing validator can't catch that
	 * value change and misattribute it to the variant it used to represent, incorrectly clearing (or setting) that
	 * variant's validation message.
	 *
	 * The new validator, symmetrically, must not reveal an already-known invalid state until the element's value
	 * has actually caught up to `dataPath` — the value arrives asynchronously, later, via `#valueObserver` — or it
	 * risks the element re-validating the *previous* dataPath's still-present value and misattributing that stale
	 * verdict to this one. See `UmbFormControlValidator#syncControlPristine`, called from `#valueObserver` once the
	 * value is confirmed current.
	 */
	#setupControlValidation(): void {
		this.#controlValidator?.destroy();
		this.#controlValidator = undefined;
		this.#validationMessageBinder?.destroy();
		this.#validationMessageBinder = undefined;

		if (!this._element || !('checkValidity' in this._element)) return;

		const dataPath = this.dataPath;
		this.#controlValidator = new UmbFormControlValidator(this, this._element as any, dataPath);
		if (dataPath) {
			this.#validationMessageBinder = new UmbBindServerValidationToFormControl(this, this._element as any, dataPath);
			this.#validationMessageBinder.value = this.#propertyContext.getValue();
		}
	}

	#createController(propertyEditorUiManifest: ManifestPropertyEditorUi): void {
		if (this.#extensionsController) {
			this.#extensionsController.destroy();
		}

		this.#extensionsController = new UmbExtensionsApiInitializer(
			this,
			umbExtensionsRegistry,
			'propertyContext',
			[],
			(manifest) => manifest.forPropertyEditorUis.includes(propertyEditorUiManifest.alias),
		);
	}

	override render() {
		return html`
			<umb-property-layout
				id="layout"
				.alias=${this._alias ?? ''}
				.label=${this._label ?? ''}
				.description=${this._description ?? ''}
				.orientation=${this._orientation ?? 'horizontal'}
				?mandatory=${this._mandatory}
				?invalid=${this._invalid}>
				${this.#renderPropertyActionMenu()}
				${this._variantDifferenceTerm
					? html`<div id="variant-info" slot="description">
							<uui-tag look="secondary">${this.localize.term(this._variantDifferenceTerm)}</uui-tag>
						</div> `
					: ''}
				${this.#renderPropertyEditor()}
			</umb-property-layout>
		`;
	}

	#renderPropertyActionMenu() {
		if (!this._propertyEditorUiAlias) return nothing;
		return html`
			<umb-property-action-menu
				slot="action-menu"
				id="action-menu"
				.propertyEditorUiAlias=${this._propertyEditorUiAlias}>
			</umb-property-action-menu>
		`;
	}

	#renderPropertyEditor() {
		return html`
			<div id="editor" slot="editor">
				${this._isReadOnly && this._supportsReadOnly === false ? html`<div id="overlay"></div>` : nothing}
				${this._element}
			</div>
		`;
	}

	static override styles = [
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
				transition: opacity 90ms;
			}

			#layout:focus-within #action-menu,
			#layout:hover #action-menu,
			#action-menu[open] {
				opacity: 1;
			}

			#variant-info {
				opacity: 0;
				transition: opacity 90ms;
				margin-top: var(--uui-size-space-2);
				margin-left: calc(var(--uui-size-space-1) * -1);
			}

			#layout:focus-within #variant-info,
			#layout:hover #variant-info {
				opacity: 1;
			}

			#editor {
				position: relative;
			}

			#overlay {
				position: absolute;
				top: 0;
				left: 0;
				right: 0;
				bottom: 0;
				background-color: white;
				opacity: 0.5;
				z-index: 1000;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-property': UmbPropertyElement;
	}
}
