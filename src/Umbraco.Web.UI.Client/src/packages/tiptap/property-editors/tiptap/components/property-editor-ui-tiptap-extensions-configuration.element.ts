import { customElement, css, html, property, state, repeat, when, nothing } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';

type UmbTiptapExtension = {
	alias: string;
	label: string;
	icon?: string;
	group?: string;
};

type UmbTiptapExtensionGroupItem = UmbTiptapExtension & {
	selected: boolean;
};

type UmbTiptapExtensionGroup = {
	group: string;
	extensions: Array<UmbTiptapExtensionGroupItem>;
};

const elementName = 'umb-property-editor-ui-tiptap-extensions-configuration';

@customElement(elementName)
export class UmbPropertyEditorUiTiptapExtensionsConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	@property({ attribute: false })
	value?: Array<string> = [];

	@property({ attribute: false })
	config?: UmbPropertyEditorConfigCollection;

	@state()
	private _extensions: Array<UmbTiptapExtension> = [];

	@state()
	private _groups: Array<UmbTiptapExtensionGroup> = [];

	protected override async firstUpdated(_changedProperties: PropertyValueMap<unknown>) {
		super.firstUpdated(_changedProperties);

		this.observe(umbExtensionsRegistry.byType('tiptapExtension'), (extensions) => {
			this._extensions = extensions
				.sort((a, b) => a.alias.localeCompare(b.alias))
				.map((ext) => ({ alias: ext.alias, label: ext.meta.label, icon: ext.meta.icon, group: ext.meta.group }));

			if (!this.value) {
				// The default value is all extensions enabled
				this.value = this._extensions.map((ext) => ext.alias);
				this.dispatchEvent(new UmbPropertyValueChangeEvent());
			}

			const items: Array<UmbTiptapExtensionGroupItem> = this._extensions.map((extension) => ({
				...extension,
				selected: this.value!.includes(extension.alias),
			}));

			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-expect-error
			const grouped = Object.groupBy(items, (item: UmbTiptapExtensionGroupItem) => item.group || 'Uncategorized');

			this._groups = Object.keys(grouped)
				.sort((a, b) => a.localeCompare(b))
				.map((key) => ({ group: key, extensions: grouped[key] }));
		});
	}

	#onClick(item: UmbTiptapExtensionGroupItem) {
		item.selected = !item.selected;

		if (!this.value) {
			this.value = [];
		}

		if (item.selected) {
			this.value = [...this.value, item.alias];
		} else {
			this.value = this.value.filter((alias) => alias !== item.alias);
		}

		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		if (!this._groups.length) return nothing;
		return html`
			${repeat(
				this._groups,
				(group) => html`
					<div class="group">
						<uui-label>${this.localize.string(group.group)}</uui-label>
						<ul>
							${when(
								group.group === '#tiptap_extGroup_formatting',
								() => html`
									<li title="This is a core extension, it must be enabled">
										<uui-checkbox checked disabled label="Rich Text Essentials">
											<div class="inner">
												<umb-icon name="icon-browser-window"></umb-icon>
												<span>Rich Text Essentials</span>
											</div>
										</uui-checkbox>
									</li>
								`,
							)}
							${repeat(
								group.extensions,
								(item) => html`
									<li>
										<uui-checkbox
											label=${this.localize.string(item.label)}
											value=${item.alias}
											?checked=${item.selected}
											@change=${() => this.#onClick(item)}>
											<div class="inner">
												${when(item.icon, () => html`<umb-icon .name=${item.icon}></umb-icon>`)}
												<span>${this.localize.string(item.label)}</span>
											</div>
										</uui-checkbox>
									</li>
								`,
							)}
						</ul>
					</div>
				`,
			)}
		`;
	}

	static override readonly styles = [
		css`
			:host {
				display: flex;
				flex-wrap: wrap;
				gap: 1rem;
			}

			.group {
				flex: 1;

				ul {
					list-style: none;
					padding: 0;
					margin: 1rem 0 0;

					.inner {
						display: flex;
						flex-direction: row;
						gap: 0.5rem;

						umb-icon {
							font-size: 1.2rem;
						}
					}
				}
			}
		`,
	];
}

export { UmbPropertyEditorUiTiptapExtensionsConfigurationElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbPropertyEditorUiTiptapExtensionsConfigurationElement;
	}
}
