import type { UmbBlockGridTypeAreaTypePermission, UmbBlockGridTypeGroupType } from '../../index.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { html, customElement, property, css, state, repeat, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import { UMB_DATA_TYPE_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/data-type';
import type { UmbBlockTypeWithGroupKey } from '@umbraco-cms/backoffice/block-type';
import type { UUIComboboxElement, UUIComboboxEvent, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-property-editor-ui-block-grid-area-type-permission')
export class UmbPropertyEditorUIBlockGridAreaTypePermissionElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	@property({ type: Array })
	public get value(): Array<UmbBlockGridTypeAreaTypePermission> {
		return this._value;
	}
	public set value(value: Array<UmbBlockGridTypeAreaTypePermission>) {
		this._value = value ?? [];
	}
	@state()
	private _value: Array<UmbBlockGridTypeAreaTypePermission> = [];

	@state()
	private _blockTypes: Array<UmbBlockTypeWithGroupKey> = [];

	@state()
	private _blockGroups: Array<UmbBlockGridTypeGroupType> = [];

	constructor() {
		super();

		this.consumeContext(UMB_DATA_TYPE_WORKSPACE_CONTEXT, async (context) => {
			this.observe(
				await context.propertyValueByAlias<Array<UmbBlockTypeWithGroupKey>>('blocks'),
				(blockTypes) => {
					this._blockTypes = blockTypes ?? [];
				},
				'observeBlockType',
			);
			this.observe(
				await context.propertyValueByAlias<Array<UmbBlockGridTypeGroupType>>('blockGroups'),
				(blockGroups) => {
					this._blockGroups = blockGroups ?? [];
				},
				'observeBlockGroups',
			);
		}).passContextAliasMatches();
	}

	#addNewPermission() {
		this.value = [...this.value, { minAllowed: 0, maxAllowed: undefined }];
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
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
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	#setPermissionMinimumRange(e: UUIInputEvent, index: number) {
		const value = [...this.value];
		const input = e.target.value as string;

		this.value = value.map((permission, i) =>
			i === index ? { ...permission, minAllowed: parseInt(input) ?? 0 } : permission,
		);
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}
	#setPermissionMaximumRange(e: UUIInputEvent, index: number) {
		const value = [...this.value];
		const input = e.target.value as string;

		this.value = value.map((permission, i) =>
			i === index ? { ...permission, maxAllowed: parseInt(input) ?? undefined } : permission,
		);
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	#remove(index: number) {
		this.value = [...this.value].filter((_, i) => i !== index);
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`<div id="permissions">
				${repeat(
					this._value,
					(permission) => permission,
					(permission, index) => {
						const showCategoryHeader = this._blockGroups.length && this._blockTypes.length;

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
			this._blockTypes,
			(block) => block.contentElementTypeKey,
			(block) =>
				html`<uui-combobox-list-option
					.value=${block.contentElementTypeKey}
					?selected=${area.elementTypeKey === block.contentElementTypeKey}>
					${block.label}
				</uui-combobox-list-option>`,
		);
	}

	static styles = [
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
