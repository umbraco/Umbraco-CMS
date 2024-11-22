import { html, customElement, property, css } from '@umbraco-cms/backoffice/external/lit';
import {
	type UmbPropertyEditorUiElement,
	UmbPropertyValueChangeEvent,
	type UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbBlockGridTypeGroupType } from '@umbraco-cms/backoffice/block-grid';

@customElement('umb-property-editor-ui-block-grid-group-configuration')
export class UmbPropertyEditorUIBlockGridGroupConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	private _value: Array<UmbBlockGridTypeGroupType> = [];

	@property({ type: Array })
	public set value(value: Array<UmbBlockGridTypeGroupType>) {
		this._value = value || [];
	}
	public get value(): Array<UmbBlockGridTypeGroupType> {
		return this._value;
	}

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {}

	#addGroup() {
		this.value = [...this._value, { name: 'Unnamed group', key: UmbId.new() }];
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		return html`
			<uui-button label=${this.localize.term('blockEditor_addBlockGroup')} look="placeholder" @click=${this.#addGroup}>
				${this.localize.term('blockEditor_addBlockGroup')}
			</uui-button>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			uui-button {
				display: block;
			}
		`,
	];
}

export default UmbPropertyEditorUIBlockGridGroupConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-grid-group-configuration': UmbPropertyEditorUIBlockGridGroupConfigurationElement;
	}
}
