import type { UmbBlockGridTypeAreaType } from '../../index.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { html, customElement, property, css, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-property-editor-ui-block-grid-areas-config')
export class UmbPropertyEditorUIBlockGridAreasConfigElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	// local vars:
	#defaultAreaGridColumns: number = 12;
	#valueOfAreaGridColumns?: number | null;

	@property({ type: Array })
	value: Array<UmbBlockGridTypeAreaType> = [];

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		const defaultAreaGridColumns = config?.getValueByAlias('defaultAreaGridColumns');
		if (typeof defaultAreaGridColumns === 'number' && defaultAreaGridColumns > 0) {
			this.#defaultAreaGridColumns = defaultAreaGridColumns ?? null;
		} else {
			this.#defaultAreaGridColumns = 12;
		}
		this.#gotAreaColumns();
	}

	@state()
	_areaGridColumns?: number;

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, async (context) => {
			this.observe(await context.propertyValueByAlias('areaGridColumns'), (value) => {
				// Value can be undefined, but 'undefined > 0' is still valid JS and will return false. [NL]
				this.#valueOfAreaGridColumns = (value as number) > 0 ? (value as number) : null;
				this.#gotAreaColumns();
			});
		});
	}

	#gotAreaColumns() {
		if (!this.#defaultAreaGridColumns || this.#valueOfAreaGridColumns === undefined) return;
		this._areaGridColumns = this.#valueOfAreaGridColumns ?? this.#defaultAreaGridColumns;
	}

	render() {
		return this._areaGridColumns ? html`<div id="wrapper">Hello columns: ${this._areaGridColumns}</div>` : '';
	}

	static styles = [UmbTextStyles, css``];
}

export default UmbPropertyEditorUIBlockGridAreasConfigElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-grid-areas-config': UmbPropertyEditorUIBlockGridAreasConfigElement;
	}
}
