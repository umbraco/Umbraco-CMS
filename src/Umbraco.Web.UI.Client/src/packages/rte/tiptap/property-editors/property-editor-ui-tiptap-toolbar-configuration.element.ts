import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { customElement, css, html, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import {
	UmbPropertyValueChangeEvent,
	type UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';

import './tiptap-toolbar-groups-configuration.element.js';

import { tinymce } from '@umbraco-cms/backoffice/external/tinymce';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

const tinyIconSet = tinymce.IconManager.get('default');

// If an extension exists in the extensions array but not in the toolbarLayout, it means that the extension is hidden in the toolbar
type ServerValue = {
	extensions: Array<string>;
	toolbarLayout: string[][][];
};

type ExtensionConfig = {
	alias: string;
	label: string;
	icon?: string;
	selected: boolean;
	category: string;
};

type ExtensionCategory = {
	category: string;
	extensions: ExtensionConfig[];
};

@customElement('umb-property-editor-ui-tiptap-toolbar-configuration')
export class UmbPropertyEditorUiTiptapToolbarConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	@property({ attribute: false })
	set value(value: ServerValue) {
		if (!value) value = { extensions: [], toolbarLayout: [] };

		this.#value = value;
	}
	get value(): ServerValue {
		return this.#value;
	}

	#value: ServerValue = { extensions: [], toolbarLayout: [] };

	@property({ attribute: false })
	config?: UmbPropertyEditorConfigCollection;

	@state()
	private _extensionCategories: ExtensionCategory[] = [];

	@state()
	private _availableExtensions: ExtensionConfig[] = [];

	#toolbarLayout = new UmbArrayState<string[][]>([[[]]], (x) => x);
	toolbarLayout = this.#toolbarLayout.asObservable();

	constructor() {
		super();
		this.provideContext('umb-tiptap-toolbar-context', {
			state: this.#toolbarLayout,
			observable: this.toolbarLayout,
		});

		this.toolbarLayout.subscribe((value) => {
			this.#value = { ...this.#value, toolbarLayout: value };

			this.dispatchEvent(new UmbPropertyValueChangeEvent());
		});
	}

	protected override async firstUpdated(_changedProperties: PropertyValueMap<unknown>) {
		super.firstUpdated(_changedProperties);

		this.#toolbarLayout.setValue(this.#value.toolbarLayout);

		const toolbarConfigValue = this.config?.getValueByAlias<ExtensionConfig[]>('toolbar');
		if (!toolbarConfigValue) return;

		this._availableExtensions = toolbarConfigValue.map((v) => {
			return {
				...v,
				selected: this.#value.extensions.includes(v.alias),
			};
		});

		const grouped = this._availableExtensions.reduce((acc: any, item) => {
			const group = item.category || 'miscellaneous'; // Assign to "miscellaneous" if no group

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

		this.requestUpdate('_toolbarConfig');
		this.requestUpdate('_availableExtensions');
	}

	#onExtensionSelect(item: ExtensionConfig) {
		item.selected = !item.selected;

		// Clone the toolbarLayout for immutability before making changes
		const updatedLayout = this.#value.toolbarLayout.map((row) => row.map((group) => [...group]));

		// Add or remove the alias from toolbarLayout
		if (item.selected) {
			const lastRow = updatedLayout.at(-1);
			const lastGroup = lastRow?.at(-1);
			lastGroup?.push(item.alias);
		} else {
			updatedLayout.forEach((row) =>
				row.forEach((group, groupIndex) => {
					row[groupIndex] = group.filter((alias) => alias !== item.alias);
				}),
			);
		}

		// Update extensions based on the selection state
		const updatedExtensions = item.selected
			? [...this.#value.extensions, item.alias]
			: this.#value.extensions.filter((alias) => alias !== item.alias);

		this.#value = {
			...this.#value,
			extensions: updatedExtensions,
		};
		this.#toolbarLayout.setValue(updatedLayout);
		this.requestUpdate('_extensionCategories');
	}

	override render() {
		return html`
		<umb-tiptap-toolbar-groups-configuration .availableExtensions=${this._availableExtensions}></umb-tiptap-toolbar-groups-configuration>
			<div class="extensions">
				${repeat(
					this._extensionCategories,
					(category) => html`
						<div class="category">
							<p class="category-name">
								${category.category}
								<span style="margin-left: auto; font-size: 0.8em; opacity: 0.5;">Hide in toolbar</span>
							</p>
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
											@click=${() => this.#onExtensionSelect(item)}
											><uui-icon .svg=${tinyIconSet?.icons[item.icon ?? 'alignjustify']}></uui-icon
										></uui-button>
										<span>${item.label}</span>
										<uui-checkbox aria-label="Hide in toolbar"></uui-checkbox>
									</div>`,
							)}
						</div>
					`,
				)}
					</div>
			</div>
		`;
	}

	static override styles = [
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
				display: grid;
				grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
				gap: 16px;
				margin-top: 16px;
			}
			.extension-item {
				display: grid;
				grid-template-columns: 36px 1fr auto;
				grid-template-rows: 1fr;
				align-items: center;
				gap: 9px;
			}
			.category {
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

export default UmbPropertyEditorUiTiptapToolbarConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiptap-toolbar-configuration': UmbPropertyEditorUiTiptapToolbarConfigurationElement;
	}
}
