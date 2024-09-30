import type { UmbBlockTypeWithGroupKey, UmbInputBlockTypeElement } from '../../../block-type/index.js';
import '../../../block-type/components/input-block-type/index.js';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';
import {
	html,
	customElement,
	property,
	state,
	repeat,
	nothing,
	css,
	ifDefined,
} from '@umbraco-cms/backoffice/external/lit';
import {
	UmbPropertyValueChangeEvent,
	type UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import {
	UMB_BLOCK_GRID_TYPE,
	UMB_BLOCK_GRID_TYPE_WORKSPACE_MODAL,
	type UmbBlockGridTypeGroupType,
} from '@umbraco-cms/backoffice/block-grid';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import {
	UMB_PROPERTY_CONTEXT,
	UMB_PROPERTY_DATASET_CONTEXT,
	type UmbPropertyDatasetContext,
} from '@umbraco-cms/backoffice/property';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';

interface MappedGroupWithBlockTypes extends UmbBlockGridTypeGroupType {
	blocks: Array<UmbBlockTypeWithGroupKey>;
}

/**
 * @element umb-property-editor-ui-block-grid-type-configuration
 */
@customElement('umb-property-editor-ui-block-grid-type-configuration')
export class UmbPropertyEditorUIBlockGridTypeConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	#moveData?: Array<UmbBlockTypeWithGroupKey>;
	#sorter = new UmbSorterController<MappedGroupWithBlockTypes, HTMLElement>(this, {
		getUniqueOfElement: (element) => element.getAttribute('data-umb-group-key'),
		getUniqueOfModel: (modelEntry) => modelEntry.key!,
		itemSelector: '.group',
		draggableSelector: '.group-handle',
		containerSelector: '#groups',
		onChange: ({ model }) => {
			this._groupsWithBlockTypes = model;
		},
		onEnd: () => {
			// TODO: make one method for updating the blockGroupsDataSetValue:
			this.#datasetContext?.setPropertyValue(
				'blockGroups',
				this._groupsWithBlockTypes.map((group) => ({ key: group.key, name: group.name })),
			);
		},
	});

	#datasetContext?: UmbPropertyDatasetContext;
	#blockTypeWorkspaceModalRegistration?: UmbModalRouteRegistrationController<
		typeof UMB_BLOCK_GRID_TYPE_WORKSPACE_MODAL.DATA,
		typeof UMB_BLOCK_GRID_TYPE_WORKSPACE_MODAL.VALUE
	>;

	#value: Array<UmbBlockTypeWithGroupKey> = [];
	@property({ attribute: false })
	get value() {
		return this.#value;
	}
	set value(value: Array<UmbBlockTypeWithGroupKey>) {
		this.#value = value ?? [];
		this.#mapValuesToBlockGroups();
	}

	@state()
	public _alias?: string;

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	#blockGroups?: Array<UmbBlockGridTypeGroupType>;

	@state()
	private _groupsWithBlockTypes: Array<MappedGroupWithBlockTypes> = [];

	@state()
	private _notGroupedBlockTypes: Array<UmbBlockTypeWithGroupKey> = [];

	@state()
	private _workspacePath?: string;

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_CONTEXT, async (context) => {
			this._alias = context.getAlias();
		});

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, async (context) => {
			this.#datasetContext = context;
			//this.#observeBlocks();
			this.#observeBlockGroups();
		});

		this.#blockTypeWorkspaceModalRegistration = new UmbModalRouteRegistrationController(
			this,
			UMB_BLOCK_GRID_TYPE_WORKSPACE_MODAL,
		)
			.addAdditionalPath(UMB_BLOCK_GRID_TYPE)
			.observeRouteBuilder((routeBuilder) => {
				const newpath = routeBuilder({});
				this._workspacePath = newpath;
			});
	}

	async #observeBlockGroups() {
		if (!this.#datasetContext) return;
		this.observe(await this.#datasetContext.propertyValueByAlias('blockGroups'), (value) => {
			this.#blockGroups = (value as Array<UmbBlockGridTypeGroupType>) ?? [];
			this.#mapValuesToBlockGroups();
		});
	}
	// TODO: No need for this, we just got the value via the value property.. [NL]
	/*
	async #observeBlocks() {
		if (!this.#datasetContext) return;
		this.observe(await this.#datasetContext.propertyValueByAlias('blocks'), (value) => {
			this.value = (value as Array<UmbBlockTypeWithGroupKey>) ?? [];
			this.#mapValuesToBlockGroups();
		});
	}
	*/

	#mapValuesToBlockGroups() {
		if (!this.#blockGroups) return;
		// Map blocks that are not in any group, or in a group that does not exist
		this._notGroupedBlockTypes = this.#value.filter(
			(block) => !block.groupKey || !this.#blockGroups!.find((group) => group.key === block.groupKey),
		);

		// Map blocks to the group they belong to
		this._groupsWithBlockTypes = this.#blockGroups.map((group) => {
			return { name: group.name, key: group.key, blocks: this.#value.filter((value) => value.groupKey === group.key) };
		});

		this.#sorter.setModel(this._groupsWithBlockTypes);
	}

	#onDelete(e: CustomEvent, groupKey?: string) {
		const updatedValues = (e.target as UmbInputBlockTypeElement).value.map((value) => ({ ...value, groupKey }));
		const filteredValues = this.#value.filter((value) => value.groupKey !== groupKey);
		this.value = [...filteredValues, ...updatedValues];
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	async #onChange(e: CustomEvent) {
		e.stopPropagation();
		const element = e.target as UmbInputBlockTypeElement;
		const value = element.value;

		if (!e.detail?.moveComplete) {
			// Container change, store data of the new group...
			const newGroupKey = element.getAttribute('data-umb-group-key');
			const movedItem = e.detail?.item as UmbBlockTypeWithGroupKey;
			// Check if item moved back to original group...
			if (movedItem.groupKey === newGroupKey) {
				this.#moveData = undefined;
			} else {
				this.#moveData = value.map((block) => ({ ...block, groupKey: newGroupKey }));
			}
		} else if (e.detail?.moveComplete) {
			// Move complete, get the blocks that were in an untouched group
			const blocks = this.#value
				.filter((block) => !value.find((value) => value.contentElementTypeKey === block.contentElementTypeKey))
				.filter(
					(block) => !this.#moveData?.find((value) => value.contentElementTypeKey === block.contentElementTypeKey),
				);

			this.value = this.#moveData ? [...blocks, ...value, ...this.#moveData] : [...blocks, ...value];
			this.dispatchEvent(new UmbPropertyValueChangeEvent());
			this.#moveData = undefined;
		}
	}

	#onCreate(e: CustomEvent, groupKey?: string) {
		const selectedElementType = e.detail.contentElementTypeKey;
		if (selectedElementType) {
			this.#blockTypeWorkspaceModalRegistration?.open({}, 'create/' + selectedElementType + '/' + (groupKey ?? null));
		}
	}

	// TODO: Implement confirm dialog [NL]
	#deleteGroup(groupKey: string) {
		// TODO: make one method for updating the blockGroupsDataSetValue: [NL]
		// This one that deletes might require the ability to parse what to send as an argument to the method, then a filtering can occur before.
		this.#datasetContext?.setPropertyValue(
			'blockGroups',
			this.#blockGroups?.filter((group) => group.key !== groupKey),
		);

		// If a group is deleted, Move the blocks to no group:
		this.value = this.#value.map((block) => (block.groupKey === groupKey ? { ...block, groupKey: undefined } : block));
	}

	#changeGroupName(e: UUIInputEvent, groupKey: string) {
		const groupName = e.target.value as string;
		// TODO: make one method for updating the blockGroupsDataSetValue: [NL]
		this.#datasetContext?.setPropertyValue(
			'blockGroups',
			this.#blockGroups?.map((group) => (group.key === groupKey ? { ...group, name: groupName } : group)),
		);
	}

	override render() {
		return html`<div id="groups">
			${this._notGroupedBlockTypes
				? html`<umb-input-block-type
						.propertyAlias=${this._alias}
						.value=${this._notGroupedBlockTypes}
						.workspacePath=${this._workspacePath}
						@change=${this.#onChange}
						@create=${(e: CustomEvent) => this.#onCreate(e, undefined)}
						@delete=${(e: CustomEvent) => this.#onDelete(e, undefined)}></umb-input-block-type>`
				: ''}
			${repeat(
				this._groupsWithBlockTypes,
				(group) => group.key,
				(group) =>
					html`<div class="group" data-umb-group-key=${ifDefined(group.key)}>
						${group.key ? this.#renderGroupInput(group.key, group.name) : nothing}
						<umb-input-block-type
							data-umb-group-key=${group.key}
							.propertyAlias=${this._alias + '_' + group.key}
							.value=${group.blocks}
							.workspacePath=${this._workspacePath}
							@change=${this.#onChange}
							@create=${(e: CustomEvent) => this.#onCreate(e, group.key)}
							@delete=${(e: CustomEvent) => this.#onDelete(e, group.key)}></umb-input-block-type>
					</div>`,
			)}
		</div>`;
	}

	#renderGroupInput(groupKey: string, groupName?: string) {
		return html`<div class="group-handle">
			<uui-input
				auto-width
				label="Group"
				.value=${groupName ?? ''}
				@change=${(e: UUIInputEvent) => this.#changeGroupName(e, groupKey)}>
				<uui-button compact slot="append" label="delete" @click=${() => this.#deleteGroup(groupKey)}>
					<uui-icon name="icon-trash"></uui-icon>
				</uui-button>
			</uui-input>
		</div>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			uui-input:not(:hover, :focus) {
				border: 1px solid transparent;
			}
			uui-input:not(:hover, :focus) uui-button {
				opacity: 0;
			}

			.group-handle {
				padding: var(--uui-size-1);
				margin-top: var(--uui-size-6);
				margin-bottom: var(--uui-size-4);
				cursor: grab;
			}

			.group-handle:hover {
				background-color: var(--uui-color-divider);
				border-radius: var(--uui-border-radius);
			}

			.group:has([drag-placeholder]) {
				opacity: 0.2;
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
