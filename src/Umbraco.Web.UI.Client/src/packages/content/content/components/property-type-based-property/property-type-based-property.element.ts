import { UmbPropertyTypeBasedPropertyContext } from './property-type-based-property.context.js';
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
	public property?: UmbPropertyTypeModel;

	/**
	 * Optional pre-loaded data type detail. When provided, the element uses this directly instead of loading individually.
	 * @type {UmbDataTypeDetailModel}
	 */
	@property({ attribute: false })
	public dataTypeDetail?: UmbDataTypeDetailModel;

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
	private _propertyEditorDataSourceAlias?: string;

	@state()
	private _isUnsupported?: boolean;

	private _dataTypeValues?: UmbPropertyEditorConfig;

	private _dataTypeDetailRepository = new UmbDataTypeDetailRepository(this);
	private _dataTypeObserver?: UmbObserverController<UmbDataTypeDetailModel | undefined>;

	#context = new UmbPropertyTypeBasedPropertyContext(this);

	#lastDataTypeUnique?: string;

	private async _checkSchemaSupport() {
		if (!this._ownerEntityType || !this._propertyEditorSchemaAlias) return;

		if (this._ownerEntityType in UMB_UNSUPPORTED_EDITOR_SCHEMA_ALIASES) {
			// TODO: We should get rid of this system, f your reading this please dont rely on this, we will get rid of it in the future. [NL]
			this._isUnsupported = UMB_UNSUPPORTED_EDITOR_SCHEMA_ALIASES[this._ownerEntityType].includes(
				this._propertyEditorSchemaAlias,
			);
		}
	}

	override willUpdate(changedProperties: Map<string, unknown>) {
		super.willUpdate(changedProperties);

		if (changedProperties.has('property') || changedProperties.has('dataTypeDetail')) {
			const propertyDataTypeUnique = this.property?.dataType.unique;

			if (propertyDataTypeUnique !== this.#lastDataTypeUnique || changedProperties.has('dataTypeDetail')) {
				this.#lastDataTypeUnique = propertyDataTypeUnique;

				if (this.dataTypeDetail && propertyDataTypeUnique && this.dataTypeDetail.unique === propertyDataTypeUnique) {
					// Use provided data type detail directly — no API call needed
					this._dataTypeObserver?.destroy();
					this.#applyDataType(this.dataTypeDetail);
				} else if (propertyDataTypeUnique) {
					// Fall back to individual loading
					this.#observeDataType(propertyDataTypeUnique);
				}
			}
		}
	}

	async #observeDataType(dataTypeUnique: string) {
		this._dataTypeObserver?.destroy();

		await this._dataTypeDetailRepository.requestByUnique(dataTypeUnique);
		this._dataTypeObserver = this.observe(
			await this._dataTypeDetailRepository.byUnique(dataTypeUnique),
			(dataTypeDetail) => {
				this.#applyDataType(dataTypeDetail);
			},
			'_observeDataType',
		);
	}

	#applyDataType(dataType: UmbDataTypeDetailModel | undefined) {
		const contextValue = dataType ? { unique: dataType.unique } : undefined;
		this.#context.setDataType(contextValue);

		this._dataTypeValues = dataType?.values;
		this._propertyEditorUiAlias = dataType?.editorUiAlias || undefined;
		this._propertyEditorSchemaAlias = dataType?.editorAlias || undefined;
		this._propertyEditorDataSourceAlias = dataType?.editorDataSourceAlias || undefined;
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
	}

	override render() {
		if (!this._propertyEditorUiAlias || !this.property?.alias) return;
		if (this._isUnsupported) {
			return html`<umb-unsupported-property
				.alias=${this.property.alias}
				.schema=${this._propertyEditorSchemaAlias!}></umb-unsupported-property>`;
		}
		return html`
			<umb-property
				.dataPath=${this.dataPath}
				.alias=${this.property.alias}
				.label=${this.property.name}
				.description=${this.property.description ?? undefined}
				.appearance=${this.property.appearance}
				property-editor-ui-alias=${ifDefined(this._propertyEditorUiAlias)}
				property-editor-data-source-alias=${ifDefined(this._propertyEditorDataSourceAlias)}
				.config=${this._dataTypeValues}
				.validation=${this.property.validation}
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
