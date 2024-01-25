import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { html, customElement, property, repeat, css, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import type { UUIBooleanInputEvent, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { extractUmbColorVariable } from '@umbraco-cms/backoffice/resources';
import type {
	UmbModalManagerContext} from '@umbraco-cms/backoffice/modal';
import {
	UMB_ICON_PICKER_MODAL,
	UMB_MODAL_MANAGER_CONTEXT
} from '@umbraco-cms/backoffice/modal';
import type {
	UmbPropertyEditorConfigCollection} from '@umbraco-cms/backoffice/property-editor';
import {
	UmbPropertyValueChangeEvent,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

interface LayoutConfig {
	icon?: string;
	isSystem?: boolean;
	name?: string;
	path?: string;
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
	@property({ type: Array })
	value: Array<LayoutConfig> = [];

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	private _modalContext?: UmbModalManagerContext;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this._modalContext = instance;
		});
	}

	#onAdd() {
		this.value = [...this.value, { isSystem: false, icon: 'icon-stop', selected: true }];
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	#onRemove(unique: number) {
		const values = [...this.value];
		values.splice(unique, 1);
		this.value = values;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	#onChangePath(e: UUIInputEvent, index: number) {
		const values = [...this.value];
		values[index] = { ...values[index], path: e.target.value as string };
		this.value = values;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	#onChangeName(e: UUIInputEvent, index: number) {
		const values = [...this.value];
		values[index] = { ...values[index], name: e.target.value as string };
		this.value = values;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	#onChangeSelected(e: UUIBooleanInputEvent, index: number) {
		const values = [...this.value];
		values[index] = { ...values[index], selected: e.target.checked };
		this.value = values;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	async #onIconChange(index: number) {
		const icon = this.#iconReader(this.value[index].icon ?? '');

		// TODO: send icon data to modal
		const modalContext = this._modalContext?.open(UMB_ICON_PICKER_MODAL);
		const picked = await modalContext?.onSubmit();
		if (!picked) return;

		const values = [...this.value];
		values[index] = { ...values[index], icon: `${picked.icon} color-${picked.color}` };
		this.value = values;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`<div id="layout-wrapper">
				${repeat(
					this.value,
					(layout, index) => '' + layout.name + layout.icon,
					(layout, index) =>
						html` <div class="layout-item">
							<uui-icon name="icon-navigation"></uui-icon> ${layout.isSystem
								? this.renderSystemFieldRow(layout, index)
								: this.renderCustomFieldRow(layout, index)}
						</div>`,
				)}
			</div>
			<uui-button
				id="add"
				label=${this.localize.term('general_add')}
				look="placeholder"
				@click=${this.#onAdd}></uui-button>`;
	}

	#iconReader(iconString: string): { icon: string; color?: string } {
		if (!iconString) return { icon: '' };

		const parts = iconString.split(' ');

		if (parts.length === 2) {
			const [icon, color] = parts;
			const varName = extractUmbColorVariable(color.replace('color-', ''));
			return { icon, color: varName };
		} else {
			const [icon] = parts;
			return { icon };
		}
	}

	renderSystemFieldRow(layout: LayoutConfig, index: number) {
		const icon = this.#iconReader(layout.icon ?? '');

		return html` <uui-button compact disabled label="Icon" look="outline">
				<uui-icon name=${ifDefined(icon.icon)}></uui-icon>
			</uui-button>
			${index}
			<span><strong>${ifDefined(layout.name)}</strong> <small>(system field)</small></span>
			<uui-checkbox
				?checked=${layout.selected}
				label="Show"
				@change=${(e: UUIBooleanInputEvent) => this.#onChangeSelected(e, index)}>
			</uui-checkbox>`;
	}

	renderCustomFieldRow(layout: LayoutConfig, index: number) {
		const icon = this.#iconReader(layout.icon ?? '');

		return html`<uui-button compact look="outline" label="pick icon" @click=${() => this.#onIconChange(index)}>
				${icon.color
					? html`<uui-icon name=${icon.icon} style="color:var(${icon.color})"></uui-icon>`
					: html`<uui-icon name=${icon.icon}></uui-icon>`}
			</uui-button>
			${index}
			<uui-input
				label="name"
				value=${ifDefined(layout.name)}
				placeholder="Name..."
				@change=${(e: UUIInputEvent) => this.#onChangeName(e, index)}></uui-input>
			<uui-input
				label="path"
				value=${ifDefined(layout.path)}
				placeholder="Layout path..."
				@change=${(e: UUIInputEvent) => this.#onChangePath(e, index)}></uui-input>
			<uui-button
				label=${this.localize.term('actions_remove')}
				look="secondary"
				@click=${() => this.#onRemove(index)}></uui-button>`;
	}

	static styles = [
		UmbTextStyles,
		css`
			#layout-wrapper {
				display: flex;
				flex-direction: column;
				gap: 1px;
				margin-bottom: var(--uui-size-3);
			}

			.layout-item {
				background-color: var(--uui-color-surface-alt);
				display: flex;
				align-items: center;
				gap: var(--uui-size-6);
				padding: var(--uui-size-3) var(--uui-size-6);
			}

			.layout-item > :last-child {
				margin-left: auto;
			}

			#add {
				width: 100%;
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
