import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { customElement, css, html, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import {
	UmbPropertyValueChangeEvent,
	type UmbPropertyEditorConfigCollection,
	type UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';

type ExtensionConfig = {
	alias: string;
	label: string;
	icon?: string;
	group: string;
};

type ExtensionCategoryItem = {
	alias: string;
	label: string;
	icon?: string;
	selected: boolean;
};

type ExtensionCategory = {
	category: string;
	extensions: ExtensionCategoryItem[];
};

@customElement('umb-property-editor-ui-tiptap-extensions-configuration')
export class UmbPropertyEditorUiTiptapExtensionsConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	@property({ attribute: false })
	set value(value: string[] | undefined) {
		this.#value = value;
	}
	get value(): string[] | undefined {
		return this.#value;
	}

	#value?: string[] = [];

	@property({ attribute: false })
	config?: UmbPropertyEditorConfigCollection;

	@state()
	private _extensionCategories: ExtensionCategory[] = [];

	@state()
	private _extensionConfigs: ExtensionConfig[] = [];

	protected override async firstUpdated(_changedProperties: PropertyValueMap<unknown>) {
		super.firstUpdated(_changedProperties);

		this.observe(umbExtensionsRegistry.byType('tiptapExtension'), (extensions) => {
			this._extensionConfigs = extensions.map((ext) => {
				return {
					alias: ext.alias,
					label: ext.meta.label,
					icon: ext.meta.icon,
					group: ext.meta.group,
				};
			});

			if (!this.value) {
				// The default value is all extensions enabled
				this.#value = this._extensionConfigs.map((ext) => ext.alias);
				this.dispatchEvent(new UmbPropertyValueChangeEvent());
			}

			this.#setupExtensionCategories();
		});
	}

	#setupExtensionCategories() {
		const useDefault = !this.value; // The default value is all extensions enabled
		const withSelectedProperty = this._extensionConfigs.map((extensionConfig) => {
			return {
				...extensionConfig,
				selected: useDefault ? true : this.value!.includes(extensionConfig.alias),
			};
		});

		const grouped = withSelectedProperty.reduce((acc: any, item) => {
			const group = item.group || 'Uncategorized'; // Assign to "Uncategorized" if no group
			if (!acc[group]) {
				acc[group] = [];
			}
			acc[group].push(item);
			return acc;
		}, {});
		this._extensionCategories = Object.keys(grouped).map((group) => ({
			category: group.charAt(0).toUpperCase() + group.slice(1).replace(/-/g, ' '),
			extensions: grouped[group],
		}));
	}

	#onExtensionClick(item: ExtensionCategoryItem) {
		item.selected = !item.selected;

		if (!this.value) {
			this.value = [];
		}

		if (item.selected) {
			this.#value = [...this.value, item.alias];
		} else {
			this.#value = this.value.filter((alias) => alias !== item.alias);
		}

		this.requestUpdate('_extensionCategories');
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		return html`
			<div class="extensions">
				${repeat(
					this._extensionCategories,
					(category) => html`
						<div class="category">
							<p class="category-name">${category.category}</p>
							${repeat(
								category.extensions,
								(item) =>
									html`<div class="extension-item">
										<uui-button
											compact
											look="outline"
											class=${item.selected ? 'selected' : ''}
											label=${item.label}
											.value=${item.alias}
											@click=${() => this.#onExtensionClick(item)}
											><umb-icon name=${item.icon ?? ''}></umb-icon
										></uui-button>
										<span>${item.label}</span>
									</div>`,
							)}
						</div>
					`,
				)}
					</div>
			</div>
		`;
	}

	static override readonly styles = [
		UmbTextStyles,
		css`
			uui-icon {
				width: unset;
				height: unset;
				display: flex;
				vertical-align: unset;
			}
			uui-button.selected {
				--uui-button-border-color: var(--uui-color-selected);
				--uui-button-border-width: 2px;
			}
			.extensions {
				display: flex;
				flex-wrap: wrap;
				gap: 16px;
				margin-top: 16px;
			}
			.extension-item {
				display: grid;
				grid-template-columns: 36px 1fr;
				grid-template-rows: 1fr;
				align-items: center;
				gap: 9px;
			}
			.category {
				flex: 1;
				background-color: var(--uui-color-surface-alt);
				padding: 12px;
				border-radius: 6px;
				display: flex;
				flex-direction: column;
				gap: 6px;
				border: 1px solid var(--uui-color-border);
			}
			.category-name {
				grid-column: 1 / -1;
				margin: 0;
				font-weight: bold;
				display: flex;
			}
		`,
	];
}

export default UmbPropertyEditorUiTiptapExtensionsConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiptap-extensions-configuration': UmbPropertyEditorUiTiptapExtensionsConfigurationElement;
	}
}
