import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbStylesheetRepository } from '@umbraco-cms/backoffice/stylesheet';
import { StylesheetOverviewResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';

/**
 * @element umb-property-editor-ui-tiny-mce-stylesheets-configuration
 */
@customElement('umb-property-editor-ui-tiny-mce-stylesheets-configuration')
export class UmbPropertyEditorUITinyMceStylesheetsConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	@property({ type: Array })
	value: string[] = [];

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	@state()
	stylesheetList: Array<StylesheetOverviewResponseModel & Partial<{ selected: boolean }>> = [];

	#repository;

	constructor() {
		super();
		this.#repository = new UmbStylesheetRepository(this);

		this.#getAllStylesheets();
	}
	async #getAllStylesheets() {
		const { data } = await this.#repository.getAll();
		if (!data) return;

		const styles = data.items;

		this.stylesheetList = styles.map((stylesheet) => ({
			...stylesheet,
			selected: this.value?.some((path) => path === stylesheet.path),
		}));
	}

	#onChange(event: CustomEvent) {
		const checkbox = event.target as HTMLInputElement;

		if (checkbox.checked) {
			if (this.value) {
				this.value = [...this.value, checkbox.value];
			} else {
				this.value = [checkbox.value];
			}
		} else {
			this.value = this.value.filter((v) => v !== checkbox.value);
		}

		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<ul>
			${this.stylesheetList.map(
				(stylesheet) =>
					html`<li>
						<uui-checkbox
							.label=${stylesheet.name}
							.value=${stylesheet.path ?? ''}
							@change=${this.#onChange}
							?checked=${stylesheet.selected}>
							${stylesheet.name}
						</uui-checkbox>
					</li>`,
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

export default UmbPropertyEditorUITinyMceStylesheetsConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiny-mce-stylesheets-configuration': UmbPropertyEditorUITinyMceStylesheetsConfigurationElement;
	}
}
