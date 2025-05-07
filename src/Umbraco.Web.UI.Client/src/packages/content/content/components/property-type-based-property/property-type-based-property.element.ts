import { UmbContentPropertyContext } from '../../content-property.context.js';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';
import { css, customElement, html, ifDefined, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbDataTypeDetailRepository } from '@umbraco-cms/backoffice/data-type';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbDataTypeDetailModel } from '@umbraco-cms/backoffice/data-type';
import type { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { UMB_UNSUPPORTED_EDITOR_SCHEMA_ALIASES } from '@umbraco-cms/backoffice/property';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';

@customElement('umb-property-type-based-property')
export class UmbPropertyTypeBasedPropertyElement extends UmbLitElement {
	@property({ type: Object, attribute: false })
	public set property(value: UmbPropertyTypeModel | undefined) {
		const oldProperty = this._property;
		this._property = value;
		if (this._property?.dataType.unique !== oldProperty?.dataType.unique) {
			this._observeDataType(this._property?.dataType.unique);
		}
	}
	public get property(): UmbPropertyTypeModel | undefined {
		return this._property;
	}
	private _property?: UmbPropertyTypeModel;

	@property({ type: String, attribute: 'data-path' })
	public dataPath?: string;

	@property({ type: String })
	public get ownerEntityType(): string | undefined {
		return this._ownerEntityType;
	}
	public set ownerEntityType(value: string | undefined) {
		// Change this to ownerSchemaEditorAlias and retrieve the correct information.
		this._ownerEntityType = value;
	}

	private _ownerEntityType?: string;

	/**
	 * Sets the property to readonly, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	public readonly: boolean = false;

	@state()
	private _propertyEditorUiAlias?: string;

	@state()
	private _propertyEditorSchemaAlias?: string;

	@state()
	private _isUnsupported?: boolean;

	@state()
	private _dataTypeData?: UmbPropertyEditorConfig;

	private _dataTypeDetailRepository = new UmbDataTypeDetailRepository(this);
	private _dataTypeObserver?: UmbObserverController<UmbDataTypeDetailModel | undefined>;

	#contentPropertyContext = new UmbContentPropertyContext(this);

	private async _checkSchemaSupport() {
		if (!this._ownerEntityType || !this._propertyEditorSchemaAlias) return;

		if (this._ownerEntityType in UMB_UNSUPPORTED_EDITOR_SCHEMA_ALIASES) {
			this._isUnsupported = UMB_UNSUPPORTED_EDITOR_SCHEMA_ALIASES[this._ownerEntityType].includes(
				this._propertyEditorSchemaAlias,
			);
		}
	}

	private async _observeDataType(dataTypeUnique?: string) {
		this._dataTypeObserver?.destroy();
		if (dataTypeUnique) {
			// Its not technically needed to have await here, this is only to ensure that the data is loaded before we observe it, and thereby only updating the DOM with the latest data.
			await this._dataTypeDetailRepository.requestByUnique(dataTypeUnique);
			this._dataTypeObserver = this.observe(
				await this._dataTypeDetailRepository.byUnique(dataTypeUnique),
				(dataType) => {
					const contextValue = dataType ? { unique: dataType.unique } : undefined;
					this.#contentPropertyContext.setDataType(contextValue);

					this._dataTypeData = dataType?.values;
					this._propertyEditorUiAlias = dataType?.editorUiAlias || undefined;
					this._propertyEditorSchemaAlias = dataType?.editorAlias || undefined;
					this._checkSchemaSupport();

					// If there is no UI, we will look up the Property editor model to find the default UI alias:
					if (!this._propertyEditorUiAlias && dataType?.editorAlias) {
						//use 'dataType.editorAlias' to look up the extension in the registry:
						// TODO: lets implement a way to observe once. [NL]
						this.observe(
							umbExtensionsRegistry.byTypeAndAlias('propertyEditorSchema', dataType.editorAlias),
							(extension) => {
								if (!extension) return;
								this._propertyEditorUiAlias = extension?.meta.defaultPropertyEditorUiAlias;
								this.removeUmbControllerByAlias('_observePropertyEditorSchema');
							},
							'_observePropertyEditorSchema',
						);
					} else {
						this.removeUmbControllerByAlias('_observePropertyEditorSchema');
					}
				},
				'_observeDataType',
			);
		}
	}

	override render() {
		if (!this._propertyEditorUiAlias || !this._property?.alias) return;
		if (this._isUnsupported) {
			return html`<umb-unsupported-property
				.alias=${this._property.alias}
				.schema=${this._propertyEditorSchemaAlias!}></umb-unsupported-property>`;
		}
		return html`
			<umb-property
				.dataPath=${this.dataPath}
				.alias=${this._property.alias}
				.label=${this._property.name}
				.description=${this._property.description ?? undefined}
				.appearance=${this._property.appearance}
				property-editor-ui-alias=${ifDefined(this._propertyEditorUiAlias)}
				.config=${this._dataTypeData}
				.validation=${this._property.validation}
				?readonly=${this.readonly}>
			</umb-property>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-type-based-property': UmbPropertyTypeBasedPropertyElement;
	}
}
