import {
	html,
	customElement,
	property,
	repeat,
	css,
	ifDefined,
	nothing,
	when,
} from '@umbraco-cms/backoffice/external/lit';
import { extractUmbColorVariable } from '@umbraco-cms/backoffice/resources';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_ICON_PICKER_MODAL, UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { UmbInputManifestElement } from '@umbraco-cms/backoffice/components';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import type { UUIInputElement, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

interface UmbCollectionLayoutConfig {
	icon?: string;
	name?: string;
	collectionView?: string;
	isSystem?: boolean;
	selected?: boolean;
}

/**
 * @element umb-property-editor-ui-collection-view-layout-configuration
 */
@customElement('umb-property-editor-ui-collection-view-layout-configuration')
export class UmbPropertyEditorUICollectionViewLayoutConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	// TODO: [LK] Add sorting.

	@property({ type: Array })
	value?: Array<UmbCollectionLayoutConfig>;

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	async #focusNewItem() {
		await this.updateComplete;
		const input = this.shadowRoot?.querySelector('.layout-item:last-of-type > uui-input') as UUIInputElement;
		input.focus();
	}

	#onAdd(event: { target: UmbInputManifestElement }) {
		const manifest = event.target.value;

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

	render() {
		if (!this.value) return nothing;
		return html`
			<div id="layout-wrapper">
				${repeat(
					this.value,
					(layout, index) => '' + index + layout.name + layout.icon,
					(layout, index) => this.#renderLayout(layout, index),
				)}
			</div>
			<umb-input-manifest extension-type="collectionView" @change=${this.#onAdd}></umb-input-manifest>
		`;
	}

	#renderLayout(layout: UmbCollectionLayoutConfig, index: number) {
		const icon = this.#parseIcon(layout.icon);
		const varName = icon.color ? extractUmbColorVariable(icon.color) : undefined;

		return html`
			<div class="layout-item">
				<uui-icon name="icon-navigation"></uui-icon>

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

	static styles = [
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
		`,
	];
}

export default UmbPropertyEditorUICollectionViewLayoutConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-collection-view-layout-configuration': UmbPropertyEditorUICollectionViewLayoutConfigurationElement;
	}
}
