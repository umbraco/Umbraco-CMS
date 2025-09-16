import type { UmbTiptapStatusbarValue } from '../../components/types.js';
import type { UmbTiptapStatusbarViewModel, UmbTiptapStatusbarExtension } from '../types.js';
import { UmbTiptapStatusbarConfigurationContext } from './tiptap-statusbar-configuration.context.js';
import { customElement, css, html, property, when, repeat, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { debounce } from '@umbraco-cms/backoffice/utils';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-property-editor-ui-tiptap-statusbar-configuration')
export class UmbPropertyEditorUiTiptapStatusbarConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	#context = new UmbTiptapStatusbarConfigurationContext(this);

	#currentDragItem?: { alias: string; fromPos?: [number, number] };

	#debouncedFilter = debounce((query: string) => {
		this._availableExtensions = this.#context.filterExtensions(query);
	}, 250);

	#initialized = false;

	@state()
	private _availableExtensions: Array<UmbTiptapStatusbarExtension> = [];

	@state()
	private _statusbar: Array<UmbTiptapStatusbarViewModel> = [];

	@property({ attribute: false })
	set value(value: UmbTiptapStatusbarValue | undefined) {
		if (!value) value = [[], []];
		if (value === this.#value) return;
		this.#value = value;
	}
	get value(): UmbTiptapStatusbarValue | undefined {
		if (this.#value === undefined) return undefined;
		return this.#context.cloneStatusbarValue(this.#value);
	}
	#value?: UmbTiptapStatusbarValue;

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_CONTEXT, (propertyContext) => {
			this.observe(this.#context.extensions, (extensions) => {
				this._availableExtensions = extensions;
			});

			this.observe(this.#context.reload, (reload) => {
				if (reload) this.requestUpdate();
			});

			this.observe(this.#context.statusbar, (statusbar) => {
				if (!statusbar.length) return;
				this._statusbar = statusbar;
				if (this.#initialized) {
					this.#value = statusbar.map((area) => [...area.data]);
					propertyContext?.setValue(this.#value);
				}
			});
		});
	}

	protected override firstUpdated() {
		this.#context.setStatusbar(this.#value);
		this.#initialized = true;
	}

	#onClick(item: UmbTiptapStatusbarExtension) {
		const lastArea = (this.#value?.length ?? 1) - 1;
		const lastItem = this.#value?.[lastArea].length ?? 0;
		this.#context.insertStatusbarItem(item.alias, [lastArea, lastItem]);
	}

	#onDragStart(event: DragEvent, alias: string, fromPos?: [number, number]) {
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

			this.#context.removeStatusbarItem(fromPos);
		}
	}

	#onDrop(event: DragEvent, toPos?: [number, number]) {
		event.preventDefault();
		const { alias, fromPos } = this.#currentDragItem ?? {};

		// Remove item if no destination position is provided
		if (fromPos && !toPos) {
			this.#context.removeStatusbarItem(fromPos);
			return;
		}

		// Move item if both source and destination positions are available
		if (fromPos && toPos) {
			this.#context.moveStatusbarItem(fromPos, toPos);
			return;
		}

		// Insert item if an alias and a destination position are provided
		if (alias && toPos) {
			this.#context.insertStatusbarItem(alias, toPos);
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
		const label = this.localize.term('placeholders_filter');
		return html`
			<uui-box id="toolbox" headline=${this.localize.term('tiptap_statusbar_availableItems')}>
				<div slot="header-actions">
					<uui-input type="search" autocomplete="off" label=${label} placeholder=${label} @input=${this.#onFilterInput}>
						<div slot="prepend">
							<uui-icon name="search"></uui-icon>
						</div>
					</uui-input>
				</div>
				<uui-scroll-container>
					<div class="available-items" dropzone="move" @drop=${this.#onDrop} @dragover=${this.#onDragOver}>
						${when(
							this._availableExtensions.length === 0,
							() =>
								html`<umb-localize key="tiptap_statusbar_availableItemsEmpty"
									>There are no statusbar extensions to show</umb-localize
								>`,
							() => repeat(this._availableExtensions, (item) => this.#renderAvailableItem(item)),
						)}
					</div>
				</uui-scroll-container>
			</uui-box>
		`;
	}

	#renderAvailableItem(item: UmbTiptapStatusbarExtension) {
		const forbidden = !this.#context.isExtensionEnabled(item.alias);
		const inUse = this.#context.isExtensionInUse(item.alias);
		if (inUse || forbidden) return nothing;
		const label = this.localize.string(item.label);
		return html`
			<uui-button
				compact
				class=${forbidden ? 'forbidden' : ''}
				data-mark="tiptap-toolbar-item:${item.alias}"
				draggable="true"
				label=${label}
				look=${forbidden ? 'placeholder' : 'outline'}
				?disabled=${forbidden || inUse}
				@click=${() => this.#onClick(item)}
				@dragstart=${(e: DragEvent) => this.#onDragStart(e, item.alias)}
				@dragend=${this.#onDragEnd}>
				<div class="inner">
					${when(item.icon, () => html`<umb-icon .name=${item.icon}></umb-icon>`)}
					<span>${label}</span>
				</div>
			</uui-button>
		`;
	}

	#renderDesigner() {
		return html`
			<div id="statusbar">
				<div id="areas">
					${repeat(
						this._statusbar,
						(area) => area.unique,
						(area, idx) => this.#renderArea(area, idx),
					)}
				</div>
			</div>
		`;
	}

	#renderArea(area?: UmbTiptapStatusbarViewModel, areaIndex = 0) {
		if (!area) return nothing;
		return html`
			<div
				class="area"
				dropzone="move"
				@dragover=${this.#onDragOver}
				@drop=${(e: DragEvent) => this.#onDrop(e, [areaIndex, area.data.length - 1])}>
				<div class="items">
					${when(
						area?.data.length === 0,
						() => html`<em><umb-localize key="tiptap_toolbar_emptyGroup">Empty</umb-localize></em>`,
						() => html`${area!.data.map((alias, idx) => this.#renderItem(alias, areaIndex, idx))}`,
					)}
				</div>
			</div>
		`;
	}

	#renderItem(alias: string, areaIndex = 0, itemIndex = 0) {
		const item = this.#context?.getExtensionByAlias(alias);
		if (!item) return nothing;

		const forbidden = !this.#context?.isExtensionEnabled(item.alias);
		const label = this.localize.string(item.label);

		return html`
			<uui-button
				compact
				class=${forbidden ? 'forbidden' : ''}
				data-mark="tiptap-toolbar-item:${item.alias}"
				draggable="true"
				label=${label}
				look=${forbidden ? 'placeholder' : 'outline'}
				title=${label}
				?disabled=${forbidden}
				@click=${() => this.#context.removeStatusbarItem([areaIndex, itemIndex])}
				@dragend=${this.#onDragEnd}
				@dragstart=${(e: DragEvent) => this.#onDragStart(e, alias, [areaIndex, itemIndex])}>
				<div class="inner">
					${when(item.icon, (icon) => html`<umb-icon .name=${icon}></umb-icon>`)}
					<span>${label}</span>
				</div>
			</uui-button>
		`;
	}

	static override readonly styles = [
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

				#statusbar {
					flex-grow: 100;
				}
			}

			#toolbox {
				border: 1px solid var(--uui-color-border);
			}

			uui-box {
				[slot='header-actions'] {
					margin-bottom: var(--uui-size-2);

					uui-icon {
						color: var(--uui-color-border);
					}
				}
			}

			uui-scroll-container {
				max-height: 350px;
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
						align-items: flex-end;
					}
				}
			}

			#areas {
				display: flex;
				gap: var(--uui-size-1);
				justify-content: space-between;
				align-items: center;

				.area {
					flex: 1;
					display: flex;
					align-items: flex-start;
					justify-content: space-between;

					border: 1px dashed transparent;
					padding: var(--uui-size-3);

					&:last-child {
						justify-content: flex-end;
					}

					&:focus-within,
					&:hover {
						border-color: var(--uui-color-border-standalone);
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
								align-items: flex-end;
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

export { UmbPropertyEditorUiTiptapStatusbarConfigurationElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiptap-statusbar-configuration': UmbPropertyEditorUiTiptapStatusbarConfigurationElement;
	}
}
