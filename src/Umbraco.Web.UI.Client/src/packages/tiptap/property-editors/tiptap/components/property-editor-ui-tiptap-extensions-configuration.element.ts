import {
	customElement,
	css,
	html,
	ifDefined,
	nothing,
	property,
	state,
	repeat,
	when,
} from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';

type UmbTiptapExtension = {
	alias: string;
	label: string;
	icon?: string;
	group?: string;
	description?: string;
};

type UmbTiptapExtensionGroupItem = UmbTiptapExtension & {
	selected: boolean;
};

type UmbTiptapExtensionGroup = {
	group: string;
	extensions: Array<UmbTiptapExtensionGroupItem>;
};

const TIPTAP_CORE_EXTENSION_ALIAS = 'Umb.Tiptap.RichTextEssentials';
const TIPTAP_BLOCK_EXTENSION_ALIAS = 'Umb.Tiptap.Block';

const elementName = 'umb-property-editor-ui-tiptap-extensions-configuration';

@customElement(elementName)
export class UmbPropertyEditorUiTiptapExtensionsConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	#disabledExtensions = new Set<string>([TIPTAP_CORE_EXTENSION_ALIAS]);

	@property({ attribute: false })
	value?: Array<string> = [TIPTAP_CORE_EXTENSION_ALIAS];

	@property({ attribute: false })
	config?: UmbPropertyEditorConfigCollection;

	@state()
	private _extensions: Array<UmbTiptapExtension> = [];

	@state()
	private _groups: Array<UmbTiptapExtensionGroup> = [];

	constructor() {
		super();
		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, async (dataset) => {
			this.observe(
				await dataset.propertyValueByAlias<Array<unknown>>('blocks'),
				(blocks) => {
					const tmpValue = this.value ? [...this.value] : [];

					// When blocks are configured, the block extension can be enabled;
					// otherwise, the block extension must be disabled.
					if (blocks?.length) {
						// Check if the block extension is already enabled, if not, add it.
						if (!tmpValue.includes(TIPTAP_BLOCK_EXTENSION_ALIAS)) {
							tmpValue.push(TIPTAP_BLOCK_EXTENSION_ALIAS);
						}
						this.#disabledExtensions.delete(TIPTAP_BLOCK_EXTENSION_ALIAS);
					} else {
						// Check if the block extension is enabled, if so, remove it.
						const idx = tmpValue.indexOf(TIPTAP_BLOCK_EXTENSION_ALIAS) ?? -1;
						if (idx >= 0) {
							tmpValue.splice(idx, 1);
						}
						this.#disabledExtensions.add(TIPTAP_BLOCK_EXTENSION_ALIAS);
					}

					if (!this.value || !this.#isArrayEqualTo(tmpValue, this.value)) {
						this.#setValue(tmpValue);
						this.#syncViewModel();
					}
				},
				'_observeBlocks',
			);
		});
	}

	protected override async firstUpdated(_changedProperties: PropertyValueMap<unknown>) {
		super.firstUpdated(_changedProperties);

		this.observe(umbExtensionsRegistry.byType('tiptapExtension'), (extensions) => {
			this._extensions = extensions
				.sort((a, b) => a.alias.localeCompare(b.alias))
				.map((ext) => ({ alias: ext.alias, label: ext.meta.label, icon: ext.meta.icon, group: ext.meta.group }));

			// Hardcoded core extension
			this._extensions.unshift({
				alias: TIPTAP_CORE_EXTENSION_ALIAS,
				label: 'Rich Text Essentials',
				icon: 'icon-browser-window',
				group: '#tiptap_extGroup_formatting',
				description: 'This is a core extension, it must be enabled',
			});

			if (!this.value) {
				// The default value is all extensions enabled
				this.#setValue(this._extensions.map((ext) => ext.alias));
			}

			this.#syncViewModel();
		});
	}

	#isArrayEqualTo(a: Array<string>, b: Array<string>) {
		return a.length === b.length && a.every((item) => b.includes(item)) && b.every((item) => a.includes(item));
	}

	#onClick(item: UmbTiptapExtensionGroupItem) {
		item.selected = !item.selected;

		const tmpValue = item.selected
			? [...(this.value ?? []), item.alias]
			: (this.value ?? []).filter((alias) => alias !== item.alias);

		this.#setValue(tmpValue);
	}

	#setValue(value: Array<string>) {
		this.value = value;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	#syncViewModel() {
		const items: Array<UmbTiptapExtensionGroupItem> = this._extensions.map((extension) => ({
			...extension,
			selected: this.value!.includes(extension.alias) || extension.alias === TIPTAP_CORE_EXTENSION_ALIAS,
		}));

		const uncategorizedLabel = this.localize.term('tiptap_extGroup_unknown');

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-expect-error
		const grouped = Object.groupBy(items, (item: UmbTiptapExtensionGroupItem) => item.group || uncategorizedLabel);

		this._groups = Object.keys(grouped)
			.sort((a, b) => a.localeCompare(b))
			.map((key) => ({ group: key, extensions: grouped[key] }));
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
							${repeat(
								group.extensions,
								(item) => html`
									<li title=${ifDefined(item.description)}>
										<uui-checkbox
											label=${this.localize.string(item.label)}
											value=${item.alias}
											?checked=${item.selected}
											?disabled=${this.#disabledExtensions.has(item.alias)}
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
