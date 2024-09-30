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

type ExtensionGroupItem = {
	alias: string;
	label: string;
	icon?: string;
	selected: boolean;
};

type ExtensionGroup = {
	group: string;
	extensions: ExtensionGroupItem[];
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
	private _extensionCategories: ExtensionGroup[] = [];

	@state()
	private _extensionConfigs: ExtensionConfig[] = [];

	protected override async firstUpdated(_changedProperties: PropertyValueMap<unknown>) {
		super.firstUpdated(_changedProperties);

		this.observe(umbExtensionsRegistry.byType('tiptapExtension'), (extensions) => {
			this._extensionConfigs = extensions
				.sort((a, b) => a.alias.localeCompare(b.alias))
				.map((ext) => {
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

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-expect-error
		const grouped = Object.groupBy(withSelectedProperty, (item: ExtensionConfig) => item.group || 'Uncategorized');

		this._extensionCategories = Object.keys(grouped)
			.sort((a, b) => a.localeCompare(b))
			.map((key) => ({
				group: key,
				extensions: grouped[key],
			}));
	}

	#onExtensionClick(item: ExtensionGroupItem) {
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
					(group) => html`
						<div class="group">
							<p class="group-name">${this.localize.string(group.group)}</p>
							${repeat(
								group.extensions,
								(item) => html`
									<div class="extension-item">
										<uui-button
											compact
											class=${item.selected ? 'selected' : ''}
											label=${this.localize.string(item.label)}
											look="outline"
											.value=${item.alias}
											@click=${() => this.#onExtensionClick(item)}>
											<umb-icon name=${item.icon ?? ''}></umb-icon>
										</uui-button>
										<uui-button
											compact
											label=${this.localize.string(item.label)}
											look="default"
											style="--uui-button-content-align:left;"
											@click=${() => this.#onExtensionClick(item)}></uui-button>
									</div>
								`,
							)}
						</div>
					`,
				)}
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

			.group {
				flex: 1;
				display: flex;
				flex-direction: column;
				gap: 6px;
				padding: 12px;
				background-color: var(--uui-color-surface-alt);
				border: 1px solid var(--uui-color-border);
				border-radius: 6px;
			}

			.group-name {
				grid-column: 1 / -1;
				display: flex;
				font-weight: bold;
				margin: 0;
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
