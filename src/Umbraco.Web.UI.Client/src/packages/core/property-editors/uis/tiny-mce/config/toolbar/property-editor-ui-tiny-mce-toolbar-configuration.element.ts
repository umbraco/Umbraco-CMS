import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { customElement, css, html, property, map, state, PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbPropertyEditorExtensionElement, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbDataTypePropertyCollection } from '@umbraco-cms/backoffice/components';

type ToolbarConfig = {
	alias: string;
	label: string;
	icon: string;
	selected: boolean;
};

/**
 * @element umb-property-editor-ui-tiny-mce-toolbar-configuration
 */
@customElement('umb-property-editor-ui-tiny-mce-toolbar-configuration')
export class UmbPropertyEditorUITinyMceToolbarConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorExtensionElement
{
	@property()
	value: string[] = [];

	@property({ type: Array, attribute: false })
	config?: UmbDataTypePropertyCollection;

	@state()
	private _toolbarConfig: ToolbarConfig[] = [];

	async firstUpdated(_changedProperties: PropertyValueMap<unknown>) {
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

	async getToolbarPlugins(): Promise<void> {
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

	render() {
		return html`<ul>
			${map(
				this._toolbarConfig,
				(v) => html`<li>
					<uui-checkbox value=${v.alias} ?checked=${v.selected}>
						<uui-icon name=${v.icon}></uui-icon>
						${v.label}
					</uui-checkbox>
				</li>`
			)}
		</ul>`;
	}

	static styles = [
		UUITextStyles,
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
