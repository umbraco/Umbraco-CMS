import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { customElement, css, html, property, map, state, PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbPropertyEditorUiElement, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { tinymce } from '@umbraco-cms/backoffice/external/tinymce';

const tinyIconSet = tinymce.default?.IconManager.get('default');

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
	@property()
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
	private _toolbarConfig: ToolbarConfig[] = [];

	#selectedValues: string[] = [];

	protected async firstUpdated(_changedProperties: PropertyValueMap<unknown>) {
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
		const plugin$ = umbExtensionsRegistry.extensionsOfType('tinyMcePlugin');

		const plugins = await firstValueFrom(plugin$);

		plugins.forEach((p) => {
			// If the plugin has a toolbar, add it to the config
			if (p.meta?.toolbar) {
				p.meta.toolbar.forEach((t) => {
					this._toolbarConfig.push({
						alias: t.alias,
						label: t.label,
						icon: t.icon ?? 'umb:autofill',
						selected: this.value.includes(t.alias),
					});
				});
			}
		});
	}

	private onChange(event: CustomEvent) {
		const checkbox = event.target as HTMLInputElement;
		const alias = checkbox.value;

		if (checkbox.checked) {
			this.value = [...this.value, alias];
		} else {
			this.value = this.value.filter((v) => v !== alias);
		}

		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<ul>
			${map(
				this._toolbarConfig,
				(v) => html`<li>
					<uui-checkbox value=${v.alias} ?checked=${v.selected} @change=${this.onChange}>
						<uui-icon .svg=${tinyIconSet?.icons[v.icon ?? 'alignjustify']}></uui-icon>
						${v.label}
					</uui-checkbox>
				</li>`
			)}
		</ul>`;
	}

	static styles = [
		UmbTextStyles,
		css`
			ul {
				list-style: none;
				padding: 0;
				margin: 0;
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
