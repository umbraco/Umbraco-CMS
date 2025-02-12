import { css, customElement, html, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { tinymce } from '@umbraco-cms/backoffice/external/tinymce';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import type {
	UmbPropertyEditorUiElement,
	UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';

const tinyIconSet = tinymce.IconManager.get('default');

type ToolbarConfig = {
	alias: string;
	label: string;
	icon?: string;
	selected: boolean;
};

/**
 * @element umb-property-editor-ui-tiny-mce-toolbar-configuration
 */
@customElement('umb-property-editor-ui-tiny-mce-toolbar-configuration')
export class UmbPropertyEditorUITinyMceToolbarConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	@property({ attribute: false })
	set value(value: string | string[] | null) {
		if (!value) return;

		if (typeof value === 'string') {
			this.#selectedValues = value.split(',').filter((x) => x.length > 0);
		} else if (Array.isArray(value)) {
			this.#selectedValues = value;
		} else {
			this.#selectedValues = [];
			return;
		}

		// Migrations
		if (this.#selectedValues.includes('ace')) {
			this.#selectedValues = this.#selectedValues.filter((v) => v !== 'ace');
			this.#selectedValues.push('sourcecode');
		}

		this._toolbarConfig.forEach((v) => {
			v.selected = this.#selectedValues.includes(v.alias);
		});
	}
	get value(): string[] {
		return this.#selectedValues;
	}

	@property({ attribute: false })
	config?: UmbPropertyEditorConfigCollection;

	@state()
	private readonly _toolbarConfig: ToolbarConfig[] = [];

	#selectedValues: string[] = [];

	protected override async firstUpdated(_changedProperties: PropertyValueMap<unknown>) {
		super.firstUpdated(_changedProperties);

		this.config?.getValueByAlias<ToolbarConfig[]>('toolbar')?.forEach((v) => {
			this._toolbarConfig.push({
				...v,
				selected: this.value.includes(v.alias),
			});
		});

		await this.getToolbarPlugins();

		this.requestUpdate('_toolbarConfig');
	}

	private async getToolbarPlugins(): Promise<void> {
		// Get all the toolbar plugins
		const plugin$ = umbExtensionsRegistry.byType('tinyMcePlugin');

		const plugins = await firstValueFrom(plugin$);

		plugins.forEach((p) => {
			// If the plugin has a toolbar, add it to the config
			if (p.meta?.toolbar) {
				p.meta.toolbar.forEach((t: any) => {
					this._toolbarConfig.push({
						alias: t.alias,
						label: this.localize.string(t.label),
						icon: t.icon ?? 'icon-autofill',
						selected: this.value.includes(t.alias),
					});
				});
			}
		});
	}

	private onChange(event: CustomEvent) {
		const checkbox = event.target as HTMLInputElement;
		const alias = checkbox.value;

		const value = this._toolbarConfig
			.filter((t) => (t.alias !== alias && t.selected) || (t.alias === alias && checkbox.checked))
			.map((v) => v.alias);

		this.value = value;

		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		return html`<ul>
			${repeat(
				this._toolbarConfig,
				(v) => v.alias,
				(v) =>
					html`<li>
						<uui-checkbox label=${v.label} value=${v.alias} ?checked=${v.selected} @change=${this.onChange}>
							<uui-icon .svg=${tinyIconSet?.icons[v.icon ?? 'alignjustify']}></uui-icon>
							${v.label}
						</uui-checkbox>
					</li>`,
			)}
		</ul>`;
	}

	static override readonly styles = [
		UmbTextStyles,
		css`
			ul {
				list-style: none;
				padding: 0;
				margin: 0;

				uui-icon {
					width: 1.5em;
					height: 1.5em;
					margin-right: 5px;
				}
			}
		`,
	];
}

export default UmbPropertyEditorUITinyMceToolbarConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiny-mce-toolbar-configuration': UmbPropertyEditorUITinyMceToolbarConfigurationElement;
	}
}
