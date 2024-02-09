import type { UmbBlockTypeWithGroupKey, UmbInputBlockTypeElement } from '../../../block-type/index.js';
import '../../../block-type/components/input-block-type/index.js';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { html, customElement, property, state, repeat, nothing, css } from '@umbraco-cms/backoffice/external/lit';
import {
	UmbPropertyValueChangeEvent,
	type UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import {
	UMB_BLOCK_GRID_TYPE,
	type UmbBlockGridGroupType,
	type UmbBlockGridGroupTypeConfiguration,
} from '@umbraco-cms/backoffice/block';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { UMB_PROPERTY_DATASET_CONTEXT, type UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import { UMB_WORKSPACE_MODAL, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';

/**
 * @element umb-property-editor-ui-block-grid-type-configuration
 */
@customElement('umb-property-editor-ui-block-grid-type-configuration')
export class UmbPropertyEditorUIBlockGridTypeConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	#datasetContext?: UmbPropertyDatasetContext;
	#blockTypeWorkspaceModalRegistration?: UmbModalRouteRegistrationController<
		typeof UMB_WORKSPACE_MODAL.DATA,
		typeof UMB_WORKSPACE_MODAL.VALUE
	>;

	private _value: Array<UmbBlockTypeWithGroupKey> = [];
	@property({ attribute: false })
	get value() {
		return this._value;
	}
	set value(value: Array<UmbBlockTypeWithGroupKey>) {
		this._value = value ?? [];
	}

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	@state()
	private _blockGroups: Array<UmbBlockGridGroupType> = [];

	@state()
	private _mappedValuesAndGroups: Array<UmbBlockGridGroupTypeConfiguration> = [];

	@state()
	private _workspacePath?: string;

	constructor() {
		super();
		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, async (instance) => {
			this.#datasetContext = instance;
			this.#observeProperties();
		});

		this.#blockTypeWorkspaceModalRegistration?.destroy();

		this.#blockTypeWorkspaceModalRegistration = new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath(UMB_BLOCK_GRID_TYPE)
			.onSetup(() => {
				return { data: { entityType: UMB_BLOCK_GRID_TYPE, preset: {} }, modal: { size: 'large' } };
			})
			.observeRouteBuilder((routeBuilder) => {
				const newpath = routeBuilder({});
				this._workspacePath = newpath;
			});
	}

	async #observeProperties() {
		if (!this.#datasetContext) return;

		this.observe(await this.#datasetContext.propertyValueByAlias('blockGroups'), (value) => {
			this._blockGroups = (value as Array<UmbBlockGridGroupType>) ?? [];
			this.#mapValuesToBlockGroups();
		});
		this.observe(await this.#datasetContext.propertyValueByAlias('blocks'), () => {
			this.#mapValuesToBlockGroups();
		});
	}

	#mapValuesToBlockGroups() {
		// What if a block is in a group that does not exist in the block groups? Should it be removed? (Right now they will never be rendered)
		const valuesWithNoGroup = this._value.filter((value) => !value.groupKey);

		const valuesWithGroup = this._blockGroups.map((group) => {
			return { name: group.name, key: group.key, blocks: this._value.filter((value) => value.groupKey === group.key) };
		});

		this._mappedValuesAndGroups = [{ blocks: valuesWithNoGroup }, ...valuesWithGroup];
	}

	#onChange(e: CustomEvent, groupKey?: string) {
		const updatedValues = (e.target as UmbInputBlockTypeElement).value.map((value) => ({ ...value, groupKey }));
		const filteredValues = this.value.filter((value) => value.groupKey !== groupKey);
		this.value = [...filteredValues, ...updatedValues];
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	#onCreate(e: CustomEvent, groupKey: string | null) {
		const selectedElementType = e.detail.contentElementTypeKey;
		if (selectedElementType) {
			this.#blockTypeWorkspaceModalRegistration?.open({}, 'create/' + selectedElementType + '/' + groupKey);
		}
	}

	#deleteGroup(groupKey: string) {
		this.#datasetContext?.setPropertyValue(
			'blockGroups',
			this._blockGroups.filter((group) => group.key !== groupKey),
		);

		// Should blocks that belonged to the removed group be deleted as well?
		this.value = this._value.filter((block) => block.groupKey !== groupKey);
	}

	#changeGroupName(e: UUIInputEvent, groupKey: string) {
		const groupName = e.target.value as string;
		this.#datasetContext?.setPropertyValue(
			'blockGroups',
			this._blockGroups.map((group) => (group.key === groupKey ? { ...group, name: groupName } : group)),
		);
	}

	render() {
		return html`${repeat(
			this._mappedValuesAndGroups,
			(group) => group.key,
			(group) =>
				html`${group.key ? this.#renderGroupInput(group.key, group.name) : nothing}
					<umb-input-block-type
						.value=${group.blocks}
						.workspacePath=${this._workspacePath}
						@create=${(e: CustomEvent) => this.#onCreate(e, group.key ?? null)}
						@change=${(e: CustomEvent) => this.#onChange(e, group.key)}></umb-input-block-type>`,
		)}`;
	}

	#renderGroupInput(groupKey: string, groupName?: string) {
		return html`<uui-input
			auto-width
			label="Group"
			.value=${groupName ?? ''}
			@change=${(e: UUIInputEvent) => this.#changeGroupName(e, groupKey)}>
			<uui-button compact slot="append" label="delete" @click=${() => this.#deleteGroup(groupKey)}>
				<uui-icon name="icon-trash"></uui-icon>
			</uui-button>
		</uui-input>`;
	}

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
