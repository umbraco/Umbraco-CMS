import {
	css,
	customElement,
	html,
	ifDefined,
	nothing,
	property,
	repeat,
	when,
} from '@umbraco-cms/backoffice/external/lit';
import { extractUmbColorVariable } from '@umbraco-cms/backoffice/resources';
import { simpleHashCode } from '@umbraco-cms/backoffice/observable-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { UmbInputManifestElement } from '@umbraco-cms/backoffice/components';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import type { UUIInputElement, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { UMB_ICON_PICKER_MODAL } from '@umbraco-cms/backoffice/icon';

interface UmbCollectionLayoutConfiguration {
	icon?: string;
	name?: string;
	collectionView?: string;
	isSystem?: boolean;
	selected?: boolean;
}

/**
 * @element umb-property-editor-ui-collection-layout-configuration
 */
@customElement('umb-property-editor-ui-collection-layout-configuration')
export class UmbPropertyEditorUICollectionLayoutConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	#sorter = new UmbSorterController<UmbCollectionLayoutConfiguration>(this, {
		getUniqueOfElement: (element) => {
			return element.id;
		},
		getUniqueOfModel: (modelEntry) => {
			return this.#getUnique(modelEntry);
		},
		itemSelector: '.layout-item',
		containerSelector: '#layout-wrapper',
		onChange: ({ model }) => {
			this.value = model;
			this.dispatchEvent(new UmbPropertyValueChangeEvent());
		},
	});

	@property({ type: Array })
	public set value(value: Array<UmbCollectionLayoutConfiguration> | undefined) {
		this.#value = value ?? [];
		this.#sorter.setModel(this.#value);
	}
	public get value(): Array<UmbCollectionLayoutConfiguration> {
		return this.#value;
	}
	#value: Array<UmbCollectionLayoutConfiguration> = [];

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	async #focusNewItem() {
		await this.updateComplete;
		const input = this.shadowRoot?.querySelector('.layout-item:last-of-type > uui-input') as UUIInputElement;
		input.focus();
	}

	#onAdd(event: { target: UmbInputManifestElement }) {
		const manifest = event.target.value;

		const duplicate = this.value?.find((config) => manifest?.value === config.collectionView);

		if (duplicate) {
			// TODO: Show error to user, can not add duplicate `collectionView` aliases. [LK]
			throw new Error('Duplicate `collectionView` aliases are not allowed.');
			return;
		}

		this.value = [
			...(this.value ?? []),
			{
				icon: manifest?.icon,
				name: manifest?.label,
				collectionView: manifest?.value,
			},
		];

		this.dispatchEvent(new UmbPropertyValueChangeEvent());

		this.#focusNewItem();
	}

	#onChangeLabel(e: UUIInputEvent, index: number) {
		const values = [...(this.value ?? [])];
		values[index] = { ...values[index], name: e.target.value as string };
		this.value = values;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	#onRemove(unique: number) {
		const values = [...(this.value ?? [])];
		values.splice(unique, 1);
		this.value = values;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	async #onIconChange(icon: typeof UMB_ICON_PICKER_MODAL.VALUE, index: number) {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modal = modalManager.open(this, UMB_ICON_PICKER_MODAL, { value: icon });
		const picked = await modal?.onSubmit();
		if (!picked) return;

		const values = [...(this.value ?? [])];
		values[index] = { ...values[index], icon: `${picked.icon} color-${picked.color}` };
		this.value = values;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	#parseIcon(iconString: string | undefined): typeof UMB_ICON_PICKER_MODAL.VALUE {
		const [icon, color] = iconString?.split(' ') ?? [];
		return { icon, color: color?.replace('color-', '') };
	}

	#getUnique(layout: UmbCollectionLayoutConfiguration): string {
		return 'x' + simpleHashCode('' + layout.collectionView + layout.name + layout.icon).toString(16);
	}

	override render() {
		return html`
			<div id="layout-wrapper">${this.#renderLayouts()}</div>
			${this.#renderInput()}
		`;
	}

	#renderInput() {
		return html`<umb-input-manifest extension-type="collectionView" @change=${this.#onAdd}></umb-input-manifest>`;
	}

	#renderLayouts() {
		if (!this.value) return nothing;
		return repeat(
			this.value,
			(layout) => this.#getUnique(layout),
			(layout, index) => this.#renderLayout(layout, index),
		);
	}

	#renderLayout(layout: UmbCollectionLayoutConfiguration, index: number) {
		const icon = this.#parseIcon(layout.icon);
		const varName = icon.color ? extractUmbColorVariable(icon.color) : undefined;
		return html`
			<div class="layout-item" id=${this.#getUnique(layout)}>
				<uui-icon class="drag-handle" name="icon-drag-vertical"></uui-icon>

				<uui-button compact look="outline" label="pick icon" @click=${() => this.#onIconChange(icon, index)}>
					${when(
						icon.color,
						() => html`<uui-icon name=${ifDefined(icon.icon)} style="color:var(${varName})"></uui-icon>`,
						() => html`<uui-icon name=${ifDefined(icon.icon)}></uui-icon>`,
					)}
				</uui-button>

				<uui-input
					label="name"
					value=${ifDefined(layout.name)}
					placeholder="Enter a label..."
					@change=${(e: UUIInputEvent) => this.#onChangeLabel(e, index)}></uui-input>

				<div class="alias">
					<code>${layout.collectionView}</code>
				</div>

				<div class="actions">
					<uui-button
						label=${this.localize.term('general_remove')}
						look="secondary"
						@click=${() => this.#onRemove(index)}></uui-button>
				</div>
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#layout-wrapper {
				display: flex;
				flex-direction: column;
				gap: 1px;
				margin-bottom: var(--uui-size-1);
			}

			.layout-item {
				background-color: var(--uui-color-surface-alt);
				display: flex;
				align-items: center;
				gap: var(--uui-size-6);
				padding: var(--uui-size-3) var(--uui-size-6);
			}

			.layout-item > uui-icon {
				flex: 0 0 var(--uui-size-6);
			}

			.layout-item > uui-button {
				flex: 0 0 var(--uui-size-10);
			}

			.layout-item > uui-input,
			.layout-item > .alias {
				flex: 1;
			}

			.layout-item > .actions {
				flex: 0 0 auto;
				display: flex;
				justify-content: flex-end;
			}

			.drag-handle {
				cursor: grab;
			}

			.drag-handle:active {
				cursor: grabbing;
			}
		`,
	];
}

export default UmbPropertyEditorUICollectionLayoutConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-collection-layout-configuration': UmbPropertyEditorUICollectionLayoutConfigurationElement;
	}
}
