import type { UmbBlockGridTypeAreaTypePermission } from '../../index.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { html, customElement, property, css, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import { UMB_DATA_TYPE_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/data-type';

@customElement('umb-property-editor-ui-block-grid-area-type-permission')
export class UmbPropertyEditorUIBlockGridAreaTypePermissionElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	@property({ type: Array })
	public get value(): Array<UmbBlockGridTypeAreaTypePermission> {
		return this._value;
	}
	public set value(value: Array<UmbBlockGridTypeAreaTypePermission>) {
		this._value = value ?? [];
	}
	@state()
	private _value: Array<UmbBlockGridTypeAreaTypePermission> = [];

	constructor() {
		super();

		this.consumeContext(UMB_DATA_TYPE_WORKSPACE_CONTEXT, async (context) => {
			this.observe(
				await context.propertyValueByAlias('blocks'),
				(blockTypes) => {
					console.log('blockTypes', blockTypes);
				},
				'observeBlockType',
			);
			this.observe(
				await context.propertyValueByAlias('blockGroups'),
				(blockGroups) => {
					console.log('blockGroups', blockGroups);
				},
				'observeBlockGroups',
			);
		}).passContextAliasMatches();
	}

	#addNewPermission() {
		this.value = [...this.value, { minAllowed: 0, maxAllowed: undefined }];
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	// TODO: Needs localizations:
	render() {
		return html`<div>
				${repeat(
					this.value,
					(area) => area,
					(area) => html`<span>Missing component for a value entry</span>`,
				)}
			</div>
			<uui-button
				id="add-button"
				look="placeholder"
				label=${'Add permission'}
				@click=${this.#addNewPermission}></uui-button>`;
	}

	static styles = [UmbTextStyles, css``];
}

export default UmbPropertyEditorUIBlockGridAreaTypePermissionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-grid-area-type-permission': UmbPropertyEditorUIBlockGridAreaTypePermissionElement;
	}
}
