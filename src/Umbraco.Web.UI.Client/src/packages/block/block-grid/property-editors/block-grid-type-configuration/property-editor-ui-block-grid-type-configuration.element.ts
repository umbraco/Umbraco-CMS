import type { UmbBlockGridTypeGroupType } from '../../types.js';
import { UMB_BLOCK_GRID_TYPE_WORKSPACE_MODAL } from '../../workspace/index.js';
import { UMB_BLOCK_GRID_TYPE } from '../../constants.js';
import type { UmbBlockTypeWithGroupKey, UmbInputBlockTypeElement } from '@umbraco-cms/backoffice/block-type';
import type {
	UmbPropertyEditorUiElement,
	UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
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
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import {
	UMB_PROPERTY_CONTEXT,
	UMB_PROPERTY_DATASET_CONTEXT,
	type UmbPropertyDatasetContext,
} from '@umbraco-cms/backoffice/property';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';

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
	#sorter = new UmbSorterController<MappedGroupWithBlockTypes, HTMLElement>(this, {
		getUniqueOfElement: (element) => element.getAttribute('data-umb-group-key'),
		getUniqueOfModel: (modelEntry) => modelEntry.key!,
		itemSelector: '.group',
		handleSelector: '.group-handle',
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
			this.observe(
				await this.#datasetContext.propertyValueByAlias('blockGroups'),
				(value) => {
					this.#blockGroups = (value as Array<UmbBlockGridTypeGroupType>) ?? [];
					this.#mapValuesToBlockGroups();
				},
				'_observeBlockGroups',
			);
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

	async #onChange(e: Event, groupKey?: string) {
		e.stopPropagation();
		const element = e.target as UmbInputBlockTypeElement;
		const value = element.value.map((x) => ({ ...x, groupKey }));

		if (groupKey) {
			// Update the specific group:
			this._groupsWithBlockTypes = this._groupsWithBlockTypes.map((group) => {
				if (group.key === groupKey) {
					return { ...group, blocks: value };
				}
				return group;
			});
		} else {
			// Update the not grouped blocks:
			this._notGroupedBlockTypes = value;
		}

		this.#updateValue();
	}

	#updateValue() {
		this.value = [...this._notGroupedBlockTypes, ...this._groupsWithBlockTypes.flatMap((group) => group.blocks)];
		this.dispatchEvent(new UmbChangeEvent());
	}

	#updateBlockGroupsValue(groups: Array<UmbBlockGridTypeGroupType>) {
		this.#datasetContext?.setPropertyValue('blockGroups', groups);
	}

	#onCreate(e: CustomEvent, groupKey?: string) {
		const selectedElementType = e.detail.contentElementTypeKey;
		if (selectedElementType) {
			this.#blockTypeWorkspaceModalRegistration?.open({}, 'create/' + selectedElementType + '/' + (groupKey ?? 'null'));
		}
	}

	// TODO: Implement confirm dialog [NL]
	async #deleteGroup(groupKey: string) {
		const groupName = this.#blockGroups?.find((group) => group.key === groupKey)?.name ?? '';
		await umbConfirmModal(this, {
			headline: '#blockEditor_confirmDeleteBlockGroupTitle',
			content: this.localize.term('#blockEditor_confirmDeleteBlockGroupMessage', [groupName]),
			color: 'danger',
			confirmLabel: '#general_delete',
		});
		// If a group is deleted, Move the blocks to no group:
		this.value = this.#value.map((block) => (block.groupKey === groupKey ? { ...block, groupKey: undefined } : block));
		if (this.#blockGroups) {
			this.#updateBlockGroupsValue(this.#blockGroups.filter((group) => group.key !== groupKey));
		}
	}

	#onGroupNameChange(e: UUIInputEvent, groupKey: string) {
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
						@change=${(e: Event) => this.#onChange(e, undefined)}
						@create=${(e: CustomEvent) => this.#onCreate(e, undefined)}></umb-input-block-type>`
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
							@change=${(e: Event) => this.#onChange(e, group.key)}
							@create=${(e: CustomEvent) => this.#onCreate(e, group.key)}></umb-input-block-type>
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
				@change=${(e: UUIInputEvent) => this.#onGroupNameChange(e, groupKey)}>
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
