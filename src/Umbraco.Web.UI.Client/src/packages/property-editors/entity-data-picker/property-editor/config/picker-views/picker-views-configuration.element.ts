import {
	UMB_ENTITY_DATA_PICKER_CARD_COLLECTION_VIEW_ALIAS,
	UMB_ENTITY_DATA_PICKER_REF_COLLECTION_VIEW_ALIAS,
} from '../../../picker-collection/views/constants.js';
import type { UmbEntityDataPickerPickerViewsConfigurationPropertyValue } from './types.js';
import { UMB_DATA_TYPE_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/data-type';
import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { isPickerCollectionDataSource, type UmbPickerDataSource } from '@umbraco-cms/backoffice/picker-data-source';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import type { ManifestPropertyEditorDataSource } from '@umbraco-cms/backoffice/property-editor-data-source';

@customElement('umb-entity-data-picker-picker-views-configuration-property-editor-ui')
export class UmbEntityDataPickerPickerViewsConfigurationPropertyEditorUIElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	#dataSourceApiInitializer?: UmbExtensionApiInitializer<ManifestPropertyEditorDataSource>;
	#isNew = false;

	constructor() {
		super();

		this.consumeContext(UMB_DATA_TYPE_WORKSPACE_CONTEXT, (context) => {
			if (!context) return;
			this.observe(
				context.isNew,
				(isNew) => {
					this.#isNew = isNew ?? false;
				},
				'observeIsNew',
			);
			this.observe(
				context.propertyEditorDataSourceAlias,
				(alias) => {
					this.#initializeDataSource(alias);
				},
				'observeDataSourceAlias',
			);
		});
	}

	@property({ type: Array })
	public set value(value: UmbEntityDataPickerPickerViewsConfigurationPropertyValue | undefined) {
		this.#value = value ?? [];
	}
	public get value(): UmbEntityDataPickerPickerViewsConfigurationPropertyValue {
		return this.#value;
	}
	#value: UmbEntityDataPickerPickerViewsConfigurationPropertyValue = [];

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	@state()
	private _isCollectionDataSource = false;

	#onChange(event: Event) {
		event.stopPropagation();
		const target = event.target as HTMLElement & { selection: Array<string> };
		this.#value = target.selection.map((alias) => ({ alias }));
		this.dispatchEvent(new UmbChangeEvent());
	}

	#initializeDataSource(dataSourceAlias: string | null | undefined) {
		// Clean up previous initializer
		this.#dataSourceApiInitializer?.destroy();
		this._isCollectionDataSource = false;

		if (!dataSourceAlias) {
			return;
		}

		this.#dataSourceApiInitializer = new UmbExtensionApiInitializer<
			ManifestPropertyEditorDataSource,
			UmbExtensionApiInitializer<ManifestPropertyEditorDataSource>,
			UmbPickerDataSource
		>(this, umbExtensionsRegistry, dataSourceAlias, [this], (permitted, ctrl) => {
			if (!permitted) {
				this._isCollectionDataSource = false;
				return;
			}

			const api = ctrl.api as UmbPickerDataSource;
			this._isCollectionDataSource = isPickerCollectionDataSource(api);

			if (!this._isCollectionDataSource && this.#value.length > 0) {
				this.#value = [];
				this.dispatchEvent(new UmbChangeEvent());
			}

			if (this._isCollectionDataSource && this.#isNew && this.#value.length === 0) {
				this.#value = [
					{ alias: UMB_ENTITY_DATA_PICKER_REF_COLLECTION_VIEW_ALIAS },
					{ alias: UMB_ENTITY_DATA_PICKER_CARD_COLLECTION_VIEW_ALIAS },
				];
				this.dispatchEvent(new UmbChangeEvent());
			}
		});
	}

	override render() {
		if (!this._isCollectionDataSource) {
			return this.#renderDisabledState();
		}

		return html`<umb-input-extension
			.allowedExtensionTypes=${['collectionView']}
			.selection=${this.#value.map((v) => v.alias)}
			@change=${this.#onChange}></umb-input-extension>`;
	}

	#renderDisabledState() {
		return html`
			<div id="disabled-message">
				<umb-localize key="entityDataPicker_pickerViewsDisabled">
					Picker views are only available for collection-based data sources.
				</umb-localize>
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#disabled-message {
				color: var(--uui-color-text-alt);
				font-style: italic;
				padding: var(--uui-size-3);
			}
		`,
	];
}

export { UmbEntityDataPickerPickerViewsConfigurationPropertyEditorUIElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-data-picker-picker-views-configuration-property-editor-ui': UmbEntityDataPickerPickerViewsConfigurationPropertyEditorUIElement;
	}
}
