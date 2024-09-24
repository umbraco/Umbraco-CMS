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

const tinyIconSet = tinymce.IconManager.get('default');

type EditorExtension = {
	alias: string;
	label: string;
	icon?: string;
	hideInToolbar: boolean;
	row: number;
	group: [number, number];
};

type EditorExtensionValue = {
	alias: string;
	hideInToolbar: boolean;
	position: [number, number];
};

type ExtensionConfig = {
	alias: string;
	label: string;
	icon?: string;
	selected: boolean;
	category: string;
};

type ExtensionCategory = Array<{
	category: string;
	extensions: ExtensionConfig[];
}>;

@customElement('umb-property-editor-ui-tiptap-toolbar-configuration')
export class UmbPropertyEditorUiTiptapToolbarConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	@property({ attribute: false })
	set value(value: Array<EditorExtensionValue>) {
		if (!value) value = [];

		this.#value = value;
		this.requestUpdate('#value');
	}
	get value(): Array<EditorExtensionValue> {
		return this.#value;
	}

	#value: Array<EditorExtensionValue> = [];

	@property({ attribute: false })
	config?: UmbPropertyEditorConfigCollection;

	@state()
	private _extensionCategories: ExtensionCategory = [];

	@state()
	private _extensionsConfig: Array<ExtensionConfig> = [];

	protected override async firstUpdated(_changedProperties: PropertyValueMap<unknown>) {
		super.firstUpdated(_changedProperties);

		const toolbarConfig = this.config?.getValueByAlias<ExtensionConfig[]>('toolbar');
		if (!toolbarConfig) return;

		const extensions: Array<ExtensionConfig> = [];

		toolbarConfig.forEach((v) => {
			extensions.push({
				...v,
				selected: this.value.some((x) => x.alias == v.alias),
			});
		});

		const grouped = extensions.reduce((acc: any, item) => {
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
	}

	#onExtensionSelect(item: ExtensionConfig) {
		item.selected = !item.selected;

		if (item.selected) {
			this.value = [...this.value, { alias: item.alias, hideInToolbar: false, position: [0, 0] }];
		} else {
			this.value = this.value.filter((x) => x.alias !== item.alias);
		}

		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	#test() {
		this.value = [
			{
				alias: 'bold',
				hideInToolbar: false,
				position: [0, 0],
			},
			{
				alias: 'italic',
				hideInToolbar: false,
				position: [0, 0],
			},
			{
				alias: 'underline',
				hideInToolbar: false,
				position: [0, 0],
			},
		];
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		return html`
		<button @click=${this.#test}>Test</button>
		<umb-tiptap-toolbar-groups-configuration></umb-tiptap-toolbar-groups-configuration>
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
