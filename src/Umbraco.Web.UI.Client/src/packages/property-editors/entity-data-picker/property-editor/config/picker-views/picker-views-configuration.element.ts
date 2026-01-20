import { UMB_DATA_TYPE_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/data-type';
import {
	css,
	customElement,
	html,
	property,
	state,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { isPickerCollectionDataSource, type UmbPickerDataSource } from '@umbraco-cms/backoffice/picker-data-source';
import type { UmbInputManifestElement } from '@umbraco-cms/backoffice/components';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import type { ManifestPropertyEditorDataSource } from '@umbraco-cms/backoffice/property-editor-data-source';
import type { UmbCollectionLayoutConfiguration } from '@umbraco-cms/backoffice/collection';

type UmbEntityDataPickerPickerViewsConfigurationPropertyValue = Array<UmbEntityDataPickerPickerViewsConfigurationPropertyValueEntry>

interface UmbEntityDataPickerPickerViewsConfigurationPropertyValueEntry {
	alias: string;
}

/**
 * @element umb-property-editor-ui-entity-data-picker-picker-views-configuration
 */
@customElement('umb-property-editor-ui-entity-data-picker-picker-views-configuration')
export class UmbPropertyEditorUIEntityDataPickerPickerViewsConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	#dataSourceApiInitializer?: UmbExtensionApiInitializer<ManifestPropertyEditorDataSource>;

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

	constructor() {
		super();

		this.consumeContext(UMB_DATA_TYPE_WORKSPACE_CONTEXT, (context) => {
			if (!context) return;
			this.observe(
				context.propertyEditorDataSourceAlias,
				(alias) => {
					this.#initializeDataSource(alias);
				},
				'observeDataSourceAlias',
			);
		});
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
		});
	}

	#onChange(event: UmbChangeEvent) {
		const target = event.target as UmbInputManifestElement;
		const value = target.value;

		this.value = value?.map((view) => ({ alias: view })) ?? [];
		debugger;
		this.dispatchEvent(new UmbChangeEvent());
	}

	#renderItem(item: UmbCollectionLayoutConfiguration) {
		return html`<div><umb-icon name=${item.icon}></umb-icon>${item.name}${item.collectionView}</div>`
	}

	override render() {
		if (!this._isCollectionDataSource) {
			return this.#renderDisabledState();
		}

		// TODO: Implement a proper input-manifest that can handle array of collection views
		return html`<umb-input-manifest extension-type="collectionView" @change=${this.#onChange}></umb-input-manifest>`;
	}

	#renderDisabledState() {
		return html`
			<div id="disabled-message">
				<umb-localize key="entityDataPicker_pickerViewsDisabled">
					Picker views configuration is only available for collection-based data sources.
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

export { UmbPropertyEditorUIEntityDataPickerPickerViewsConfigurationElement as element};

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-entity-data-picker-picker-views-configuration': UmbPropertyEditorUIEntityDataPickerPickerViewsConfigurationElement;
	}
}
