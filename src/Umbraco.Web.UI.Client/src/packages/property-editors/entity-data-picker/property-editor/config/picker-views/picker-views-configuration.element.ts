import { UMB_DATA_TYPE_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/data-type';
import { css, customElement, html, nothing, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
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
import type { ManifestCollectionView } from '@umbraco-cms/backoffice/collection';

type UmbEntityDataPickerPickerViewsConfigurationPropertyValue =
	Array<UmbEntityDataPickerPickerViewsConfigurationPropertyValueEntry>;

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
		this.#observeManifests();
	}
	public get value(): UmbEntityDataPickerPickerViewsConfigurationPropertyValue {
		return this.#value;
	}
	#value: UmbEntityDataPickerPickerViewsConfigurationPropertyValue = [];

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	@state()
	private _isCollectionDataSource = false;

	@state()
	private _manifestByAlias: Map<string, ManifestCollectionView> = new Map();

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

	#observeManifests() {
		const aliases = this.#value.map((entry) => entry.alias);
		if (aliases.length === 0) {
			this._manifestByAlias = new Map();
			return;
		}

		this.observe(
			umbExtensionsRegistry.byTypeAndAliases<ManifestCollectionView>('collectionView', aliases),
			(manifests) => {
				this._manifestByAlias = new Map(manifests.map((m) => [m.alias, m]));
			},
			'observeCollectionViewManifests',
		);
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

	#onAdd(event: UmbChangeEvent) {
		const target = event.target as UmbInputManifestElement;
		const pickedValue = target.value;

		if (!pickedValue?.value) return;

		// Check for duplicate
		const isDuplicate = this.#value.some((entry) => entry.alias === pickedValue.value);
		if (isDuplicate) {
			return;
		}

		this.#value = [...this.#value, { alias: pickedValue.value }];
		this.#observeManifests();
		this.dispatchEvent(new UmbChangeEvent());
	}

	#onRemove(alias: string) {
		this.#value = this.#value.filter((entry) => entry.alias !== alias);
		this.#observeManifests();
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		if (!this._isCollectionDataSource) {
			return this.#renderDisabledState();
		}

		return html`
			<div id="items">${this.#renderItems()}</div>
			<umb-input-manifest extension-type="collectionView" @change=${this.#onAdd}></umb-input-manifest>
		`;
	}

	#renderItems() {
		if (this.#value.length === 0) return nothing;

		return html`<uui-ref-list>
			${repeat(
				this.#value,
				(entry) => entry.alias,
				(entry) => this.#renderItem(entry.alias),
			)}
		</uui-ref-list>`;
	}

	#renderItem(alias: string) {
		const manifest = this._manifestByAlias.get(alias);

		if (!manifest) {
			return html`
				<uui-ref-node name=${this.localize.term('general_notFound')} detail=${alias} readonly class="not-found">
					<umb-icon slot="icon" name="icon-alert"></umb-icon>
					<uui-action-bar slot="actions">
						<uui-button
							label=${this.localize.term('general_remove')}
							@click=${() => this.#onRemove(alias)}></uui-button>
					</uui-action-bar>
				</uui-ref-node>
			`;
		}

		return html`
			<uui-ref-node name=${manifest.meta.label} detail=${alias} readonly>
				<umb-icon slot="icon" name=${manifest.meta.icon}></umb-icon>
				<uui-action-bar slot="actions">
					<uui-button label=${this.localize.term('general_remove')} @click=${() => this.#onRemove(alias)}></uui-button>
				</uui-action-bar>
			</uui-ref-node>
		`;
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

			.not-found {
				color: var(--uui-color-danger);
			}
		`,
	];
}

export { UmbPropertyEditorUIEntityDataPickerPickerViewsConfigurationElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-entity-data-picker-picker-views-configuration': UmbPropertyEditorUIEntityDataPickerPickerViewsConfigurationElement;
	}
}
