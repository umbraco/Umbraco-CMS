import type { UmbBlockGridTypeAreaTypePermission, UmbBlockGridTypeGroupType } from '../../index.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { html, customElement, property, css, state, repeat, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_DATA_TYPE_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/data-type';
import type { UmbBlockTypeWithGroupKey } from '@umbraco-cms/backoffice/block-type';
import type { UUIComboboxElement, UUIComboboxEvent, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbRepositoryItemsManager } from '@umbraco-cms/backoffice/repository';
import {
	UMB_DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS,
	type UmbDocumentTypeItemModel,
} from '@umbraco-cms/backoffice/document-type';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-property-editor-ui-block-grid-area-type-permission')
export class UmbPropertyEditorUIBlockGridAreaTypePermissionElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	@property({ type: Array })
	public set value(value: Array<UmbBlockGridTypeAreaTypePermission>) {
		this._value = value ?? [];
	}
	public get value(): Array<UmbBlockGridTypeAreaTypePermission> {
		return this._value;
	}

	@state()
	private _value: Array<UmbBlockGridTypeAreaTypePermission> = [];

	_blockTypes: Array<UmbBlockTypeWithGroupKey> = [];

	@state()
	private _blockTypesWithElementName: Array<{ type: UmbBlockTypeWithGroupKey; name: string }> = [];

	@state()
	private _blockGroups: Array<UmbBlockGridTypeGroupType> = [];

	#itemsManager = new UmbRepositoryItemsManager<UmbDocumentTypeItemModel>(
		this,
		UMB_DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS,
	);

	constructor() {
		super();

		this.observe(this.#itemsManager.items, (items) => {
			this._blockTypesWithElementName = items
				.map((item) => {
					const blockType = this._blockTypes.find((block) => block.contentElementTypeKey === item.unique);
					if (blockType) {
						return { type: blockType, name: item.name };
					}
					return undefined;
				})
				.filter((x) => x !== undefined) as Array<{ type: UmbBlockTypeWithGroupKey; name: string }>;
		});

		this.consumeContext(UMB_DATA_TYPE_WORKSPACE_CONTEXT, async (context) => {
			this.observe(
				await context?.propertyValueByAlias<Array<UmbBlockTypeWithGroupKey>>('blocks'),
				(blockTypes) => {
					this._blockTypes = blockTypes ?? [];
					this.#itemsManager.setUniques(this._blockTypes.map((block) => block.contentElementTypeKey));
				},
				'observeBlockType',
			);
			this.observe(
				await context?.propertyValueByAlias<Array<UmbBlockGridTypeGroupType>>('blockGroups'),
				(blockGroups) => {
					this._blockGroups = blockGroups ?? [];
				},
				'observeBlockGroups',
			);
		}).passContextAliasMatches();
	}

	#addNewPermission() {
		this.value = [...this.value, { minAllowed: 0, maxAllowed: undefined }];
		this.dispatchEvent(new UmbChangeEvent());
	}

	#setPermissionKey(e: UUIComboboxEvent, index: number) {
		const value = [...this.value];
		const optionElement = e.composedPath()[0] as UUIComboboxElement;
		const optionKey = optionElement.value as string;

		// If optionKey exists (new option picked), we just assume its a elementType if the key is not from blockGroups.
		// If optionKey is empty (the option removed was removed), set both to undefined.
		const setting: UmbBlockGridTypeAreaTypePermission = optionKey
			? this._blockGroups.find((group) => group.key === optionKey)
				? { elementTypeKey: undefined, groupKey: optionKey }
				: { elementTypeKey: optionKey, groupKey: undefined }
			: { elementTypeKey: undefined, groupKey: undefined };

		this.value = value.map((permission, i) => (i === index ? { ...permission, ...setting } : permission));
		this.dispatchEvent(new UmbChangeEvent());
	}

	#setPermissionMinimumRange(e: UUIInputEvent, index: number) {
		const value = [...this.value];
		const input = e.target.value as string;

		this.value = value.map((permission, i) =>
			i === index ? { ...permission, minAllowed: parseInt(input) ?? 0 } : permission,
		);
		this.dispatchEvent(new UmbChangeEvent());
	}
	#setPermissionMaximumRange(e: UUIInputEvent, index: number) {
		const value = [...this.value];
		const input = e.target.value as string;

		this.value = value.map((permission, i) =>
			i === index ? { ...permission, maxAllowed: parseInt(input) ?? undefined } : permission,
		);
		this.dispatchEvent(new UmbChangeEvent());
	}

	#remove(index: number) {
		this.value = [...this.value].filter((_, i) => i !== index);
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		if (this._blockTypesWithElementName.length === 0) {
			return nothing;
		}
		return html`<div id="permissions">
				${repeat(
					this._value,
					(permission) => permission,
					(permission, index) => {
						const showCategoryHeader = this._blockGroups.length > 0 && this._blockTypesWithElementName.length > 0;

						return html`<div class="permission-setting">
							<uui-combobox
								@change=${(e: UUIComboboxEvent) => this.#setPermissionKey(e, index)}
								.value=${permission.elementTypeKey ?? permission.groupKey ?? ''}>
								<uui-combobox-list>
									${showCategoryHeader ? html`<strong>${this.localize.term('general_groups')}</strong>` : nothing}
									${this.#renderBlockGroups(permission)}
									${showCategoryHeader ? html`<strong>${this.localize.term('general_elements')}</strong>` : nothing}
									${this.#renderBlockTypes(permission)}
								</uui-combobox-list>
							</uui-combobox>
							<span>
								<uui-input
									type="number"
									placeholder="0"
									min="0"
									.value=${permission.minAllowed}
									@change=${(e: UUIInputEvent) => this.#setPermissionMinimumRange(e, index)}></uui-input>
								-
								<uui-input
									type="number"
									placeholder="&infin;"
									min="0"
									.value=${permission.maxAllowed}
									@change=${(e: UUIInputEvent) => this.#setPermissionMaximumRange(e, index)}></uui-input>
								<uui-button
									label=${this.localize.term('general_remove')}
									look="secondary"
									compact
									@click=${() => this.#remove(index)}>
									<uui-icon name="icon-trash"></uui-icon>
								</uui-button>
							</span>
						</div>`;
					},
				)}
			</div>
			<uui-button
				id="add-button"
				look="placeholder"
				label=${this.localize.term('general_add')}
				@click=${this.#addNewPermission}></uui-button>
			${!this._value.length
				? html`<small>
						<umb-localize key="blockEditor_areaAllowedBlocksEmpty">
							By default, all block types are allowed in an Area, Use this option to allow only selected types.
						</umb-localize>
					</small>`
				: nothing} `;
	}

	#renderBlockGroups(area: UmbBlockGridTypeAreaTypePermission) {
		return repeat(
			this._blockGroups,
			(group) => group.key,
			(group) =>
				html`<uui-combobox-list-option .value=${group.key} ?selected=${area.groupKey === group.key}>
					${group.name}
				</uui-combobox-list-option>`,
		);
	}

	#renderBlockTypes(area: UmbBlockGridTypeAreaTypePermission) {
		return repeat(
			this._blockTypesWithElementName,
			(block) => block.type.contentElementTypeKey,
			(block) =>
				html`<uui-combobox-list-option
					.value=${block.type.contentElementTypeKey}
					?selected=${area.elementTypeKey === block.type.contentElementTypeKey}>
					${block.name}
				</uui-combobox-list-option>`,
		);
	}

	static override styles = [
		UmbTextStyles,
		css`
			#permissions {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-3);
				margin-bottom: var(--uui-size-space-3);
			}

			#add-button {
				width: 100%;
			}

			.permission-setting {
				flex: 1;
				display: flex;
				gap: var(--uui-size-space-6);
			}

			.permission-setting > uui-combobox {
				flex: 1;
			}

			.permission-setting > span {
				display: flex;
				gap: var(--uui-size-space-1);
				align-items: center;
			}

			uui-combobox strong {
				padding: 0 var(--uui-size-space-1);
			}
		`,
	];
}

export default UmbPropertyEditorUIBlockGridAreaTypePermissionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-grid-area-type-permission': UmbPropertyEditorUIBlockGridAreaTypePermissionElement;
	}
}
