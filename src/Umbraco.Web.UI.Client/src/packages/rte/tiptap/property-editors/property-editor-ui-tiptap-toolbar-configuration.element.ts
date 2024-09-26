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
import './tiptap-toolbar-groups-configuration2.element.js';

import { tinymce } from '@umbraco-cms/backoffice/external/tinymce';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

const tinyIconSet = tinymce.IconManager.get('default');

// If an extension does not have a position, it is considered hidden in the toolbar
type TestServerValue = Array<{
	alias: string;
	position?: [number, number, number];
}>;

type ExtensionConfig = {
	alias: string;
	label: string;
	icon?: string;
	category: string;
};

type Extension = {
	alias: string;
	label: string;
	icon?: string;
	selected: boolean;
};

type ExtensionCategory = {
	category: string;
	extensions: Extension[];
};

@customElement('umb-property-editor-ui-tiptap-toolbar-configuration')
export class UmbPropertyEditorUiTiptapToolbarConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	@property({ attribute: false })
	value: TestServerValue = [];

	@property({ attribute: false })
	config?: UmbPropertyEditorConfigCollection;

	@state()
	private _extensionCategories: ExtensionCategory[] = [];

	@state()
	private _extensionConfigs: ExtensionConfig[] = [];

	protected override async firstUpdated(_changedProperties: PropertyValueMap<unknown>) {
		super.firstUpdated(_changedProperties);

		this.#setupExtensionCategories();
	}

	#setupExtensionCategories() {
		const toolbarConfigValue = this.config?.getValueByAlias<ExtensionConfig[]>('toolbar');
		if (!toolbarConfigValue) return;
		const withSelected = toolbarConfigValue.map((v) => {
			return {
				...v,
				selected: this.value.some((item) => item.alias === v.alias),
			};
		});

		const grouped = withSelected.reduce((acc: any, item) => {
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
	}

	#onExtensionSelect(item: Extension) {
		item.selected = !item.selected;

		if (item.selected) {
			this.value = [
				...this.value,
				{
					alias: item.alias,
				},
			];
		} else {
			this.value = this.value.filter((v) => v.alias !== item.alias);
		}

		this.requestUpdate('_extensionCategories');
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	#onChange(event: CustomEvent) {
		this.value = event.target.value;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		return html`
		<umb-tiptap-toolbar-groups-configuration2 @change=${this.#onChange} .value=${this.value}></umb-tiptap-toolbar-groups-configuration2>
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
											@click=${() => this.#onExtensionSelect(item)}
											><uui-icon .svg=${tinyIconSet?.icons[item.icon ?? 'alignjustify']}></uui-icon
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

export default UmbPropertyEditorUiTiptapToolbarConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiptap-toolbar-configuration': UmbPropertyEditorUiTiptapToolbarConfigurationElement;
	}
}
