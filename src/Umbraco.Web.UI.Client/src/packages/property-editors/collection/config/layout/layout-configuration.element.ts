import { css, customElement, html, ifDefined, property, when } from '@umbraco-cms/backoffice/external/lit';
import { extractUmbColorVariable } from '@umbraco-cms/backoffice/resources';
import { simpleHashCode } from '@umbraco-cms/backoffice/observable-api';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_ICON_PICKER_MODAL } from '@umbraco-cms/backoffice/icon';
import type { UmbInputManifestElement } from '@umbraco-cms/backoffice/components';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import type { UmbSortableListElement } from '@umbraco-cms/backoffice/sorter';
import type { UUIInputElement, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

import '@umbraco-cms/backoffice/sorter';

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
	@property({ type: Array })
	public set value(value: Array<UmbCollectionLayoutConfiguration> | undefined) {
		this.#value = value ?? [];
	}
	public get value(): Array<UmbCollectionLayoutConfiguration> {
		return this.#value;
	}
	#value: Array<UmbCollectionLayoutConfiguration> = [];

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	async #focusNewItem() {
		await this.updateComplete;
		const list = this.shadowRoot?.querySelector('umb-sortable-list');
		await list?.updateComplete;
		const input = list?.shadowRoot?.querySelector('umb-sortable-list-item:last-of-type > uui-input') as
			| UUIInputElement
			| undefined;
		input?.focus();
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

		this.dispatchEvent(new UmbChangeEvent());

		this.#focusNewItem();
	}

	#onChangeLabel(e: UUIInputEvent, index: number) {
		e.stopPropagation();
		const values = [...(this.value ?? [])];
		values[index] = { ...values[index], name: e.target.value as string };
		this.value = values;
		this.dispatchEvent(new UmbChangeEvent());
	}

	async #onRemove(layout: UmbCollectionLayoutConfiguration, index: number) {
		await umbConfirmModal(this, {
			color: 'danger',
			headline: `#actions_remove?`,
			content: `#defaultdialogs_confirmremove ${layout.name ?? ''}?`,
			confirmLabel: '#actions_remove',
		});

		const values = [...(this.value ?? [])];
		values.splice(index, 1);
		this.value = values;

		this.dispatchEvent(new UmbChangeEvent());
	}

	async #onIconChange(icon: typeof UMB_ICON_PICKER_MODAL.VALUE, index: number) {
		const picked = await (await import('@umbraco-cms/backoffice/modal'))
			.umbOpenModal(this, UMB_ICON_PICKER_MODAL, { value: icon })
			.catch(() => undefined);
		if (!picked) return;

		const values = [...(this.value ?? [])];
		values[index] = { ...values[index], icon: `${picked.icon} color-${picked.color}` };
		this.value = values;
		this.dispatchEvent(new UmbChangeEvent());
	}

	#parseIcon(iconString: string | undefined): typeof UMB_ICON_PICKER_MODAL.VALUE {
		const [icon, color] = iconString?.split(' ') ?? [];
		return { icon, color: color?.replace('color-', '') };
	}

	#getUnique(layout: UmbCollectionLayoutConfiguration): string {
		return 'x' + simpleHashCode('' + layout.collectionView + layout.name + layout.icon).toString(16);
	}

	#onSort(e: Event) {
		this.value = (e.target as UmbSortableListElement<UmbCollectionLayoutConfiguration>).items;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<umb-sortable-list
				.items=${this.value}
				.getUnique=${(layout: UmbCollectionLayoutConfiguration) => this.#getUnique(layout)}
				.renderMethod=${(layout: UmbCollectionLayoutConfiguration, index: number) => this.#renderLayout(layout, index)}
				@change=${this.#onSort}></umb-sortable-list>
			${this.#renderInput()}
		`;
	}

	#renderInput() {
		return html`<umb-input-manifest extension-type="collectionView" @change=${this.#onAdd}></umb-input-manifest>`;
	}

	#renderLayout(layout: UmbCollectionLayoutConfiguration, index: number) {
		const icon = this.#parseIcon(layout.icon);
		const varName = icon.color ? extractUmbColorVariable(icon.color) : undefined;
		return html`
			<umb-sortable-list-item .unique=${this.#getUnique(layout)}>
				<div style="display:flex;align-items:center;gap:var(--uui-size-6);">
					<uui-button
						compact
						look="outline"
						label="pick icon"
						style="min-width:var(--uui-size-11);"
						@click=${() => this.#onIconChange(icon, index)}>
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
						style="flex:1;"
						@change=${(e: UUIInputEvent) => this.#onChangeLabel(e, index)}>
					</uui-input>
					<div class="alias" style="flex:1;word-break:break-all;">
						<code>${layout.collectionView}</code>
					</div>
				</div>
				<uui-action-bar slot="actions">
					<uui-button label=${this.localize.term('general_remove')} @click=${() => this.#onRemove(layout, index)}>
						<uui-icon name="icon-trash"></uui-icon> </uui-button
				></uui-action-bar>
			</umb-sortable-list-item>
		`;
	}

	static override readonly styles = [
		css`
			umb-sortable-list {
				display: block;
				margin-bottom: var(--uui-size-1);
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
