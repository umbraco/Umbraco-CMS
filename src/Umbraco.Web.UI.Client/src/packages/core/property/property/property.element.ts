import { UmbPropertyContext } from './property.context.js';
import { css, customElement, html, property, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { createExtensionElement } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import {
	UmbBindValidationMessageToFormControl,
	UmbFormControlValidator,
	UmbObserveValidationStateController,
} from '@umbraco-cms/backoffice/validation';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorConfig,
} from '@umbraco-cms/backoffice/property-editor';
import type {
	UmbPropertyTypeAppearanceModel,
	UmbPropertyTypeValidationModel,
} from '@umbraco-cms/backoffice/content-type';
import type { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

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
	 * @attr
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
	 * @attr
	 * @default
	 */
	@property({ type: String })
	public set alias(alias: string) {
		this.#propertyContext.setAlias(alias);
	}
	public get alias() {
		return this.#propertyContext.getAlias() ?? '';
	}

	/**
	 * Property Editor UI Alias. Render the Property Editor UI registered for this alias.
	 * @public
	 * @type {string}
	 * @attr
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

	/**
	 * Config. Configuration to pass to the Property Editor UI. This is also the configuration data stored on the Data Type.
	 * @public
	 * @type {string}
	 * @attr
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
	 * @attr
	 * @default
	 */
	@property({ type: String, attribute: false })
	public set dataPath(dataPath: string | undefined) {
		this.#dataPath = dataPath;
		new UmbObserveValidationStateController(this, dataPath, (invalid) => {
			this._invalid = invalid;
		});
	}
	public get dataPath(): string | undefined {
		return this.#dataPath;
	}
	#dataPath?: string;

	@state()
	private _variantDifference?: string;

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

	#propertyContext = new UmbPropertyContext(this);

	#controlValidator?: UmbFormControlValidator;
	#validationMessageBinder?: UmbBindValidationMessageToFormControl;
	#valueObserver?: UmbObserverController<unknown>;
	#configObserver?: UmbObserverController<UmbPropertyEditorConfigCollection | undefined>;

	constructor() {
		super();

		this.observe(
			this.#propertyContext.alias,
			(alias) => {
				this._alias = alias;
			},
			null,
		);

		this.observe(
			this.#propertyContext.label,
			(label) => {
				this._label = label;
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
				this._variantDifference = variantDifference;
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
			this.#propertyContext.validation,
			(validation) => {
				this._mandatory = validation?.mandatory;
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
					this._gotEditorUI(manifest);
				},
				'_observePropertyEditorUI',
			);
		}
	}

	private async _gotEditorUI(manifest?: ManifestPropertyEditorUi | null): Promise<void> {
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
			this.#controlValidator?.destroy();
			oldElement?.removeEventListener('change', this._onPropertyEditorChange as any as EventListener);
			oldElement?.removeEventListener('property-value-change', this._onPropertyEditorChange as any as EventListener);

			this._element = el as ManifestPropertyEditorUi['ELEMENT_TYPE'];

			this.#propertyContext.setEditor(this._element);

			if (this._element) {
				this._element.addEventListener('change', this._onPropertyEditorChange as any as EventListener);
				this._element.addEventListener('property-value-change', this._onPropertyEditorChange as any as EventListener);

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

				if ('checkValidity' in this._element) {
					this.#controlValidator = new UmbFormControlValidator(this, this._element as any, this.#dataPath);
					// We trust blindly that the dataPath is available at this stage. [NL]
					if (this.#dataPath) {
						this.#validationMessageBinder = new UmbBindValidationMessageToFormControl(
							this,
							this._element as any,
							this.#dataPath,
						);
						this.#validationMessageBinder.value = this.#propertyContext.getValue();
					}
				}
			}

			this.requestUpdate('element', oldElement);
		}
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
				${this._variantDifference
					? html`<uui-tag look="secondary" slot="description">${this._variantDifference}</uui-tag>`
					: ''}
				<div slot="editor">${this._element}</div>
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
