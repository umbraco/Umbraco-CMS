import type { UmbBlockTypeWithGroupKey, UmbInputBlockTypeElement } from '../../../block-type/index.js';
import '../../../block-type/components/input-block-type/index.js';
import { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { html, customElement, property, state, css, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_PROPERTY_DATASET_CONTEXT, UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import { UmbBlockGridGroupType, UmbBlockGridGroupTypeConfiguration } from '@umbraco-cms/backoffice/block';

/**
 * @element umb-property-editor-ui-block-grid-type-configuration
 */
@customElement('umb-property-editor-ui-block-grid-type-configuration')
export class UmbPropertyEditorUIBlockGridTypeConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	#datasetContext?: UmbPropertyDatasetContext;

	@property({ attribute: false })
	value: UmbBlockTypeWithGroupKey[] = [];

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	@state()
	private _blockGroups: Array<UmbBlockGridGroupType> = [];

	@state()
	private _mappedValuesAndGroups: Array<UmbBlockGridGroupTypeConfiguration> = [];

	constructor() {
		super();
		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, async (instance) => {
			this.#datasetContext = instance;
			this.#observeProperties();
		});
	}

	async #observeProperties() {
		if (!this.#datasetContext) return;

		this.observe(await this.#datasetContext.propertyValueByAlias('blockGroups'), (value) => {
			this._blockGroups = value as Array<UmbBlockGridGroupType>;
			this.#mapValuesToBlockGroups();
			console.log('groups changed', value);
		});
		this.observe(await this.#datasetContext.propertyValueByAlias('blocks'), (value) => {
			this.#mapValuesToBlockGroups();
			console.log('value changed', value);
		});
	}

	#mapValuesToBlockGroups() {
		const valuesWithNoGroup = this.value.filter(
			// Look for values without a group, or with a group that is non existent.
			(value) => !value.groupKey || this._blockGroups.find((group) => group.key !== value.groupKey),
		);
		//.map((value) => ({ ...value, groupKey: undefined }));

		const valuesWithGroup = this._blockGroups.map((group) => {
			return { name: group.name, key: group.key, blocks: this.value.filter((value) => value.groupKey === group.key) };
		});

		this._mappedValuesAndGroups = [{ blocks: valuesWithNoGroup }, ...valuesWithGroup];
	}

	#onChange(e: Event, groupKey?: string) {
		const newValues = (e.target as UmbInputBlockTypeElement).value;

		// remove all values that are in the group, or have a group that does not exist in the block groups.
		const values = this.value
			.filter((b) => b.groupKey !== groupKey)
			.filter((b) => this._blockGroups.find((group) => group.key === b.groupKey));

		this.value = [...values, ...newValues];
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`${repeat(
			this._mappedValuesAndGroups,
			(group) => group.key,
			(group) =>
				html`<umb-input-block-type
					entity-type="block-grid-type"
					.groupKey=${group.key}
					.groupName=${group.name}
					.value=${group.blocks}
					@change=${(e: Event) => this.#onChange(e, group.key)}></umb-input-block-type>`,
		)}`;
	}

	/*
	render() {
		return html`<umb-input-block-type
			entity-type="block-grid-type"
			.groups=${this._blockGroups}
			.value=${this.value}
			@change=${(e: Event) => (this.value = (e.target as UmbInputBlockTypeElement).value)}></umb-input-block-type>`;
	}*/

	static styles = [
		UmbTextStyles,
		css`
			uui-input {
				margin-top: var(--uui-size-6);
				margin-bottom: var(--uui-size-4);
			}

			uui-input:not(:hover, :focus) {
				border: 1px solid transparent;
			}
			uui-input:not(:hover, :focus) uui-button {
				opacity: 0;
			}
		`,
	];
}

export default UmbPropertyEditorUIBlockGridTypeConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-grid-type-configuration': UmbPropertyEditorUIBlockGridTypeConfigurationElement;
	}
}
