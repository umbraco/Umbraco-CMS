import { UmbTiptapToolbarConfigurationContext } from '../contexts/tiptap-toolbar-configuration.context.js';
import type {
	UmbTiptapToolbarExtension,
	UmbTiptapToolbarGroupViewModel,
	UmbTiptapToolbarRowViewModel,
} from '../types.js';
import type { UmbTiptapToolbarValue } from '../../../components/types.js';
import { customElement, css, html, property, repeat, state, when, nothing } from '@umbraco-cms/backoffice/external/lit';
import { debounce } from '@umbraco-cms/backoffice/utils';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-property-editor-ui-tiptap-toolbar-configuration')
export class UmbPropertyEditorUiTiptapToolbarConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	#context = new UmbTiptapToolbarConfigurationContext(this);

	#currentDragItem?: { alias: string; fromPos?: [number, number, number] };

	#debouncedFilter = debounce((query: string) => {
		this._availableExtensions = this.#context.filterExtensions(query);
	}, 250);

	@state()
	private _availableExtensions: Array<UmbTiptapToolbarExtension> = [];

	@state()
	private _toolbar: Array<UmbTiptapToolbarRowViewModel> = [];

	@property({ attribute: false })
	set value(value: UmbTiptapToolbarValue | undefined) {
		if (!value) value = [[[]]];
		if (value === this.#value) return;
		this.#value = this.#context.migrateTinyMceToolbar(value);
	}
	get value(): UmbTiptapToolbarValue | undefined {
		return this.#context.cloneToolbarValue(this.#value);
	}
	#value?: UmbTiptapToolbarValue;

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_CONTEXT, (propertyContext) => {
			this.observe(this.#context.extensions, (extensions) => {
				this._availableExtensions = extensions;
			});

			this.observe(this.#context.reload, (reload) => {
				if (reload) {
					this.requestUpdate();
				}
			});

			this.observe(this.#context.toolbar, (toolbar) => {
				if (!toolbar.length) return;
				this._toolbar = toolbar;
				this.#value = toolbar.map((rows) => rows.data.map((groups) => [...groups.data]));
				propertyContext.setValue(this.#value);
			});
		});
	}

	protected override firstUpdated() {
		this.#context.setToolbar(this.value);
	}

	#onClick(item: UmbTiptapToolbarExtension) {
		const lastRow = (this.#value?.length ?? 1) - 1;
		const lastGroup = (this.#value?.[lastRow].length ?? 1) - 1;
		const lastItem = this.#value?.[lastRow][lastGroup].length ?? 0;
		this.#context.insertToolbarItem(item.alias, [lastRow, lastGroup, lastItem]);
	}

	#onDragStart(event: DragEvent, alias: string, fromPos?: [number, number, number]) {
		event.dataTransfer!.effectAllowed = 'move';
		this.#currentDragItem = { alias, fromPos };
	}

	#onDragOver(event: DragEvent) {
		event.preventDefault();
		event.dataTransfer!.dropEffect = 'move';
	}

	#onDragEnd(event: DragEvent) {
		event.preventDefault();
		if (event.dataTransfer?.dropEffect === 'none') {
			const { fromPos } = this.#currentDragItem ?? {};
			if (!fromPos) return;

			this.#context.removeToolbarItem(fromPos);
		}
	}

	#onDrop(event: DragEvent, toPos?: [number, number, number]) {
		event.preventDefault();
		const { alias, fromPos } = this.#currentDragItem ?? {};

		// Remove item if no destination position is provided
		if (fromPos && !toPos) {
			this.#context.removeToolbarItem(fromPos);
			return;
		}

		// Move item if both source and destination positions are available
		if (fromPos && toPos) {
			this.#context.moveToolbarItem(fromPos, toPos);
			return;
		}

		// Insert item if an alias and a destination position are provided
		if (alias && toPos) {
			this.#context.insertToolbarItem(alias, toPos);
		}
	}

	#onFilterInput(event: InputEvent & { target: HTMLInputElement }) {
		const query = (event.target.value ?? '').toLocaleLowerCase();
		this.#debouncedFilter(query);
	}

	override render() {
		return html`${this.#renderDesigner()} ${this.#renderAvailableItems()}`;
	}

	#renderAvailableItems() {
		return html`
			<uui-box id="toolbox" headline=${this.localize.term('tiptap_toobar_availableItems')}>
				<div slot="header-actions">
					<uui-input
						type="search"
						autocomplete="off"
						placeholder=${this.localize.term('placeholders_filter')}
						@input=${this.#onFilterInput}>
						<div slot="prepend">
							<uui-icon name="search"></uui-icon>
						</div>
					</uui-input>
				</div>
				<div class="available-items" dropzone="move" @drop=${this.#onDrop} @dragover=${this.#onDragOver}>
					${when(
						this._availableExtensions.length === 0,
						() =>
							html`<umb-localize key="tiptap_toobar_availableItemsEmpty"
								>There are no toolbar extensions to show</umb-localize
							>`,
						() => repeat(this._availableExtensions, (item) => this.#renderAvailableItem(item)),
					)}
				</div>
			</uui-box>
		`;
	}

	#renderAvailableItem(item: UmbTiptapToolbarExtension) {
		const forbidden = !this.#context.isExtensionEnabled(item.alias);
		const inUse = this.#context.isExtensionInUse(item.alias);
		return inUse || forbidden
			? nothing
			: html`
					<uui-button
						compact
						class=${forbidden ? 'forbidden' : ''}
						draggable="true"
						look=${forbidden ? 'placeholder' : 'outline'}
						?disabled=${forbidden || inUse}
						@click=${() => this.#onClick(item)}
						@dragstart=${(e: DragEvent) => this.#onDragStart(e, item.alias)}
						@dragend=${this.#onDragEnd}>
						<div class="inner">
							${when(item.icon, () => html`<umb-icon .name=${item.icon}></umb-icon>`)}
							<span>${this.localize.string(item.label)}</span>
						</div>
					</uui-button>
				`;
	}

	#renderDesigner() {
		return html`
			<div id="toolbar">
				<div id="rows">
					${repeat(
						this._toolbar,
						(row) => row.unique,
						(row, idx) => this.#renderRow(row, idx),
					)}
				</div>
				<uui-button
					id="btnAddRow"
					look="placeholder"
					label=${this.localize.term('tiptap_toolbar_addRow')}
					@click=${() => this.#context.insertToolbarRow(this._toolbar.length)}></uui-button>
			</div>
		`;
	}

	#renderRow(row?: UmbTiptapToolbarRowViewModel, rowIndex = 0) {
		if (!row) return nothing;
		const hideActionBar = this._toolbar.length === 1;
		return html`
			<uui-button-inline-create
				label=${this.localize.term('tiptap_toolbar_addRow')}
				@click=${() => this.#context?.insertToolbarRow(rowIndex)}></uui-button-inline-create>
			<div class="row">
				<div class="groups">
					<uui-button-inline-create
						vertical
						label=${this.localize.term('tiptap_toolbar_addGroup')}
						@click=${() => this.#context?.insertToolbarGroup(rowIndex, 0)}></uui-button-inline-create>
					${repeat(
						row.data,
						(group) => group.unique,
						(group, idx) => this.#renderGroup(group, rowIndex, idx),
					)}
				</div>
				${when(
					!hideActionBar,
					() => html`
						<uui-action-bar>
							<uui-button
								look="secondary"
								label=${this.localize.term('tiptap_toolbar_removeRow')}
								@click=${() => this.#context?.removeToolbarRow(rowIndex)}>
								<uui-icon name="icon-trash"></uui-icon>
							</uui-button>
						</uui-action-bar>
					`,
				)}
			</div>
		`;
	}

	#renderGroup(group?: UmbTiptapToolbarGroupViewModel, rowIndex = 0, groupIndex = 0) {
		if (!group) return nothing;
		const showActionBar = this._toolbar[rowIndex].data.length > 1 && group.data.length === 0;
		return html`
			<div
				class="group"
				dropzone="move"
				@dragover=${this.#onDragOver}
				@drop=${(e: DragEvent) => this.#onDrop(e, [rowIndex, groupIndex, group.data.length - 1])}>
				<div class="items">
					${when(
						group?.data.length === 0,
						() => html`<em><umb-localize key="toolbar_emptyGroup">Empty</umb-localize></em>`,
						() => html`${group!.data.map((alias, idx) => this.#renderItem(alias, rowIndex, groupIndex, idx))}`,
					)}
				</div>
				${when(
					showActionBar,
					() => html`
						<uui-action-bar>
							<uui-button
								look="secondary"
								label=${this.localize.term('tiptap_toolbar_removeGroup')}
								@click=${() => this.#context?.removeToolbarGroup(rowIndex, groupIndex)}>
								<uui-icon name="icon-trash"></uui-icon>
							</uui-button>
						</uui-action-bar>
					`,
				)}
			</div>
			<uui-button-inline-create
				vertical
				label=${this.localize.term('tiptap_toolbar_addGroup')}
				@click=${() => this.#context?.insertToolbarGroup(rowIndex, groupIndex + 1)}></uui-button-inline-create>
		`;
	}

	#renderItem(alias: string, rowIndex = 0, groupIndex = 0, itemIndex = 0) {
		const item = this.#context?.getExtensionByAlias(alias);
		if (!item) return nothing;
		const forbidden = !this.#context?.isExtensionEnabled(item.alias);
		return html`
			<uui-button
				compact
				class=${forbidden ? 'forbidden' : ''}
				draggable="true"
				look=${forbidden ? 'placeholder' : 'outline'}
				title=${this.localize.string(item.label)}
				?disabled=${forbidden}
				@click=${() => this.#context.removeToolbarItem([rowIndex, groupIndex, itemIndex])}
				@dragend=${this.#onDragEnd}
				@dragstart=${(e: DragEvent) => this.#onDragStart(e, alias, [rowIndex, groupIndex, itemIndex])}>
				<div class="inner">
					${when(
						item.icon,
						() => html`<umb-icon .name=${item.icon}></umb-icon>`,
						() => html`<span>${this.localize.string(item.label)}</span>`,
					)}
				</div>
			</uui-button>
		`;
	}

	static override readonly styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-4);
				flex-grow: 1;
			}

			@media (min-width: 1400px) {
				:host {
					flex-direction: row;
				}
				#toolbox {
					width: 500px;
					max-width: 33%;
					flex-grow: 1;
				}

				#toolbar {
					flex-grow: 100;
				}
			}

			#toolbox {
				border: 1px solid var(--uui-color-border);
			}

			uui-box.minimal {
				--uui-box-header-padding: 0;
				--uui-box-default-padding: var(--uui-size-2) 0;
				--uui-box-box-shadow: none;

				[slot='header-actions'] {
					margin-bottom: var(--uui-size-2);

					uui-icon {
						color: var(--uui-color-border);
					}
				}
			}

			.available-items {
				display: flex;
				flex-wrap: wrap;
				gap: var(--uui-size-3);

				uui-button {
					--uui-button-font-weight: normal;

					&[draggable='true'],
					&[draggable='true'] > .inner {
						cursor: move;
					}

					&[disabled],
					&[disabled] > .inner {
						cursor: not-allowed;
					}

					&.forbidden {
						--color: var(--uui-color-danger);
						--color-standalone: var(--uui-color-danger-standalone);
						--color-emphasis: var(--uui-color-danger-emphasis);
						--color-contrast: var(--uui-color-danger);
						--uui-button-contrast-disabled: var(--uui-color-danger);
						--uui-button-border-color-disabled: var(--uui-color-danger);
					}

					div {
						display: flex;
						gap: var(--uui-size-1);
					}
				}
			}

			uui-button-inline-create:not([vertical]) {
				margin-bottom: -4px;
			}

			#rows {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-1);

				.row {
					display: flex;
					align-items: flex-start;
					justify-content: space-between;
					gap: var(--uui-size-3);

					&:not(:last-child) {
						border-bottom: 1px solid var(--uui-color-border);
					}

					&:focus-within,
					&:hover {
						border-color: var(--uui-color-border-standalone);
					}

					.groups {
						flex: 1;
						display: flex;
						flex-direction: row;
						flex-wrap: wrap;
						align-items: center;
						justify-content: flex-start;
						gap: var(--uui-size-1);

						uui-button-inline-create {
							height: 50px;
						}

						.group {
							display: flex;
							flex-direction: row;
							align-items: center;
							justify-content: space-between;
							gap: var(--uui-size-3);

							border: 1px dashed transparent;
							border-radius: var(--uui-border-radius);
							padding: var(--uui-size-3);

							> uui-action-bar {
								opacity: 0;
								transition: opacity 120ms;
							}

							&:focus-within,
							&:hover {
								border-color: var(--uui-color-border-standalone);
								> uui-action-bar {
									opacity: 1;
								}
							}

							.items {
								display: flex;
								flex-direction: row;
								flex-wrap: wrap;
								gap: var(--uui-size-1);

								uui-button {
									--uui-button-font-weight: normal;

									&[draggable='true'],
									&[draggable='true'] > .inner {
										cursor: move;
									}

									&[disabled],
									&[disabled] > .inner {
										cursor: not-allowed;
									}

									&.forbidden {
										--color: var(--uui-color-danger);
										--color-standalone: var(--uui-color-danger-standalone);
										--color-emphasis: var(--uui-color-danger-emphasis);
										--color-contrast: var(--uui-color-danger);
										--uui-button-contrast-disabled: var(--uui-color-danger);
										--uui-button-border-color-disabled: var(--uui-color-danger);
									}

									div {
										display: flex;
										gap: var(--uui-size-1);
									}
								}
							}
						}
					}
				}
			}

			#btnAddRow {
				display: block;
				margin-top: var(--uui-size-1);
			}

			.handle {
				cursor: move;
			}
		`,
	];
}

export { UmbPropertyEditorUiTiptapToolbarConfigurationElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiptap-toolbar-configuration': UmbPropertyEditorUiTiptapToolbarConfigurationElement;
	}
}
